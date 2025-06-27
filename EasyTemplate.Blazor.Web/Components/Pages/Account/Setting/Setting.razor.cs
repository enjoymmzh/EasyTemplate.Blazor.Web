using EasyTemplate.Tool;
using Microsoft.JSInterop;

namespace EasyTemplate.Blazor.Web.Components.Pages.Account.Setting;

public partial class Setting
{
    private readonly Dictionary<string, string> _menuMap = new Dictionary<string, string>
        {
            {"base", "基本设置"},
            //{"security", "安全设置"},
            {"binding", "账号绑定"},
            //{"notification", "消息提示"},
        };

    private string _selectKey = "base";
    /// <summary>
    /// 
    /// </summary>
    [Inject] private SqlSugarRepository<SystemUser> _user { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] NavigationManager NavigationManager { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] IJSRuntime IJSRuntime { get; set; }

    private void SelectKey(MenuItem item)
    {
        _selectKey = item.Key;
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await NavigationManager.RedirectLogin(IJSRuntime);
            Global.CurrentUser = _user.GetFirst(x => x.Id == Global.CurrentUser.Id);
        }
    }
}