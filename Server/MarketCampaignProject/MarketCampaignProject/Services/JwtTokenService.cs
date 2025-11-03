using System.Security.Claims; //Building the payload (who the user is)
using System.Text; //Turn a string secret into bytes
using Microsoft.IdentityModel.Tokens; //Sign the token so it can’t be forged
using MarketCampaignProject.Models; //We receive a User object
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates; //Core classes to create and write a JWT

namespace MarketCampaignProject.Services
{
    public class JwtTokenService
    {
        private readonly IConfiguration _config;
        // readonly - get set only once and u can't change it further 
        // IConfiguration - Interface to read settings 
        // _config - variable name that stores all configuration data 

        public JwtTokenService(IConfiguration config)
        {
            _config = config;
        }
        // _config = drawer where we will keep the token or the setting data 
        // config = the data we are passing

        public string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            // _config = reads the key from appsettings.json
            // Encoding.UTF8.GetBytes - convert the string to bytes
            // SymmetricSecurityKey - uses the same key for verification
            // Never hardcode the key - means never show your key directly, configure it so that only you can access it 
            // HMAC-SHA256 - Hash-based message authentication code 
            // SHA-256: A specific cryptographic hash function that takes an input and
            // produces a fixed-size 256-bit (32-byte) output, known as a hash.
            // USE - HMAC-SHA256 is a cryptographic algorithm used to generate a signature for a
            // JSON Web Token (JWT) by combining a header, payload, and a secret key using the
            // SHA-256 hashing function. It is used in JWTs to ensure the token's integrity
            // (that it hasn't been tampered with) and authenticity (that it came from a trusted source).

            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            // SigningCredentials will tell Jwt how to sign the token and HMAC will create a 
            // cryptographic hash of token using secret key 
            // cryptographic hash is the fixed length string of characters

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email ?? string.Empty),
                new Claim("UserId", user.Id.ToString()),
                new Claim("Username", user.Username ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            // JwtRegisteredClaimNames.Sub -> subject (usually the email or unique identifier)
            // "UserId" and "Username" are custom claims you define
            // Jti = JWT ID (unique identifier for this specific token)

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: cred
            );
            // JWT = HEADER + PAYLOAD + SIGNATURE 
            // It keeps the knowledge of issuer (who issued the token i.e. API), 
            // audience (who it is intended for), expiration and how to sign the credentials.

            return new JwtSecurityTokenHandler().WriteToken(token);
            // Converts the JwtSecurityToken object into a compact JWT string.
        }
    }
}
