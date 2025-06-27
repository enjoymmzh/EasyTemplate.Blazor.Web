using System.ComponentModel.DataAnnotations;
using SqlSugar;
using static EasyTemplate.Tool.Entity.PublicEnum;

namespace EasyTemplate.Tool.Entity;

/// <summary>
/// 系统菜单
/// </summary>
[SugarTable(null, "系统菜单")]
public class SystemMenu : EntityBaseLite
{
    /// <summary>
    /// 父级Id
    /// </summary>
    [SugarColumn(ColumnDescription = "父级Id", DefaultValue ="0", IsNullable = true)]
    public int ParentId { get; set; }

    /// <summary>
    /// 路由
    /// </summary>
    [SugarColumn(ColumnDescription = "路由", IsNullable = true)]
    public string Path { get; set; }

    /// <summary>
    /// 菜单名称
    /// </summary>
    [SugarColumn(ColumnDescription = "菜单名称", IsNullable = true)]
    public string Name { get; set; }

    /// <summary>
    /// 键
    /// </summary>
    [SugarColumn(ColumnDescription = "键", IsNullable = true)]
    public string Key { get; set; }

    /// <summary>
    /// 图标，从https://antblazor.com/zh-CN/components/icon获取名称
    /// </summary>
    [SugarColumn(ColumnDescription = "图标，从https://antblazor.com/zh-CN/components/icon获取名称", IsNullable = true)]
    public string Icon { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序", DefaultValue ="0", IsNullable = true)]
    public int Sort { get; set; }

    /// <summary>
    /// 是否必需
    /// </summary>
    [SugarColumn(ColumnDescription = "是否必需", DefaultValue = "0", IsNullable = true)]
    public bool Necessary { get; set; } 

    /// <summary>
    /// 
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public List<SystemMenu> Children { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public bool Checked { get; set; }
}

public class SystemMenuSeedData : ISeedData<SystemMenu>
{
    public IEnumerable<SystemMenu> Generate()
        =>
        [
            new SystemMenu() { Id = 1, ParentId = 0, Path="/", Name="主页", Key="home", Necessary=true, Icon="home", Sort=0, CreateTime = DateTime.Now },
            new SystemMenu() { Id = 2, ParentId = 0, Path="/system", Name="系统管理", Key="system", Icon="setting", Sort=999, CreateTime = DateTime.Now },
            new SystemMenu() { Id = 3, ParentId = 2, Path="/system/setting", Name="系统设置", Key="system.setting", Icon="desktop", Sort=0, CreateTime = DateTime.Now },
            new SystemMenu() { Id = 4, ParentId = 2, Path="/system/user", Name="用户管理", Key="system.user", Icon="idcard", Sort=1, CreateTime = DateTime.Now },
            new SystemMenu() { Id = 5, ParentId = 2, Path="/system/role", Name="角色管理", Key="system.role", Icon="user", Sort=2, CreateTime = DateTime.Now },
            new SystemMenu() { Id = 6, ParentId = 2, Path="/system/department", Name="部门管理", Key="system.department", Icon="apartment", Sort=3, CreateTime = DateTime.Now },
            new SystemMenu() { Id = 7, ParentId = 2, Path="/system/dictionary", Name="字典管理", Key="system.dictionary", Icon="api", Sort=4, CreateTime = DateTime.Now },
            new SystemMenu() { Id = 8, ParentId = 2, Path="/system/menu", Name="菜单管理", Key="system.menu", Icon="menu", Sort=5, CreateTime = DateTime.Now },
            new SystemMenu() { Id = 9, ParentId = 2, Path="/system/area", Name="区域管理", Key="system.area", Icon="block", Sort=6, CreateTime = DateTime.Now },
            new SystemMenu() { Id = 10, ParentId = 2, Path="/system/alllog", Name="日志管理", Key="system.alllog", Icon="file", Sort=7, CreateTime = DateTime.Now },
            new SystemMenu() { Id = 11, ParentId = 10, Path="/system/alllog/log", Name="日志管理", Key="system.log", Icon="file-exclamation", Sort=0, CreateTime = DateTime.Now },
            new SystemMenu() { Id = 12, ParentId = 10, Path="/system/alllog/apilog", Name="接口日志管理", Key="system.apilog", Icon="file-text", Sort=1, CreateTime = DateTime.Now },
            new SystemMenu() { Id = 13, ParentId = 10, Path="/system/alllog/loginlog", Name="登录日志管理", Key="system.loginlog", Icon="file", Sort=2, CreateTime = DateTime.Now },
            new SystemMenu() { Id = 14, ParentId = 2, Path="/system/apidoc", Name="接口文档", Key="system.apidoc", Icon="file-markdown", Sort=8, CreateTime = DateTime.Now },

            new SystemMenu() { Id = 15, ParentId = 0, Path="/app", Name="应用管理", Key="app", Icon="appstore", Sort=1, CreateTime = DateTime.Now },
            new SystemMenu() { Id = 16, ParentId = 15, Path="/app/product", Name="产品管理", Key="app.product", Icon="shopping", Sort=0, CreateTime = DateTime.Now },
            new SystemMenu() { Id = 17, ParentId = 15, Path="/app/order", Name="订单管理", Key="app.order", Icon="snippets", Sort=1, CreateTime = DateTime.Now },

        ];
}
