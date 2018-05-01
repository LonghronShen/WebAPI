using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace WebApi.Common
{

    public class TokenHelper
    {

        public const string Issuer = "http://localhost:9460";
        private const string PrivateKey = "private_key_1234567890";

        /// <summary>
        /// token的过期时间
        /// </summary>
        public TimeSpan TokenExpiration { get; }

        /// <summary>
        /// 定义SecurityKey，类中的属性用于签名认证
        /// </summary>
        public SigningCredentials SigningCredentials { get; }

        /// <summary>
        /// 定义私钥
        /// 使用对称算法生成的对称安全密钥
        /// </summary>
        public static SymmetricSecurityKey SecurityKey { get; } = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(PrivateKey));

        public TokenHelper()
        {
            TokenExpiration = TimeSpan.FromMinutes(10);
            SigningCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);
        }

        public JwtAuthResult CreateJWTToken(string uId)
        {
            DateTime now = DateTime.UtcNow;
            var claims = new[] {
                //发布人
                new Claim(JwtRegisteredClaimNames.Iss, Issuer),
                //给当前用户授权
                new Claim(JwtRegisteredClaimNames.Sub, uId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                //产生时间
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                claims: claims,
                notBefore: now,
                expires: now.Add(TokenExpiration),
                signingCredentials: SigningCredentials);
            var handler = new JwtSecurityTokenHandler();
            var encodedToken = handler.WriteToken(token);
            return new JwtAuthResult()
            {
                AccessToken = encodedToken,
                Expiration = (int)TokenExpiration.TotalSeconds
            };
        }

    }

}
