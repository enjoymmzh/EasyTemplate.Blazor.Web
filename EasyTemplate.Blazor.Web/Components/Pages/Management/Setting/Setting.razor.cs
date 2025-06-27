using AntDesign.TableModels;
using EasyTemplate.Tool;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SqlSugar;
using System.Data.Common;

namespace EasyTemplate.Blazor.Web.Components.Pages.Management.Setting;

public partial class Setting
{
    /// <summary>
    /// 注入实例
    /// </summary>
    [Inject] SqlSugarRepository<SystemSetting> _repository { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] private SqlSugarRepository<SystemArea> _area { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] NavigationManager NavigationManager { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] IJSRuntime IJSRuntime { get; set; }
    /// <summary>
    /// 
    /// </summary>
    private string AreadIds { get; set; }
    /// <summary>
    /// 
    /// </summary>
    private List<CascaderNode> Areas { get; set; } = new List<CascaderNode>();
    /// <summary>
    /// 
    /// </summary>
    [Inject] IMessageService MessageService { get; set; }

    protected override async Task OnInitializedAsync()
    {

    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await NavigationManager.RedirectLogin(IJSRuntime);
            await Query();

            var area = await _area.AsQueryable().OrderBy(x => x.Sort).ToTreeAsync(x => x.Children, x => x.ParentCode, 0, x => x.AreaCode);
            Areas = area.Select(ConvertToAreaItem).ToList();

            StateHasChanged();
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

    private void OnChange(CascaderNode[] selectedNodes)
    {
        AreadIds = string.Join(",", selectedNodes.Select(x => x.Value));
    }

    /// <summary>
    /// 查
    /// </summary>
    /// <returns></returns>
    private async Task Query()
    {
        Loading = true;

        var data = await _repository.AsQueryable().FirstAsync();
        if (!string.IsNullOrWhiteSpace(data?.AreaIds))
        {
            var ids = data.AreaIds?.Split(',');
            data.LastAreadId = ids[^1].Trim();

            var area = _area.AsQueryable()
                .Where(item => SqlFunc.SplitIn(data.AreaIds, item.Id.ToString()))
                .Select(item => item.AreaName)
                .ToArray();
            data.Area = $"{string.Join(" ", area)} {data.Address}";
        }
        _DataSource = data;

        Loading = false;
    }

    private async Task Update()
    {
        var data = _DataSource;
        var flag = await _repository.AsUpdateable()
            .SetColumns(x => x.CompanyName == data.CompanyName)
            .SetColumns(x => x.CompanyNo == data.CompanyNo)
            .SetColumns(x => x.CopyRight == data.CopyRight)
            .SetColumns(x => x.RegisterDate == data.RegisterDate)
            .SetColumns(x => x.AreaIds == AreadIds)
            .SetColumns(x => x.Address == data.Address)
            .SetColumns(x => x.ContactPhone == data.ContactPhone)
            .SetColumns(x => x.ContactEmail == data.ContactEmail)
            .SetColumns(x => x.WebsiteUrl == data.WebsiteUrl)
            .Where(x=>x.Id == data.Id)
            .ExecuteCommandAsync() > 0;
        if (flag)
        {
            await MessageService.Success("修改成功");
        }
        else
        {
            await MessageService.Error("修改失败");
        }
    }
}


