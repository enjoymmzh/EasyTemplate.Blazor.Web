using AntDesign.TableModels;
using EasyTemplate.Tool;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace EasyTemplate.Page.Pages.Order;

public partial class Order
{
    /// <summary>
    /// 
    /// </summary>
    [Inject] NavigationManager NavigationManager { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] SqlSugarRepository<SystemMenu> _Repository { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] IJSRuntime IJSRuntime { get; set; }
    /// <summary>
    ///
    /// </summary>
    private ITable _Table;
    /// <summary>
    ///
    /// </summary>
    private IEnumerable<SystemMenu> _SelectedRows = [];
    /// <summary>
    ///
    /// </summary>
    private List<SystemMenu> _DataSource;
    /// <summary>
    ///
    /// </summary>
    private int Pi = 1;
    /// <summary>
    ///
    /// </summary>
    private int Ps = 20;
    /// <summary>
    ///
    /// </summary>
    private bool Loading = false;

    /// <summary>
    /// 查
    /// </summary>
    /// <returns></returns>
    private async Task Query()
    {
        Loading = true;
        _DataSource = await _Repository.AsQueryable().OrderBy(x => x.Sort).ToTreeAsync(x => x.Children, x => x.ParentId, 0);
        Loading = false;
    }

    protected override async void OnInitialized()
    {

    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await NavigationManager.RedirectLogin(IJSRuntime);
            await Query();
        }
    }

    private async Task OnChange(QueryModel<SystemMenu> query)
        => await Query();
}
