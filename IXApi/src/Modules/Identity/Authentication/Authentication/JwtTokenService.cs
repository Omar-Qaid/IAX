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
using IAX.IXApi.Shared.Application.Identity;

namespace IAX.IXApi.Modules.Identity.Authentication.Authentication
{
    public class JwtTokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<AspNetUser> _userManager;

        public JwtTokenService(
            IOptions<JwtSettings> jwtSettings,
            UserManager<AspNetUser> userManager)
        {
            _jwtSettings = jwtSettings.Value;
            _userManager = userManager;
        }

        public async Task<string> GenerateTokenAsync(AspNetUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var userClaims = await _userManager.GetClaimsAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? ""),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var companyClaims = userClaims
                .Where(claim => claim.Type.Equals(CompanyContextDefaults.ClaimType, StringComparison.OrdinalIgnoreCase)
                    || claim.Type.Equals("Company", StringComparison.OrdinalIgnoreCase)
                    || claim.Type.Equals("DataAreaId", StringComparison.OrdinalIgnoreCase))
                .Select(claim => claim.Value)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (companyClaims.Count == 0)
                companyClaims.Add(CompanyContextDefaults.DataAreaId);

            foreach (var company in companyClaims)
                claims.Add(new Claim(CompanyContextDefaults.ClaimType, company));

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

