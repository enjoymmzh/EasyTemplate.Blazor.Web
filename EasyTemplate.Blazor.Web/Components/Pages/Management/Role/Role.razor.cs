using AntDesign.TableModels;
using EasyTemplate.Tool;
using Microsoft.JSInterop;
using SqlSugar;

namespace EasyTemplate.Blazor.Web.Components.Pages.Management.Role;

public partial class Role
{
    /// <summary>
    /// 注入实例
    /// </summary>
    [Inject] private SqlSugarRepository<SystemRole> _Repository { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] private SqlSugarRepository<SystemRoleMenu> _RoleMenu { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] private SqlSugarRepository<SystemMenu> _Menu { get; set; }
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
    private string Q_Name{ get; set; }
    /// <summary>
    /// 
    /// </summary>
    private List<SystemMenu> _TreeMenu;
    /// <summary>
    /// 
    /// </summary>
    private string[] _DefaultCheckedKeys = [];
    /// <summary>
    /// 
    /// </summary>
    private string[] _CheckedKeys = [];
    /// <summary>
    /// 
    /// </summary>
    private RowData<SystemRole> SelectedRow;

    protected override async Task OnInitializedAsync()
    {

    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _DefaultCheckedKeys = Global.Menus.Where(x => x.Necessary == true).Select(x => x.Id.ToString()).ToArray();
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
        var res = await _Repository.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(Q_Name), x => x.Name.Contains(Q_Name))
            .OrderByDescending(x => x.Id)
            .ToPageListAsync(Pi, Ps, total);
        _Total = total.Value;
        _DataSource = res;
        Loading = false;
        //_table.ReloadData();
    }

    /// <summary>
    /// 增/改
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private async Task<bool> InsertOrUpdate(SystemRole data)
    {
        bool flag = false;
        if (data.Id == 0)
        {
            if (_Repository.AsQueryable().Any(x => x.Name.ToLower() == data.Name.ToLower()))
            {
                MessageService.Error("该账号名已存在");
                return false;
            }

            var id = _Repository.AsInsertable(data).ExecuteReturnIdentity();
            _RoleMenu.Insert(new SystemRoleMenu() { RoleId = id, MenuId = 1 });

            return id > 0;
        }
        else
        {
            if (_Repository.AsQueryable().Any(x => x.Name.ToLower() == data.Name.ToLower() && x.Id != data.Id))
            {
                MessageService.Error("该账号名已存在");
                return false;
            }

            return await _Repository.AsUpdateable()
                .SetColumns(x => x.Name == data.Name)
                .SetColumns(x => x.RoleType == data.RoleType)
                .SetColumns(x => x.Enabled == data.Enabled)
                .Where(x => x.Id == data.Id)
                .ExecuteCommandAsync() > 0;
        }
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    private async Task Delete(SystemRole row)
    {
        if (!await Comfirm($"确认删除 [{row.Name}] ?")) return;

        var flag = await _Repository.AsUpdateable()
            .SetColumns(x => x.IsDelete == true)
            .Where(x => x.Id == row.Id)
            .ExecuteCommandAsync() > 0;

        _Table.ReloadData(Pi, Ps);
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateMenu()
    {
        if (SelectedRow is null)
        {
            MessageService.Error("请先选择一个角色");
            return;
        }

        var nodes = _Tree.CheckedKeys;
        if (nodes.Length == 0)
        {
            MessageService.Error("请选择菜单");
            return;
        }

        bool flag = false;
        try
        {
            _RoleMenu.Context.Ado.BeginTran();

            _RoleMenu.AsDeleteable().Where(x => x.RoleId == SelectedRow.Data.Id).ExecuteCommand();

            var menuIds = CalculateIds(nodes.ToList());
            var menus = menuIds.Select(x => new SystemRoleMenu() { RoleId = SelectedRow.Data.Id, MenuId = Convert.ToInt32(x) }).ToList();
            flag = _RoleMenu.InsertRange(menus);

            _RoleMenu.Context.Ado.CommitTran();
        }
        catch (Exception ex)
        {
            _RoleMenu.Context.Ado.RollbackTran();
            flag = false;
        }

        if (flag)
        {
            MessageService.Success("修改成功");
        }
        else
        {
            MessageService.Error("修改失败");
        }

        _CheckedKeys = _Tree.CheckedKeys;
        SelectedRow = null;

        _Table.ReloadData(Pi, Ps);
        //StateHasChanged();
    }

    private List<int> CalculateIds(List<string> ids)
    {
        var result = new List<int>();

        foreach (var id in ids)
        {
            var menuId = int.Parse(id);
            AddParentIds(menuId, result);
        }

        return result;
    }

    private void AddParentIds(int id, List<int> result)
    {
        var menu = _Menu.AsQueryable().First(m => m.Id == id);
        if (menu != null && !result.Contains(menu.Id))
        {
            result.Add(menu.Id);
            if (menu.ParentId != 0)
            {
                AddParentIds(menu.ParentId, result);
            }
        }
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

    protected override async void OnInitialized()
    {
        await Query();
    }

    private async Task OnChange(QueryModel<SystemRole> query)
        => await Query();

    private async Task Search()
        => await Query();

    private void OnRowClick(RowData<SystemRole> row)
    {
        SelectedRow = row;
        _CheckedKeys = _RoleMenu.AsQueryable()
                    .LeftJoin<SystemMenu>((rm, m) => rm.MenuId == m.Id)
                    .Where((rm, m) => rm.RoleId == row.Data.Id)
                    .Select((rm, m) => m.Id.ToString())
                    .ToArray();
    }

    private void CheckedChanged(SystemRole row)
    {
        _Repository.AsUpdateable()
            .SetColumns(x => x.Enabled == row.Enabled)
            .Where(x => x.Id == row.Id)
            .ExecuteCommand();
        //_table.ReloadData();
    }
}


