using AntDesign.TableModels;
using EasyTemplate.Tool;
using EasyTemplate.Tool.Util;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SqlSugar;

namespace EasyTemplate.Blazor.Web.Components.Pages.Management.User;

public partial class User
{
    /// <summary>
    /// 注入实例
    /// </summary>
    [Inject] private SqlSugarRepository<SystemUser> _repository { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] private SqlSugarRepository<SystemRole> _role { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] private SqlSugarRepository<SystemArea> _area { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] private SqlSugarRepository<SystemDepartment> _department { get; set; }
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
    private string Q_Account { get; set; }
    /// <summary>
    /// 
    /// </summary>
    private string AreadIds { get; set; }
    private List<CascaderNode> Areas { get; set; } = new List<CascaderNode>();

    protected override async Task OnInitializedAsync()
    {

    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await NavigationManager.RedirectLogin(IJSRuntime);
            //await Query();

            var area = await _area.AsQueryable().OrderBy(x => x.Sort).ToTreeAsync(x => x.Children, x => x.ParentCode, 0, x => x.AreaCode);
            Areas = area.Select(ConvertToAreaItem).ToList();

            Roles = _role.GetList(x => x.Enabled == true);

            Departments = _department.GetList(x => x.Enabled == true);
        }
    }

    private CascaderNode ConvertToAreaItem(SystemArea area)
    {
        return new CascaderNode
        {
            Value = area.Id.ToString(),
            Label = area.AreaName,
            Children = area.Children?.Select(ConvertToAreaItem).ToArray()
        };
    }

    /// <summary>
    /// 查
    /// </summary>
    /// <returns></returns>
    private async Task Query()
    {
        Loading = true;

        RefAsync<int> total = 0;
        var list = await _repository.AsQueryable()
            .LeftJoin<SystemRole>((c, u) => c.RoleId == u.Id)
            .LeftJoin<SystemDepartment>((c, u, d) => c.DepartmentId == d.Id)
            .WhereIF(!string.IsNullOrWhiteSpace(Q_Account), (c, u, d) => c.Account.Contains(Q_Account))
            .OrderByDescending((c, u, d) => c.Id)
            .Select((c, u, d) => new SystemUser {
                Id = c.Id,
                Avatar = c.Avatar,
                Account = c.Account,
                NickName = c.NickName,
                RoleId = c.RoleId,
                RoleName = u.Name,
                DepartmentId = c.DepartmentId,
                DepartmentName = d.Name,
                Mobile = c.Mobile,
                Email = c.Email,
                Address = c.Address,
                Enabled = c.Enabled,
                Signature = c.Signature,
                AreaIds = c.AreaIds,
                CreateTime = c.CreateTime
            })
            .ToPageListAsync(Pi, Ps, total);
        list.ForEach(x => {
            if (!string.IsNullOrWhiteSpace(x.AreaIds))
            {
                var ids = x.AreaIds?.Split(',');
                x.LastAreadId = ids[^1].Trim();

                var area = _area.AsQueryable()
                    .Where(item => SqlFunc.SplitIn(x.AreaIds, item.Id.ToString()))
                    .Select(item => item.AreaName)
                    .ToArray();
                x.Area = $"{string.Join(" ", area)} {x.Address}";
            }
        });
        Total = total.Value;
        DataSource = list;

        Loading = false;
    }

    /// <summary>
    /// 增/改
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private async Task<bool> InsertOrUpdate(SystemUser data)
    {
        if (data.Id == 0)
        {
            if (data.Password != data.ConfirmPassword)
            {
                await MessageService.Error("两次输入密码不一致");
                return false;
            }

            if (await _repository.AsQueryable().AnyAsync(x => x.Account.ToLower() == data.Account.ToLower() || x.NickName.ToLower() == data.NickName.ToLower()))
            {
                await MessageService.Error("该账号名已存在");
                return false;
            }

            return await _repository.AsInsertable(data).ExecuteCommandAsync() > 0;
        }
        else
        {
            if (await _repository.AsQueryable().AnyAsync(x => x.NickName.ToLower() == data.NickName.ToLower() && x.Id != data.Id))
            {
                await MessageService.Error("该账号名已存在");
                return false;
            }

            var res = await _repository.AsUpdateable()
                .SetColumns(x => x.NickName == data.NickName)
                .SetColumns(x => x.RoleId == data.RoleId)
                .SetColumns(x => x.DepartmentId == data.DepartmentId)
                .SetColumns(x => x.Mobile == data.Mobile)
                .SetColumns(x => x.Email == data.Email)
                .SetColumns(x => x.Address == data.Address)
                .SetColumns(x => x.Enabled == data.Enabled)
                .SetColumns(x => x.Signature == data.Signature)
                .SetColumns(x => x.AreaIds == AreadIds)
                .Where(x => x.Id == data.Id)
                .ExecuteCommandAsync() > 0;
            return res;
        }
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    private async Task Delete(SystemUser row)
    {
        if (row.Account.ToLower().Contains("admin"))
        {
            await MessageService.Error("不能删除超级管理员");
            return;
        }

        if (!await Comfirm($"确认删除 [{row.Account}] ?")) return;

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
        Q_Account = string.Empty;
        Pi = 1;
        await Query();
    }

    /// <summary>
    /// 表格使用它来初始化，不要在页面初始化时调用，否则会导致重复加载
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    private async Task OnChange(QueryModel<SystemUser> query)
        => await Query();

    private async Task Search()
    {
        Pi = 1;
        await Query();
    }

    private void OnChange(CascaderNode[] selectedNodes)
    {
        AreadIds = string.Join(",", selectedNodes.Select(x => x.Value));
    }

    private void CheckedChanged(SystemUser row)
    {
        _repository.AsUpdateable()
            .SetColumns(x => x.Enabled == row.Enabled)
            .Where(x => x.Id == row.Id)
            .ExecuteCommand();
        //_table.ReloadData();
    }
}


