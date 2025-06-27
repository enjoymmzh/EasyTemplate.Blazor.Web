using EasyTemplate.Tool;
using EasyTemplate.Tool.Util;
using Mapster;
using Microsoft.JSInterop;

namespace EasyTemplate.Blazor.Web.Components;

public partial class RightContent
{
    
    private NoticeIconData[] _notifications = { };
    private NoticeIconData[] _messages = { };
    private int _count = 0;

    private AvatarMenuItem[] AvatarMenuItems =>
        [
            //new() { Key = "center", IconType = "user", Option = L["menu.account.center"]},
            new() { Key = "setting", IconType = "setting", Option = L["menu.account.settings"] },
            new() { IsDivider = true },
            new() { Key = "logout", IconType = "logout", Option = L["menu.account.logout"]}
        ];

    private SystemUser _currentUser = new SystemUser();
    /// <summary>
    /// 
    /// </summary>
    [Inject] protected NavigationManager NavigationManager { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] protected MessageService MessageService { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] private IStringLocalizer<I18n> L { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] private ILocalizationService LocalizationService { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] private IWebHostEnvironment WebHostEnvironment { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] private IHttpContextAccessor _accessor { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] private IJSRuntime JSRuntime { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] private SqlSugarRepository<SystemLogLogin> _log { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] private SqlSugarRepository<SystemNotification> _notices { get; set; }

    protected override async Task OnInitializedAsync()
    {
        NavigationManager.RedirectLogin();
        await base.OnInitializedAsync();
        SetClassMap();

        _currentUser = Global.CurrentUser;

        // 配置 Mapster 映射规则
        TypeAdapterConfig<SystemNotification, NoticeIconData>.NewConfig()
            .Map(dest => dest.Key, src => src.Id)
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Description, src => src.Info)
            .Map(dest => dest.Read, src => src.Seen)
            .Map(dest => dest.Datetime, src => src.CreateTime);

        await Notifications();
    }

    private async Task Notifications()
    {
        var notices = await _notices.AsQueryable().ToArrayAsync();
        _notifications = notices.Where(x => x.NotifyType == PublicEnum.NotifyType.Notify).Adapt<NoticeIconData[]>();
        _messages = notices.Where(x => x.NotifyType == PublicEnum.NotifyType.Info).Adapt<NoticeIconData[]>();
        _count = notices.Count(x => x.Seen == false);
    }

    protected void SetClassMap()
    {
        ClassMapper
            .Clear()
            .Add("right");
    }

    public async Task HandleSelectUser(MenuItem item)
    {
        switch (item.Key)
        {
            case "center":
                NavigationManager.NavigateTo("/account/center");
                break;
            case "setting":
                NavigationManager.NavigateTo("/account/setting");
                break;
            case "logout":
                var localStorageHelper = new LocalStorage(JSRuntime);
                await localStorageHelper.RemoveLocalStorage(LocalStorage.AutoLogin);
                await localStorageHelper.RemoveLocalStorage(LocalStorage.UserInfo);

                var host = _accessor.GetHost();
                _log.Insert(new SystemLogLogin() { Info = $"用户【{Global.CurrentUser.Account}】登出账号", UserId = Global.CurrentUser.Id, CreateTime = DateTime.Now });
                NavigationManager.NavigateTo("/account/login");

                Global.CurrentUser = null;
                break;
        }
    }

    public async Task Seen(string key)
    {
        _notices.AsUpdateable()
            .SetColumns(x => x.Seen == true)
            .Where(x => x.Id == Convert.ToInt32(key))
            .ExecuteCommand();

        await Notifications();
    }

    public void HandleSelectLang(MenuItem item)
    {
        LocalizationService.SetLanguage(CultureInfo.GetCultureInfo(item.Key));
    }

    public async Task HandleClear(string key)
    {
        switch (key)
        {
            case "notification":
                _notices.AsDeleteable().Where(x => x.NotifyType == PublicEnum.NotifyType.Notify).ExecuteCommand();
                _notifications = [];
                break;
            case "message":
                _notices.AsDeleteable().Where(x => x.NotifyType == PublicEnum.NotifyType.Info).ExecuteCommand();
                _messages = [];
                break;
        }
        await MessageService.Success($"清空了{key}");

        await Notifications();
    }

    public async Task HandleViewMore(string key)
    {
        await MessageService.Info("点击查看更多");
    }
}
