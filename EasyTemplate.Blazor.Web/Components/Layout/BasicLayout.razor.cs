using EasyTemplate.Tool;
using EasyTemplate.Tool.Util;
using Microsoft.JSInterop;

namespace EasyTemplate.Blazor.Web.Components.Layout;

public partial class BasicLayout : LayoutComponentBase, IDisposable
{
    /// <summary>
    /// 
    /// </summary>
    private MenuDataItem[] _MenuData;
    /// <summary>
    /// 
    /// </summary>
    [Inject] private ILocalizationService _LocalizationService { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] private SqlSugarRepository<SystemMenu> _Menu { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] private SqlSugarRepository<SystemRoleMenu> _RoleMenu { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] private SqlSugarRepository<SystemUser> _User { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] private SqlSugarRepository<SystemSetting> _Setting { get; set; }
    /// <summary>
    /// 
    /// </summary>
    private EventHandler<CultureInfo> _LocalizationChanged;
    /// <summary>
    /// 
    /// </summary>
    [Inject] private NavigationManager _NavigationManager { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] private IJSRuntime _JSRuntime { get; set; }
    /// <summary>
    /// 
    /// </summary>
    private string CopyRight;
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected override async Task OnInitializedAsync()
    {
        _LocalizationChanged = (sender, args) => InvokeAsync(StateHasChanged);
        _LocalizationService.LanguageChanged += _LocalizationChanged;

        Global.Menus = await _Menu.AsQueryable()
            .OrderBy(x => x.Sort)
            .ToTreeAsync(x => x.Children, x => x.ParentId, 0);

        if (Global.CurrentUser != null)
        {
            var key = $"menu_{Global.CurrentUser.RoleId}";
            if (!await Cache.Exist(key))
            {
                var menus = _RoleMenu.AsQueryable()
                    .LeftJoin<SystemMenu>((rm, m) => rm.MenuId == m.Id)
                    .Where((rm, m) => rm.RoleId == Global.CurrentUser.RoleId)
                    .OrderBy((rm, m) => m.Sort)
                    .Select((rm, m) => new SystemMenu()
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Path = m.Path,
                        Icon = m.Icon,
                        ParentId = m.ParentId,
                        Sort = m.Sort
                    })
                    .ToList();

                var menuTree = BuildMenuTree(menus, 0);
                _MenuData = menuTree.Select(ConvertToMenuDataItem).ToArray();

                Cache.Set(key, _MenuData);
            }
            else
            {
                _MenuData = await Cache.Get<MenuDataItem[]>(key);
            }
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {

        }
    }

    private List<SystemMenu> BuildMenuTree(List<SystemMenu> menus, int parentId)
    {
        var list = menus
            .Where(m => m.ParentId == parentId)
            .Select(m => new SystemMenu
            {
                Id = m.Id,
                Name = m.Name,
                Path = m.Path,
                Icon = m.Icon,
                ParentId = m.ParentId,
                Sort = m.Sort,
                Children = BuildMenuTree(menus, m.Id)
            })
            .ToList();
        if (list.Count == 0)
        {
            return null;//这里必须返回null，否则会导致页面菜单无法点击
        }
        return list;
    }

    private MenuDataItem ConvertToMenuDataItem(SystemMenu menu)
    {
        return new MenuDataItem
        {
            Path = menu.Path,
            Name = menu.Name,
            Key = menu.Key,
            Icon = menu.Icon,
            Children = menu.Children?.Select(ConvertToMenuDataItem).ToArray()
        };
    }


    private List<MenuDataItem> BuildMenuTreeAndConvert(List<SystemMenu> menus, int parentId)
    {
        var list = menus
            .Where(m => m.ParentId == parentId)
            .Select(m => new MenuDataItem
            {
                Path = m.Path,
                Name = m.Name,
                Key = m.Key,
                Icon = m.Icon,
                Children = BuildMenuTreeAndConvert(menus, m.Id).ToArray()
            })
            .ToList();

        return list.Count == 0 ? null : list;
    }

    public void Dispose()
    {
        _LocalizationService.LanguageChanged -= _LocalizationChanged;
    }

}