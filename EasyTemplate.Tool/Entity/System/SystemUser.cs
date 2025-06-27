using System.ComponentModel.DataAnnotations;
using AntDesign;
using SqlSugar;
using static EasyTemplate.Tool.Entity.PublicEnum;

namespace EasyTemplate.Tool.Entity;

/// <summary>
/// 系统用户
/// </summary>
[SugarTable(null, "系统用户")]
public class SystemUser: EntityBase
{
    /// <summary>
    /// 账号
    /// </summary>
    [SugarColumn(ColumnDescription = "账号", Length = 32, IsNullable = true)]
    public string Account { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [SugarColumn(ColumnDescription = "密码", Length = 32, IsNullable = true)]
    public string Password { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    [SugarColumn(ColumnDescription = "昵称", Length = 32, IsNullable = true)]
    public string NickName { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    [SugarColumn(ColumnDescription = "头像", DefaultValue = "/assets/default_avatar.png", IsNullable = true)]
    public string Avatar { get; set; }

    /// <summary>
    /// 手机
    /// </summary>
    [SugarColumn(ColumnDescription = "手机", IsNullable = true)]
    public string Mobile { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    [SugarColumn(ColumnDescription = "邮箱", IsNullable = true)]
    public string Email { get; set; }

    /// <summary>
    /// 地区id
    /// </summary>
    [SugarColumn(ColumnDescription = "地区id", IsNullable = true)]
    public string AreaIds { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    [SugarColumn(ColumnDescription = "地址", IsNullable = true)]
    public string Address { get; set; }

    /// <summary>
    /// 签名
    /// </summary>
    [SugarColumn(ColumnDescription = "签名", IsNullable = true)]
    public string Signature { get; set; }
    
    /// <summary>
    /// 角色id
    /// </summary>
    [SugarColumn(ColumnDescription = "角色id", IsNullable = true)]
    public int RoleId { get; set; }

    /// <summary>
    /// 部门id
    /// </summary>
    [SugarColumn(ColumnDescription = "部门id", IsNullable = true)]
    public int DepartmentId { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [SugarColumn(ColumnDescription = "是否启用", DefaultValue ="1", IsNullable = true)]
    public bool Enabled { get; set; }

    /// <summary>
    /// 绑定谷歌账号
    /// </summary>
    [SugarColumn(ColumnDescription = "绑定谷歌账号", IsNullable = true)]
    public string BindingGoogle { get; set; }

    /// <summary>
    /// 绑定X账号
    /// </summary>
    [SugarColumn(ColumnDescription = "绑定X账号", IsNullable = true)]
    public string BindingX { get; set; }

    /// <summary>
    /// 绑定Facebook账号
    /// </summary>
    [SugarColumn(ColumnDescription = "绑定Facebook账号", IsNullable = true)]
    public string BindingFacebook { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public DateTime Expired { get; set; }

    /// <summary>
    /// 确认密码
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public string ConfirmPassword { get; set; }

    /// <summary>
    /// 角色名称
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public string RoleName { get; set; }

    /// <summary>
    /// 部门名称
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public string DepartmentName { get; set; }

    /// <summary>
    /// 最后一个区域id，用于级联回显
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public string LastAreadId { get; set; }

    /// <summary>
    /// 地区全拼
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public string Area { get; set; }

}

public class SystemUserSeedData : ISeedData<SystemUser>
{
    public IEnumerable<SystemUser> Generate()
        =>
        [
            new SystemUser() { Id = 1, Account = "admin", Password = "e10adc3949ba59abbe56e057f20f883e", Enabled=true, Avatar="/assets/default_avatar.png", NickName="超级管理员", Mobile="13789756548", AreaIds="37,82,85", Email="asdfkuhuasd@163.com", RoleId=1, DepartmentId=1, CreateTime = DateTime.Now },
            new SystemUser() { Id = 2, Account = "david111", Password = "e10adc3949ba59abbe56e057f20f883e", Enabled=true, Avatar="/assets/default_avatar.png",  NickName="用户1", Mobile="15364878984", Email="12312323@sina.com", RoleId=2, DepartmentId=1,CreateTime = DateTime.Now },
        ];
}

public class LoginInput
{
    [Required] public string Account { get; set; }

    [Required] public string Password { get; set; }

    public string Mobile { get; set; }

    public string Captcha { get; set; }
    public string VerifyCode { get; set; }

    public string LoginType { get; set; }

    public bool AutoLogin { get; set; }
}