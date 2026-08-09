using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IAX.IXApi.Modules.Identity.Authentication.Authentication
{
    public class JwtTokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<AspNetUser> _userManager;
        private readonly IAppPermissionService _permissionService;

        public JwtTokenService(
            IOptions<JwtSettings> jwtSettings,
            UserManager<AspNetUser> userManager,
            IAppPermissionService permissionService)
        {
            _jwtSettings = jwtSettings.Value;
            _userManager = userManager;
            _permissionService = permissionService;
        }

        public async Task<string> GenerateTokenAsync(AspNetUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await _permissionService.GetPermissionKeysByUserAsync(user.Id);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? ""),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            foreach (var permission in permissions)
                claims.Add(new Claim("permission", permission));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.Expires);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

