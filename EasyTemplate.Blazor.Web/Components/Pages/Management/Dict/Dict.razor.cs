using AntDesign.TableModels;
using EasyTemplate.Tool;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SqlSugar;

namespace EasyTemplate.Blazor.Web.Components.Pages.Management.Dict;

public partial class Dict
{
    /// <summary>
    /// 注入实例
    /// </summary>
    [Inject] private SqlSugarRepository<SystemDictionary> _repository { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] NavigationManager NavigationManager { get; set; }
    /// <summary>
    /// 账号
    /// </summary>
    private string Q_Name{ get; set; }
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
        RefAsync<int> total = 0;
        var res = await _repository.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(Q_Name), x => x.KeyName.Contains(Q_Name))
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
    private async Task<bool> InsertOrUpdate(SystemDictionary data)
    {
        if (data.Id == 0)
        {
            if (_repository.IsAny(x=>x.KeyName.ToLower() == data.KeyName.ToLower()))
            {
                await MessageService.Error("键值已存在");
                return false;
            }
            return await _repository.InsertAsync(data);
        }
        else
        {
            if (_repository.IsAny(x => x.KeyName.ToLower() == data.KeyName.ToLower() && x.Id != data.Id))
            {
                await MessageService.Error("键值已存在");
                return false;
            }
            return await _repository.UpdateAsync(data);
        }
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    private async Task Delete(SystemDictionary row)
    {
        if (!await Comfirm($"确认删除 [{row.KeyName}] ?")) return;

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

    private async Task OnChange(QueryModel<SystemDictionary> query)
        => await Query();

    private async Task Search()
        => await Query();
}


