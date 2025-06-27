using EasyTemplate.Service;
using EasyTemplate.Tool;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace EasyTemplate.Blazor.Web.Common;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    /// <summary>
    /// 
    /// </summary>
    private NavigationManager _NavigationManager;
    /// <summary>
    /// 
    /// </summary>
    private ClaimsIdentity identity { get; set; } = new ClaimsIdentity();

    public CustomAuthenticationStateProvider(NavigationManager NavigationManager)
    {
        _NavigationManager = NavigationManager;
    }

    /// <summary>
    /// 用户认证成功，创建用户的ClaimsIdentity
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<bool> SignIn(SystemUser user)
    {
        var claims = new[] { 
            new Claim(ClaimTypes.Name, user.Account),
            new Claim("UserId", user.Id.ToString()),
        };
        identity = new ClaimsIdentity(claims, "user");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        return true;
    }

    /// <summary>
    /// 退出登录
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SignOut()
    {
        // 用户认证成功，创建用户的ClaimsIdentity
        identity = new ClaimsIdentity();
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        return true;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        //获取当前路由
        var currentRoute = _NavigationManager.ToBaseRelativePath(_NavigationManager.Uri);

        var user = new ClaimsPrincipal(identity);
        return new AuthenticationState(user);
    }

}
