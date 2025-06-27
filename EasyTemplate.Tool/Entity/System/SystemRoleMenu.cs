using System.ComponentModel.DataAnnotations;
using SqlSugar;
using static EasyTemplate.Tool.Entity.PublicEnum;

namespace EasyTemplate.Tool.Entity;

/// <summary>
/// 系统角色与菜单对应关系
/// </summary>
[SugarTable(null, "系统角色与菜单对应关系")]
public class SystemRoleMenu
{
    /// <summary>
    /// 角色Id
    /// </summary>
    [SugarColumn(ColumnDescription = "角色Id", DefaultValue ="0", IsNullable = true)]
    public int RoleId { get; set; }

    /// <summary>
    /// 菜单Id
    /// </summary>
    [SugarColumn(ColumnDescription = "菜单Id", DefaultValue = "0", IsNullable = true)]
    public int MenuId { get; set; }

}

public class SystemRoleMenuSeedData : ISeedData<SystemRoleMenu>
{
    public IEnumerable<SystemRoleMenu> Generate()
        =>
        [
            new SystemRoleMenu() { RoleId= 1, MenuId=1},
            new SystemRoleMenu() { RoleId= 1, MenuId=2},
            new SystemRoleMenu() { RoleId= 1, MenuId=3},
            new SystemRoleMenu() { RoleId= 1, MenuId=4},
            new SystemRoleMenu() { RoleId= 1, MenuId=5},
            new SystemRoleMenu() { RoleId= 1, MenuId=6},
            new SystemRoleMenu() { RoleId= 1, MenuId=7},
            new SystemRoleMenu() { RoleId= 1, MenuId=8},
            new SystemRoleMenu() { RoleId= 1, MenuId=9},
            new SystemRoleMenu() { RoleId= 1, MenuId=10},
            new SystemRoleMenu() { RoleId= 1, MenuId=11},
            new SystemRoleMenu() { RoleId= 1, MenuId=12},
            new SystemRoleMenu() { RoleId= 1, MenuId=13},
            new SystemRoleMenu() { RoleId= 1, MenuId=14},
            new SystemRoleMenu() { RoleId= 1, MenuId=15},
            new SystemRoleMenu() { RoleId= 1, MenuId=16},
            new SystemRoleMenu() { RoleId= 1, MenuId=17},
            new SystemRoleMenu() { RoleId= 2, MenuId=1},
        ];
}
