using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EasyTemplate.Tool.Entity;
using Microsoft.IdentityModel.Tokens;
using static EasyTemplate.Tool.Entity.PublicEnum;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace EasyTemplate.Tool;

public class Jwt
{

    /// <summary>
    /// 颁发JWT字符串
    /// </summary>
    /// <param name="tokenModel"></param>
    /// <returns></returns>
    public static string Serialize(TokenModelJwt tokenModel)
    {
        // 自己封装的 appsettign.json 操作类，看下文
        /*
        var iss = Appsettings.app("Audience", "Issuer");
        var aud = Appsettings.app("Audience", "Audience");
        var secret = Appsettings.app("Audience", "Secret");
        */
        var iss = "vue";
        var aud = "datacapture";
        var secret = "zhangminbianjibushujucaiji";
        var claims = new List<Claim>
          {
             /*
             * 特别重要：
               1、这里将用户的部分信息，比如 uid 存到了Claim 中，如果你想知道如何在其他地方将这个 uid从 Token 中取出来，请看下边的SerializeJwt() 方法，或者在整个解决方案，搜索这个方法，看哪里使用了！
               2、你也可以研究下 HttpContext.User.Claims ，具体的你可以看看 Policys/PermissionHandler.cs 类中是如何使用的。
             */                

            new Claim(JwtRegisteredClaimNames.Name, tokenModel.Name),
            new Claim(JwtRegisteredClaimNames.Jti, tokenModel.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Typ, ((int)tokenModel.UserType).ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, $"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}"),
            new Claim(JwtRegisteredClaimNames.Nbf,$"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}") ,
            //这个就是过期时间，目前是过期秒，1天，可自定义，注意JWT有自己的缓冲过期时间
            new Claim (JwtRegisteredClaimNames.Exp,$"{new DateTimeOffset(DateTime.Now.AddSeconds(86400*7)).ToUnixTimeSeconds()}"),
            new Claim(JwtRegisteredClaimNames.Iss,iss),
            new Claim(JwtRegisteredClaimNames.Aud,aud),
            
            //new Claim(ClaimTypes.Role,tokenModel.Role),//为了解决一个用户多个角色(比如：Admin,System)，用下边的方法
           };

        // 可以将一个用户的多个角色全部赋予；
        // 作者：DX 提供技术支持；
        //claims.AddRange(tokenModel.Grade.Split(',').Select(s => new Claim(ClaimTypes.Role, s)));
        claims.AddRange(tokenModel.Name.Split(',').Select(s => new Claim(ClaimTypes.Name, s)));

        //秘钥 (SymmetricSecurityKey 对安全性的要求，密钥的长度太短会报出异常)
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: iss,
            claims: claims,
            signingCredentials: creds);

        var jwtHandler = new JwtSecurityTokenHandler();
        var encodedJwt = jwtHandler.WriteToken(jwt);

        return encodedJwt;
    }

    /// <summary>
    /// 解析
    /// </summary>
    /// <param name="jwtStr"></param>
    /// <returns></returns>
    public static TokenModelJwt Deserialize(string jwtStr, out DateTime expired)
    {
        var jwtHandler = new JwtSecurityTokenHandler();
        var jwtToken = jwtHandler.ReadJwtToken(jwtStr);
        int type;
        object name;
        try
        {
            jwtToken.Payload.TryGetValue(ClaimTypes.Name, out name);
            jwtToken.Payload.TryGetValue(JwtRegisteredClaimNames.Typ, out object user_type);
            type = Convert.ToInt32(user_type);
            long exp = Convert.ToInt64(jwtToken.Payload["exp"]);
            expired = exp.ToDateTime();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        var tm = new TokenModelJwt
        {
            UserId = int.Parse(jwtToken.Id),
            Name = name != null ? name.ToString() : "",
            UserType = (UserType)type
        };
        return tm;
    }
}
