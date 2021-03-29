﻿using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Mix.Cms.Lib.Constants;
using Mix.Cms.Lib.Models.Account;
using Mix.Cms.Lib.Services;
using Mix.Cms.Lib.ViewModels.Account;
using Mix.Heart.Helpers;
using Mix.Identity.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Mix.Cms.Lib.Helpers
{
    public class IdentityHelper
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public IdentityHelper(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public async Task<JObject> GetAuthData(ApplicationUser user, bool rememberMe)
        {
            var rsaKeys = RSAEncryptionHelper.GenerateKeys();
            var aesKey = AesEncryptionHelper.GenerateCombinedKeys(256);
            var token = await GenerateAccessTokenAsync(user, rememberMe, aesKey, rsaKeys[MixConstants.CONST_RSA_PUBLIC_KEY]);
            if (token != null)
            {
                token.Info = new MixUserViewModel(user);
                await token.Info.LoadUserDataAsync();

                var plainText = JObject.FromObject(token).ToString(Formatting.None).Replace("\r\n", string.Empty);
                var encryptedInfo = AesEncryptionHelper.EncryptString(plainText, aesKey);

                var resp = new JObject()
                        {
                            new JProperty("k", aesKey),
                            new JProperty("rpk", rsaKeys[MixConstants.CONST_RSA_PRIVATE_KEY]),
                            new JProperty("data", encryptedInfo)
                        };
                return resp;
            }
            return default;
        }

        public async Task<AccessTokenViewModel> GenerateAccessTokenAsync(ApplicationUser user, bool isRemember, string aesKey, string rsaPublicKey)
        {
            var dtIssued = DateTime.UtcNow;
            var dtExpired = dtIssued.AddMinutes(MixService.GetAuthConfig<int>(MixAuthConfigurations.CookieExpiration));
            var dtRefreshTokenExpired = dtIssued.AddMinutes(MixService.GetAuthConfig<int>(MixAuthConfigurations.RefreshTokenExpiration));
            string refreshTokenId = string.Empty;
            string refreshToken = string.Empty;
            if (isRemember)
            {
                refreshToken = Guid.NewGuid().ToString();
                RefreshTokenViewModel vmRefreshToken = new RefreshTokenViewModel(
                            new RefreshTokens()
                            {
                                Id = refreshToken,
                                Email = user.Email,
                                IssuedUtc = dtIssued,
                                ClientId = MixService.GetAuthConfig<string>(MixAuthConfigurations.Audience),
                                Username = user.UserName,
                                //Subject = SWCmsConstants.AuthConfiguration.Audience,
                                ExpiresUtc = dtRefreshTokenExpired
                            });

                var saveRefreshTokenResult = await vmRefreshToken.SaveModelAsync();
                refreshTokenId = saveRefreshTokenResult.Data?.Id;
            }

            AccessTokenViewModel token = new AccessTokenViewModel()
            {
                Access_token = await GenerateTokenAsync(user, dtExpired, refreshToken, aesKey, rsaPublicKey),
                Refresh_token = refreshTokenId,
                Token_type = MixService.GetAuthConfig<string>(MixAuthConfigurations.TokenType),
                Expires_in = MixService.GetAuthConfig<int>(MixAuthConfigurations.CookieExpiration),
                Issued = dtIssued,
                Expires = dtExpired,
                LastUpdateConfiguration = MixService.GetConfig<DateTime?>(MixAppSettingKeywords.LastUpdateConfiguration)
            };
            return token;
        }

        public async Task<string> GenerateTokenAsync(ApplicationUser user, DateTime expires, string refreshToken, string aesKey, string rsaPublicKey)
        {
            List<Claim> claims = await GetClaimsAsync(user);
            claims.AddRange(new[]
                {
                    new Claim(MixClaims.Id, user.Id.ToString()),
                    new Claim(MixClaims.Username, user.UserName),
                    new Claim(MixClaims.RefreshToken, refreshToken),
                    new Claim(MixClaims.AESKey, aesKey),
                    new Claim(MixClaims.RSAPublicKey, rsaPublicKey)
                });
            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
                issuer: MixService.GetAuthConfig<string>(MixAuthConfigurations.Issuer),
                audience: MixService.GetAuthConfig<string>(MixAuthConfigurations.Audience),
                notBefore: expires.AddMinutes(-MixService.GetAuthConfig<int>(MixAuthConfigurations.CookieExpiration)),

                claims: claims,
                // our token will live 1 hour, but you can change you token lifetime here
                expires: expires,
                signingCredentials: new SigningCredentials(JwtSecurityKey.Create(MixService.GetAuthConfig<string>(MixAuthConfigurations.SecretKey)), SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }

        public async Task<List<Claim>> GetClaimsAsync(ApplicationUser user)
        {
            List<Claim> claims = new List<Claim>();
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var claim in user.Claims)
            {
                claims.Add(CreateClaim(claim.ClaimType, claim.ClaimValue));
            }

            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole));
                var role = await _roleManager.FindByNameAsync(userRole);
                if (role != null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    foreach (Claim roleClaim in roleClaims)
                    {
                        claims.Add(roleClaim);
                    }
                }
            }
            return claims;
        }

        public Claim CreateClaim(string type, string value)
        {
            return new Claim(type, value, ClaimValueTypes.String);
        }

        public static string GetClaim(ClaimsPrincipal User, string claimType)
        {
            return User.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = MixService.GetAuthConfig<bool>(MixAuthConfigurations.ValidateIssuer),
                ValidateAudience = MixService.GetAuthConfig<bool>(MixAuthConfigurations.ValidateAudience),
                ValidateLifetime = MixService.GetAuthConfig<bool>(MixAuthConfigurations.ValidateLifetime),
                ValidateIssuerSigningKey = MixService.GetAuthConfig<bool>(MixAuthConfigurations.ValidateIssuerSigningKey),
                IssuerSigningKey = JwtSecurityKey.Create(MixService.GetAuthConfig<string>(MixAuthConfigurations.SecretKey))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        public static class JwtSecurityKey
        {
            public static SymmetricSecurityKey Create(string secret)
            {
                return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
            }
        }
    }
}