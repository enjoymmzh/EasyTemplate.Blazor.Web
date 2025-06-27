using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using EasyTemplate.Service.Common;
using EasyTemplate.Tool.Entity;
using EasyTemplate.Tool;
using Microsoft.AspNetCore.Components;
using static EasyTemplate.Tool.Entity.PublicEnum;
using Microsoft.AspNetCore.Authorization;

namespace EasyTemplate.Service;

[AllowAnonymous]
[ApiGroup(ApiGroupNames.Auth)]
public class AuthService : BaseService
{
    /// <summary>
    /// 注意，非blazor环境，不能使用[Inject]方式注入
    /// </summary>
    private readonly SqlSugarRepository<SystemUser> _user;
    /// <summary>
    /// 
    /// </summary>
    private readonly IHttpContextAccessor _contextAccessor;

    public AuthService(IHttpContextAccessor contextAccessor, SqlSugarRepository<SystemUser> user)
    {
        _contextAccessor = contextAccessor;
        _user = user;
    }

    /// <summary>
    /// 登录
    /// {"username":"admin","password":"123456"}
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <remarks><code>{"username":"admin","password":"123456"}</code></remarks>
    [HttpPost]
    public async Task<object> Login(LoginInput input)
    {
        var user = await _user.AsQueryable()
            .Where(x => x.Account.Equals(input.Account) && x.Password.Equals(input.Password))
            .FirstAsync();
        _ = user ?? throw Oops.Oh(ErrorCode.E1000);

        //生成Token令牌
        var token = Jwt.Serialize(new TokenModelJwt
        {
            UserId = user.Id,
            Name = user.Account,
            UserType = PublicEnum.UserType.Admin
        });
        _contextAccessor.HttpContext.Response.Headers["access-token"] = token;
        return token;
    }
}
