using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
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
        const string invoiceView = "AccountsReceivable.Invoices.View";
        const string paymentView = "AccountsReceivable.CustomerPayments.View";
        var permissions = new Dictionary<string, List<string>>
        {
            ["viewer"] = [invoiceView, paymentView],
            ["creator"] = [invoiceView, "AccountsReceivable.Invoices.Create"],
            ["poster"] = [invoiceView, "AccountsReceivable.Invoices.Post"],
            ["reverser"] = [paymentView, "AccountsReceivable.CustomerPayments.Post"]
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

        await using var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers().RequireAuthorization();
        await app.StartAsync();
        using var client = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };

        async Task Expect(string route, string? actor, HttpStatusCode expected)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, route);
            if (route == invoice)
                request.Content = JsonContent.Create(new FreeTextInvoiceSaveDto { CustomerAccount = "C-01" });
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
