using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using IAX.IXApi.Modules.Finance.AccountsReceivable.CollectionLetters;
using IAX.IXApi.Modules.Finance.AccountsReceivable.CurrencyRevaluation;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Settlement;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace IXApi.Tests;

public sealed class OrderToCashAuthorizationTests
{
    [Fact]
    public async Task Invoice_create_post_and_settlement_reversal_enforce_distinct_permissions_over_http()
    {
        const string issuer = "otc-acceptance";
        const string audience = "otc-api";
        const string invoice = "/api/v1/CustInvoiceTable";
        const string reversal = "/api/v1/customer-settlements/SET-01/reverse";
        const string revaluation = "/api/v1/CustCurrencyRevaluation/42";
        const string letters = "/api/v1/CustCollectionLetter";
        const string invoiceView = "AccountsReceivable.Invoices.View";
        const string paymentView = "AccountsReceivable.CustomerPayments.View";
        var permissions = new Dictionary<string, List<string>>
        {
            ["viewer"] = [invoiceView, paymentView],
            ["creator"] = [invoiceView, "AccountsReceivable.Invoices.Create"],
            ["poster"] = [invoiceView, "AccountsReceivable.Invoices.Post"],
            ["reverser"] = [paymentView, "AccountsReceivable.CustomerPayments.Post"],
            ["fx-viewer"] = ["AccountsReceivable.Transactions.View"],
            ["fx-poster"] = ["AccountsReceivable.CurrencyRevaluation.Post"],
            ["letter-creator"] = ["AccountsReceivable.CollectionLetters.Create"],
            ["letter-poster"] = ["AccountsReceivable.CollectionLetters.Post"]
        };
        var signingKey = new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32));
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = signingKey, ValidateIssuerSigningKey = true,
                ValidIssuer = issuer, ValidAudience = audience, ValidateLifetime = true
            };
        });
        builder.Services.AddAuthorization();
        builder.Services.AddControllers().AddApplicationPart(typeof(FreeTextInvoiceController).Assembly);
        builder.Services.AddSingleton(Stub<IAppPermissionService>(method =>
            method.Name == nameof(IAppPermissionService.GetPermissionKeysByUserAsync)
                ? arguments => Task.FromResult(permissions[(string)arguments[0]!])
                : throw new NotSupportedException(method.Name)));
        var creates = 0;
        var posts = 0;
        var reversals = 0;
        builder.Services.AddSingleton(Stub<IFreeTextInvoiceService>(method => method.Name switch
        {
            nameof(IFreeTextInvoiceService.CreateAsync) => _ =>
            {
                Interlocked.Increment(ref creates);
                return Task.FromResult(new FreeTextInvoiceResult(201, null));
            },
            nameof(IFreeTextInvoiceService.PostAsync) => _ =>
            {
                Interlocked.Increment(ref posts);
                return Task.FromResult(new FreeTextInvoiceResult(200, null));
            },
            _ => throw new NotSupportedException(method.Name)
        }));
        builder.Services.AddSingleton(Stub<ICustSettlementReversalService>(method =>
            method.Name == nameof(ICustSettlementReversalService.ReverseAsync)
                ? _ =>
                {
                    Interlocked.Increment(ref reversals);
                    return Task.FromResult(new CustPaymentResult(200, new { voucher = "SET-01" }));
                }
                : throw new NotSupportedException(method.Name)));
        builder.Services.AddSingleton(Stub<ICustSettleOpenTransactionsService>(method => throw new NotSupportedException(method.Name)));
        var fxCalls = new List<string>();
        var date = new DateOnly(2026, 5, 20);
        var letterCalls = new List<string>();
        builder.Services.AddSingleton(Stub<ICustCollectionLetterService>(method => arguments =>
        {
            letterCalls.Add(method.Name);
            if (method.Name == nameof(ICustCollectionLetterService.GenerateFirstLettersAsync))
            {
                Assert.Equal("C-01", arguments[0]);
                Assert.Equal("USD", arguments[1]);
                Assert.Equal(date, arguments[2]);
                return Task.FromResult<IReadOnlyList<string>>(["CL-01"]);
            }
            Assert.Equal(nameof(ICustCollectionLetterService.PostWarningAsync), method.Name);
            Assert.Equal("CL-01", arguments[0]);
            Assert.Equal(date, arguments[1]);
            return Task.FromResult(true);
        }));
        builder.Services.AddSingleton(Stub<ICustCurrencyRevaluationService>(method => arguments =>
        {
            Assert.Equal(42L, Assert.IsType<long>(arguments[0]));
            Assert.Equal(date, Assert.IsType<DateOnly>(arguments[1]));
            fxCalls.Add(method.Name);
            return method.Name switch
            {
                nameof(ICustCurrencyRevaluationService.PreviewAsync) => Task.FromResult<CustRevaluationPreview?>(
                    new(42, "USD", 1000m, "SAR", 3750m, 3800m, "SAR", 3750m, 3800m)),
                nameof(ICustCurrencyRevaluationService.PostAsync) => Task.FromResult<string?>("FX-01"),
                nameof(ICustCurrencyRevaluationService.ReverseAsync) => Task.FromResult<string?>("FX-02"),
                _ => throw new NotSupportedException(method.Name)
            };
        }));

        await using var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers().RequireAuthorization();
        await app.StartAsync();
        using var client = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };

        async Task Expect(string route, string? actor, HttpStatusCode expected, HttpMethod? method = null, object? body = null)
        {
            using var request = new HttpRequestMessage(method ?? HttpMethod.Post, route);
            if (route == invoice)
                request.Content = JsonContent.Create(new FreeTextInvoiceSaveDto { CustomerAccount = "C-01" });
            if (body != null)
                request.Content = JsonContent.Create(body);
            if (actor != null)
            {
                var token = new JwtSecurityToken(issuer, audience, [new Claim(JwtRegisteredClaimNames.Sub, actor)],
                    expires: DateTime.UtcNow.AddMinutes(5), signingCredentials: new(signingKey, SecurityAlgorithms.HmacSha256));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(token));
            }
            using var response = await client.SendAsync(request);
            Assert.Equal(expected, response.StatusCode);
        }

        await Expect(invoice, null, HttpStatusCode.Unauthorized);
        await Expect(invoice, "viewer", HttpStatusCode.Forbidden);
        await Expect(invoice + "/1/post", "creator", HttpStatusCode.Forbidden);
        await Expect(reversal, "poster", HttpStatusCode.Forbidden);
        Assert.Equal((0, 0, 0), (creates, posts, reversals));
        await Expect(invoice, "creator", HttpStatusCode.Created);
        await Expect(invoice + "/1/post", "poster", HttpStatusCode.OK);
        await Expect(reversal, "reverser", HttpStatusCode.OK);
        Assert.Equal((1, 1, 1), (creates, posts, reversals));
        var preview = revaluation + "/preview?rateDate=2026-05-20";
        var postRequest = new CustRevaluationPostRequest(date, date);
        var reverseRequest = new CustRevaluationReverseRequest(date);
        await Expect(preview, null, HttpStatusCode.Unauthorized, HttpMethod.Get);
        await Expect(preview, "fx-poster", HttpStatusCode.Forbidden, HttpMethod.Get);
        await Expect(revaluation + "/post", null, HttpStatusCode.Unauthorized, body: postRequest);
        await Expect(revaluation + "/post", "fx-viewer", HttpStatusCode.Forbidden, body: postRequest);
        await Expect(revaluation + "/reverse", "fx-viewer", HttpStatusCode.Forbidden, body: reverseRequest);
        Assert.Empty(fxCalls);
        await Expect(preview, "fx-viewer", HttpStatusCode.OK, HttpMethod.Get);
        await Expect(revaluation + "/post", "fx-poster", HttpStatusCode.OK, body: postRequest);
        await Expect(revaluation + "/reverse", "fx-poster", HttpStatusCode.OK, body: reverseRequest);
        Assert.Equal(new[] { "PreviewAsync", "PostAsync", "ReverseAsync" }, fxCalls);
        var generateLetter = new CustCollectionLetterGenerateRequest("C-01", "USD", date);
        var postLetter = new CustCollectionLetterPostRequest(date);
        await Expect(letters + "/generate", null, HttpStatusCode.Unauthorized, body: generateLetter);
        await Expect(letters + "/generate", "letter-poster", HttpStatusCode.Forbidden, body: generateLetter);
        await Expect(letters + "/CL-01/post", "letter-creator", HttpStatusCode.Forbidden, body: postLetter);
        Assert.Empty(letterCalls);
        await Expect(letters + "/generate", "letter-creator", HttpStatusCode.OK, body: generateLetter);
        await Expect(letters + "/CL-01/post", "letter-poster", HttpStatusCode.OK, body: postLetter);
        Assert.Equal(new[] { "GenerateFirstLettersAsync", "PostWarningAsync" }, letterCalls);
    }

    private static T Stub<T>(Func<MethodInfo, Func<object?[], object?>> resolve) where T : class
    {
        var proxy = DispatchProxy.Create<T, ServiceStub>();
        ((ServiceStub)(object)proxy).Resolve = resolve;
        return proxy;
    }

    public class ServiceStub : DispatchProxy
    {
        public Func<MethodInfo, Func<object?[], object?>> Resolve { get; set; } = null!;

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) =>
            Resolve(targetMethod!)(args ?? []);
    }
}
