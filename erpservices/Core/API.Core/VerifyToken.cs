using Manager.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace API.Core
{
    public class VerifyToken
    {

        public string VerifyTokenHash(HttpContext context)
        {
            string token = "";
            if (context.Request.Headers.TryGetValue("Authorization", out StringValues authHeader))
            {
                if (authHeader.ToString().StartsWith("Bearer "))
                {
                    token = authHeader.ToString().Substring("Bearer ".Length).Trim();                    
                }
            }
            // Create a hash of the incoming token using SHA256
            using (var sha256 = new SHA256Managed())
            {
                var tokenBytes = Encoding.UTF8.GetBytes(token);
                var hashBytes = sha256.ComputeHash(tokenBytes);
                var hashedToken = BitConverter.ToString(hashBytes);

                return hashedToken;
            }
        }

        public string VerifyTokenHash(string token)
        {
            using (var sha256 = new SHA256Managed())
            {
                var tokenBytes = Encoding.UTF8.GetBytes(token);
                var hashBytes = sha256.ComputeHash(tokenBytes);
                var hashedToken = BitConverter.ToString(hashBytes);

                return hashedToken;
            }
        }



    }
}
