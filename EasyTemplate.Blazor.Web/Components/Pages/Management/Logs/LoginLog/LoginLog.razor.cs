using AntDesign.TableModels;
using EasyTemplate.Tool;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SqlSugar;

namespace EasyTemplate.Blazor.Web.Components.Pages.Management.Logs.LoginLog;

public partial class LoginLog
{
    /// <summary>
    /// 注入实例
    /// </summary>
    [Inject] private SqlSugarRepository<SystemLogLogin> _repository { get; set; }
    /// <summary>
    /// 数据源
    /// </summary>
    [Inject] NavigationManager NavigationManager { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] IJSRuntime IJSRuntime { get; set; }
    /// <summary>
    /// 账号
    /// </summary>
    private string Q_Name { get; set; }

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
        RefAsync<int> total = 0;
        var res = await _repository.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(Q_Name), x => x.Info.Contains(Q_Name))
            .OrderByDescending(x => x.Id)
            .ToPageListAsync(Pi, Ps, total);
        _total = total.Value;
        _dataSource = res;
        Loading = false;
        //_table.ReloadData();
    }

    /// <summary>
    /// 重置查询
    /// </summary>
    private async Task ResetQuery()
    {
        Q_Name = string.Empty;
        Pi = 1;
        await Query();
    }

    private async Task OnChange(QueryModel<SystemLogLogin> query)
        => await Query();

    private async Task Search()
    {
        Pi = 1;
        await Query();
    }
}


