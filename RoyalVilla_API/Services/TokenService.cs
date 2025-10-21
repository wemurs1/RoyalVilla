using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RoyalVilla_API.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<TokenService> _logger;

        public TokenService(
            IConfiguration configuration, 
            UserManager<ApplicationUser> userManager, 
            ApplicationDbContext db,
            ILogger<TokenService> logger)
        {
            _configuration = configuration;
            _userManager = userManager;
            _db = db;
            _logger = logger;
        }

        public async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JwtSettings")["Secret"]);
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // JWT ID for tracking
            };

            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15), // Short-lived access token
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<string> GenerateRefreshTokenAsync()
        {
            // Generate a cryptographically secure random token
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            
            var refreshToken = Convert.ToBase64String(randomNumber);
            
            // Ensure uniqueness by checking database
            var exists = await _db.RefreshTokens.AnyAsync(rt => rt.RefreshTokenValue == refreshToken);
            if (exists)
            {
                // Recursively generate a new one if collision occurs (very rare)
                return await GenerateRefreshTokenAsync();
            }
            
            return refreshToken;
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JwtSettings")["Secret"]);
                var tokenHandler = new JwtSecurityTokenHandler();

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                return tokenHandler.ValidateToken(token, validationParameters, out _);
            }
            catch
            {
                return null;
            }
        }

        public async Task<(bool IsValid, string? UserId, bool TokenReused)> ValidateRefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _db.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.RefreshTokenValue == refreshToken);

            // Token doesn't exist in database
            if (storedToken == null)
            {
                return (false, null, false);
            }

            // CRITICAL SECURITY CHECK: Token Reuse Detection
            // If token exists but is marked as invalid, it means someone tried to reuse it
            // This is a strong indicator of token theft
            if (!storedToken.IsValid)
            {
                _logger.LogWarning(
                    "?? SECURITY ALERT: Refresh token reuse detected! " +
                    "Token: {TokenId}, UserId: {UserId}. " +
                    "Revoking all tokens for this user.",
                    storedToken.Id, storedToken.UserId);

                // Revoke all tokens for this user as a security measure
                await RevokeAllUserTokensAsync(storedToken.UserId);

                return (false, storedToken.UserId, true); // TokenReused = true
            }

            // Token is expired
            if (storedToken.ExpiresAt < DateTime.UtcNow)
            {
                return (false, storedToken.UserId, false);
            }

            // Token is valid
            return (true, storedToken.UserId, false);
        }

        public async Task SaveRefreshTokenAsync(string userId, string jwtTokenId, string refreshToken, DateTime expiresAt)
        {
            var refreshTokenEntity = new RefreshToken
            {
                UserId = userId,
                JwtTokenId = jwtTokenId,
                RefreshTokenValue = refreshToken,
                IsValid = true,
                ExpiresAt = expiresAt
            };

            await _db.RefreshTokens.AddAsync(refreshTokenEntity);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _db.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.RefreshTokenValue == refreshToken);

            if (storedToken == null)
            {
                return false;
            }

            storedToken.IsValid = false;
            await _db.SaveChangesAsync();
            
            _logger.LogInformation(
                "Refresh token revoked. TokenId: {TokenId}, UserId: {UserId}", 
                storedToken.Id, storedToken.UserId);
            
            return true;
        }

        public async Task RevokeAllUserTokensAsync(string userId)
        {
            var userTokens = await _db.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.IsValid)
                .ToListAsync();

            if (!userTokens.Any())
            {
                return;
            }

            foreach (var token in userTokens)
            {
                token.IsValid = false;
            }

            await _db.SaveChangesAsync();
            
            _logger.LogWarning(
                "?? All refresh tokens revoked for user. UserId: {UserId}, TokenCount: {Count}", 
                userId, userTokens.Count);
        }
    }
}
