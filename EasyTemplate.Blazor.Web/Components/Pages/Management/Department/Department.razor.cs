using AntDesign.TableModels;
using EasyTemplate.Tool;
using Microsoft.JSInterop;
using SqlSugar;

namespace EasyTemplate.Blazor.Web.Components.Pages.Management.Department;

public partial class Department
{
    /// <summary>
    /// 注入实例
    /// </summary>
    [Inject] private SqlSugarRepository<SystemDepartment> _repository { get; set; }
    /// <summary>
    /// 
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
            .WhereIF(!string.IsNullOrWhiteSpace(Q_Name), x => x.Name.Contains(Q_Name))
            .OrderByDescending(x => x.Id)
            .ToPageListAsync(Pi, Ps, total);
        _total = total.Value;
        _dataSource = res;
        Loading = false;
        //_table.ReloadData();
    }

    /// <summary>
    /// 增/改
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private async Task<bool> InsertOrUpdate(SystemDepartment data)
        => await _repository.InsertOrUpdateAsync(data);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    private async Task Delete(SystemDepartment row)
    {
        if (!await Comfirm($"确认删除 [{row.Name}] ?")) return;

        var flag = await _repository.AsUpdateable()
            .SetColumns(x => x.IsDelete == true)
            .Where(x => x.Id == row.Id)
            .ExecuteCommandAsync() > 0;

        _Table.ReloadData(Pi, Ps);
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

    private async Task OnChange(QueryModel<SystemDepartment> query)
        => await Query();

    private async Task Search()
    {
        Pi = 1;
        await Query();
    }
}


