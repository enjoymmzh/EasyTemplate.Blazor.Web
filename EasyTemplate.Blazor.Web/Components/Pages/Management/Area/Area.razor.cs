using AntDesign.TableModels;
using EasyTemplate.Tool;
using Microsoft.JSInterop;

namespace EasyTemplate.Blazor.Web.Components.Pages.Management.Area;

public partial class Area
{
    /// <summary>
    /// 账号
    /// </summary>
    private string Q_Name{ get; set; }
    /// <summary>
    /// 注入实例
    /// </summary>
    [Inject] SqlSugarRepository<SystemArea> _repository { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] NavigationManager NavigationManager { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] IJSRuntime IJSRuntime { get; set; }

    protected override async void OnInitialized()
    {

    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await NavigationManager.RedirectLogin(IJSRuntime);
            //await Query();
        }
    }

    /// <summary>
    /// 查
    /// </summary>
    /// <returns></returns>
    private async Task Query()
    {
        Loading = true;
        _dataSource = await _repository.AsQueryable().OrderBy(x => x.Sort).ToTreeAsync(x => x.Children, x => x.ParentCode, 0, x => x.AreaCode);
        Loading = false;
    }

    private async Task OnChange(QueryModel<SystemArea> query)
    => await Query();

}


