using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using aspnetcoregraphql.Models.Entities;
using Newtonsoft.Json;
using aspnetcoregraphql.Data.Repositories;
using System.Collections.Generic;

namespace aspnetcoregraphql.Auth
{
    public class AuhtenticationManager
    {
        private const string Secret = "bs5OIsj+BXE9NZDy7ycW3TcNekrF+2d/1sFnWG4HnV8KOL30iTOdtVWJG8abWvB1GlOgJuQZdcF2Lmvn/hccMo==";  
        public static TokenData GenerateToken(string username, int expireMinutes = 1) {  
            var symmetricKey = Convert.FromBase64String(Secret);  
            var tokenHandler = new JwtSecurityTokenHandler();  
            var now = DateTime.UtcNow;  
            var user = new UserRepository().GetUser(username);
            var calims = new List<Claim> {
                    new Claim("userid", user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("firstname", user.FirstName),
                    new Claim("lastname", user.LastName),
            };
            calims.AddRange(user.Roles);

            var tokenDescriptor = new SecurityTokenDescriptor {  
                Subject = new ClaimsIdentity(calims),
                Expires = now.AddMinutes(Convert.ToInt32(expireMinutes)),  
                Issuer = "SmartStores",
                Audience = "CustomClient",
                IssuedAt = now,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(symmetricKey), SecurityAlgorithms.HmacSha256Signature)                
            };
            
            var stoken = tokenHandler.CreateToken(tokenDescriptor);  
            
            var tokenData = new TokenData();
            tokenData.Token = tokenHandler.WriteToken(stoken);


            return tokenData;  
        }

        public static string ValidateToken(string token)
        {
            string username = null;
            ClaimsPrincipal principal = GetPrincipal(token);

            if (principal == null)
                return null;

            ClaimsIdentity identity = null;
            try
            {
                identity = (ClaimsIdentity)principal.Identity;
            }
            catch (NullReferenceException)
            {
                return null;
            }

            Claim usernameClaim = identity.FindFirst(ClaimTypes.Name);
            username = usernameClaim.Value;

            return username;
        }

        public static ClaimsPrincipal GetPrincipal(string token)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtToken = (JwtSecurityToken)tokenHandler.ReadToken(token);

                if (jwtToken == null)
                    return null;

                byte[] key = Convert.FromBase64String(Secret);

                TokenValidationParameters parameters = new TokenValidationParameters()
                {
                    RequireExpirationTime = true,
                    ValidateLifetime = true,

                    ValidateIssuer = true,
                    ValidIssuer = "SmartStores",
                    
                    ValidateAudience = true,
                    ValidAudience = "CustomClient",

                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                SecurityToken securityToken;
                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, parameters, out securityToken);
                return principal;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public bool CheckUser(string username, string password)
        {
            var user  = new UserRepository().GetUser(username, password);
            if (!user.Equals(null)) {
                return true;
            }
            return false;
        }         
    }
}