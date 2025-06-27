using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace EasyTemplate.Service.Common;

/// <summary>
/// 系统分组特性
/// </summary>
public class ApiGroupAttribute : Attribute, IApiDescriptionGroupNameProvider
{
    public ApiGroupAttribute(ApiGroupNames name)
    {
        GroupName = name.ToString();
    }
    public string GroupName { get; set; }
}

/// <summary>
/// 系统分组枚举值
/// </summary>
public enum ApiGroupNames
{
    [GroupInfo(Title = "登录认证", Description = "登录认证相关接口", Version = "v1")]
    Auth,
    [GroupInfo(Title = "业务", Description = "业务相关接口")]
    Bussiness,
    [GroupInfo(Title = "系统", Description = "系统相关接口")]
    System,
    [GroupInfo(Title = "统计", Description = "统计相关接口")]
    Statistics,
    [GroupInfo(Title = "测试", Description = "测试相关接口")]
    Test
}

/// <summary>
/// 系统模块枚举注释
/// </summary>
public class GroupInfoAttribute : Attribute
{
    public string Title { get; set; }
    public string Version { get; set; }
    public string Description { get; set; }
}
