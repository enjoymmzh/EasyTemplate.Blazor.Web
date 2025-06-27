using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using EasyTemplate.Service.Common;
using EasyTemplate.Tool.Entity;
using Microsoft.AspNetCore.Components;
using EasyTemplate.Tool;
using static EasyTemplate.Tool.Entity.PublicEnum;

namespace EasyTemplate.Service;

[ApiGroup(ApiGroupNames.System)]
public class SystemUserService : BaseService
{
    /// <summary>
    /// 
    /// </summary>
    private readonly SqlSugarRepository<SystemUser> _user;
    /// <summary>
    /// 
    /// </summary>
    private readonly IHttpContextAccessor _contextAccessor;

    public SystemUserService(IHttpContextAccessor contextAccessor, SqlSugarRepository<SystemUser> user)
    {
        _contextAccessor = contextAccessor;
        _user = user;
    }

    /// <summary>
    /// 多语言、单一接口的多种请求方式
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [HttpPost("/api/v1/[Controller]/[Action]")]
    [HttpGet("/api/v2/[controller]/[action]")]
    [AllowAnonymous]
    public async Task<object> Test()
    {
        //前端寫入語言
        _contextAccessor.HttpContext.Request.Headers.ContentLanguage = "en-US";

        //接口處理語言
        var list = _user.AsQueryable().ToList();


        var aaa = _contextAccessor.HttpContext.Request.Headers.Authorization;
        return list;
    }

    /// <summary>
    /// 友好异常
    /// </summary>
    /// <returns></returns>
    public async Task<object> TestException()
    {
        _contextAccessor.HttpContext.Request.Headers.ContentLanguage = "en-US";
        throw Oops.Oh(ErrorCode.E1000);
        throw Oops.Oh("123123123123");
    }

    /// <summary>
    /// 黏土类型
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<object> TestClay()
    {
        var obj = Clay.Object();
        obj.Id = 1;
        obj["Name"] = "test";
        obj.Data = new string[] { "test", "t" };
        return obj;
    }
}
