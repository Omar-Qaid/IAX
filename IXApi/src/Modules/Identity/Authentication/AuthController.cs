using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Identity.Authentication.Authentication;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Impersonation;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IAX.IXApi.Modules.Identity.Authentication
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<AspNetUser> _signInManager;
        private readonly UserManager<AspNetUser> _userManager;
        private readonly JwtTokenService _jwtService;
        private readonly ITokenBlacklist _blacklist;
        private readonly IAppPermissionService _permissionService;
        private readonly IAuthenticationSchemeProvider _schemeProvider;

        public AuthController(
            SignInManager<AspNetUser> signInManager,
            UserManager<AspNetUser> userManager,
            JwtTokenService jwtService,
            ITokenBlacklist blacklist,
            IAppPermissionService permissionService,
            IAuthenticationSchemeProvider schemeProvider)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _jwtService = jwtService;
            _blacklist = blacklist;
            _permissionService = permissionService;
            _schemeProvider = schemeProvider;
        }


        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<APIResponse<object>>> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.Username);
            if (user == null) return Unauthorized(APIResponse<object>.Fail("Invalid username or password"));

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
            if (!result.Succeeded) return Unauthorized(APIResponse<object>.Fail("Invalid username or password"));

            // Record last login so the "Online Users" view can derive recent activity.
            user.LastLoginDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var token = await _jwtService.GenerateTokenAsync(user);
            return Ok(APIResponse<object>.Ok(new { accessToken = token }));
        }

        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<APIResponse<object>>> Register([FromBody] RegisterDto dto)
        {
            var user = new AspNetUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = dto.UserName,
                Email = dto.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(APIResponse<object>.Fail("Registration failed", result.Errors.Select(e => e.Description).ToList()));

            return Ok(APIResponse<object>.Ok(null, "User created successfully"));
        }


        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult<APIResponse<object>>> Logout()
        {
            var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            if (string.IsNullOrEmpty(jti)) return BadRequest(APIResponse<object>.Fail("No JTI found on token."));

            var expUnix = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
            if (string.IsNullOrEmpty(expUnix)) return BadRequest(APIResponse<object>.Fail("No EXP found on token."));

            var exp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expUnix));
            var ttl = exp - DateTimeOffset.UtcNow;
            if (ttl <= TimeSpan.Zero) ttl = TimeSpan.FromMinutes(1);

            await _blacklist.BlacklistAsync(jti, ttl);
            return Ok(APIResponse<object>.Ok(null, "Logged out (token revoked)."));
        }


        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<APIResponse<AspNetUserDto>>> me()
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(APIResponse<AspNetUserDto>.Fail("Unauthorized"));

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(APIResponse<AspNetUserDto>.Fail("User not found"));

            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await _permissionService.GetPermissionKeysByUserAsync(userId);

            if (roles.Contains("Admin", StringComparer.OrdinalIgnoreCase) && !permissions.Contains("*"))
                permissions.Add("*");

            var dto = user.Adapt<AspNetUserDto>();
            dto.Roles = roles.ToList();
            dto.Permissions = permissions;

            return Ok(APIResponse<AspNetUserDto>.Ok(dto));
        }

        /// <summary>
        /// Issues a brand-new JWT for the currently authenticated user with fresh
        /// role and permission claims. Call this after permissions change without
        /// requiring the user to log out and back in.
        /// </summary>
        [HttpPost("refresh-token")]
        [Authorize]
        public async Task<ActionResult<APIResponse<object>>> RefreshToken()
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(APIResponse<object>.Fail("Invalid token"));

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized(APIResponse<object>.Fail("User not found"));

            var token = await _jwtService.GenerateTokenAsync(user);
            return Ok(APIResponse<object>.Ok(new { accessToken = token }));
        }

        /// <summary>
        /// Lists the external login providers that are actually configured on the server,
        /// so the login UI can render only the buttons that will work.
        /// </summary>
        [HttpGet("external-providers")]
        [AllowAnonymous]
        public async Task<ActionResult<APIResponse<object>>> ExternalProviders()
        {
            var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
            var providers = schemes
                .Select(s => new { name = s.Name, displayName = s.DisplayName ?? s.Name })
                .ToList();

            return Ok(APIResponse<object>.Ok(providers));
        }

        [HttpGet("external-login")]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLogin([FromQuery] string provider, [FromQuery] string returnUrl = "/")
        {
            if (!Url.IsLocalUrl(returnUrl))
                return BadRequest(APIResponse<object>.Fail("The return URL must be local to this application."));

            // provider must match a registered scheme: "Microsoft" (Azure / Microsoft / hotmail / outlook) or "Google" (gmail).
            // The scheme is only registered when its ClientId is set in appsettings ("Authentication:{provider}"),
            // so fail clearly instead of throwing a 500 when the provider hasn't been configured yet.
            var scheme = await _schemeProvider.GetSchemeAsync(provider ?? "");
            if (scheme == null)
                return BadRequest(APIResponse<object>.Fail($"External provider '{provider}' is not configured on the server."));

            string? redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Auth", new { returnUrl });
            AuthenticationProperties properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            return Challenge(properties, [scheme.Name]);
        }

        [HttpGet("external-login-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback([FromQuery] string returnUrl = "/")
        {
            if (!Url.IsLocalUrl(returnUrl))
                return BadRequest(APIResponse<object>.Fail("The return URL must be local to this application."));

            ExternalLoginInfo? loginInfo = await _signInManager.GetExternalLoginInfoAsync();

            if (loginInfo == null)
            {
                return BadRequest("Error loading external login info.");
            }

            // See if user already has a login (external login) 
            Microsoft.AspNetCore.Identity.SignInResult signInResult = await _signInManager.ExternalLoginSignInAsync(loginInfo.LoginProvider, loginInfo.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            AspNetUser? user;

            if (!signInResult.Succeeded)
            {
                // Maybe user does not exist yet
                string? email = loginInfo.Principal.FindFirstValue(ClaimTypes.Email);

                if (email == null)
                {
                    return BadRequest("Email claim not received from external provider.");
                }

                user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    user = new AspNetUser
                    {
                        UserName = email,
                        Email = email
                    };

                    IdentityResult createResult = await _userManager.CreateAsync(user);

                    if (!createResult.Succeeded)
                    {
                        return BadRequest(createResult.Errors);
                    }
                }

                // Link external login
                IdentityResult addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);

                if (!addLoginResult.Succeeded)
                {
                    return BadRequest(addLoginResult.Errors);
                }
            }
            else
            {
                user = await _userManager.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey);
            }

            if (user is null)
                return BadRequest("The external-login user could not be resolved.");

            // Issue the same compact JWT shape as password login.
            string jwt = await _jwtService.GenerateTokenAsync(user);

            // A fragment is not sent in HTTP requests, keeping the token out of server/proxy
            // query-string logs and referrer headers. The SPA must clear it after reading it.
            string separator = returnUrl.Contains('#') ? "&" : "#";
            string redirect = $"{returnUrl}{separator}token={Uri.EscapeDataString(jwt)}";

            return Redirect(redirect);
        }
    }
}

