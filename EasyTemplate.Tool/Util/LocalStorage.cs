using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace EasyTemplate.Tool.Util;

/// <summary>
/// 操作浏览器LocalStorage帮助类
/// 注：需要在wwwroot下创建js文件夹，然后创建LocalStorage.js文件，并在App.razor中引入
/// </summary>
public class LocalStorage
{
    public static string AutoLogin { get { return "_4F2iGsd_"; } }
    public static string UserInfo { get { return "_4F74k33_"; } }

    private readonly IJSRuntime _jsRuntime;
    public LocalStorage(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// 设置LocalStorage
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task SetLocalStorage(string key, string value)
    {
        await _jsRuntime.InvokeVoidAsync("LocalStorageSet", key, value);
    }

    /// <summary>
    /// 获取LocalStorage
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<string> GetLocalStorage(string key)
    {
        return await _jsRuntime.InvokeAsync<string>("LocalStorageGet", key);
    }

    /// <summary>
    /// 删除LocalStorage
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task RemoveLocalStorage(string key)
    {
        await _jsRuntime.InvokeVoidAsync("LocalStorageRemove", key);
    }
    
}
