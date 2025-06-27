using EasyTemplate.Blazor.Web.Common;
using EasyTemplate.Tool;
using EasyTemplate.Tool.Util;
using Microsoft.JSInterop;

namespace EasyTemplate.Blazor.Web.Components.Pages.Account.Login;

//https://www.cnblogs.com/j4587698/p/16531294.html
public partial class Login
{
    /// <summary>
    /// 
    /// </summary>
    private readonly LoginInput _Model = new();
    /// <summary>
    /// 
    /// </summary>
    [Inject] public NavigationManager NavigationManager { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] public MessageService Message { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] private SqlSugarRepository<SystemUser> _user { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] private SqlSugarRepository<SystemLogLogin> _log { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Inject] private CustomAuthenticationStateProvider AuthStateProvider { get; set; }
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
    [Inject] private IMessageService MessageService { get; set; }
    /// <summary>
    /// 
    /// </summary>
    private bool _Loading;
    /// <summary>
    /// 
    /// </summary>
    private bool _LoginLoading;

    protected override async void OnInitialized()
    {
        base.OnInitialized();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _Model.LoginType = "1";
            var localStorageHelper = new LocalStorage(JSRuntime);
            var auto = await localStorageHelper.GetLocalStorage(LocalStorage.AutoLogin);
            _Model.AutoLogin = auto == "True";
            if (_Model.AutoLogin)
            {
                var user = await localStorageHelper.GetLocalStorage(LocalStorage.UserInfo);
                if (!string.IsNullOrWhiteSpace(user))
                {
                    var suser = Crypto.AESDecrypt(user).ToEntity<SystemUser>();
                    var match = _user.AsQueryable()
                        .Where(x => x.Account == suser.Account && x.Password == suser.Password)
                        .First();

                    if (match is not null)
                        ExecuteLogin(suser, true);
                }
            }
        }
    }


    private async Task HandleSubmit()
    {
        _LoginLoading = true;

        try
        {
            SystemUser user = null;
            if (_Model.LoginType == "1")
            {
                if (string.IsNullOrWhiteSpace(_Model.Account) || string.IsNullOrWhiteSpace(_Model.Password))
                {
                    await Message.Error("账号或密码错误");
                    return;
                }

                user = _user.AsQueryable()
                    .Where(x => x.Account == _Model.Account && x.Password == Crypto.MD5Encrypt(_Model.Password))
                    .First();
            }
            else
            {
                if (string.IsNullOrWhiteSpace(_Model.Mobile) || string.IsNullOrWhiteSpace(_Model.VerifyCode))
                {
                    await Message.Error("手机号或验证码错误");
                    return;
                }

                user = _user.AsQueryable()
                    .Where(x => x.Mobile == _Model.Mobile)
                    .First();
            }

            if (user == null)
            {
                await Message.Error("账号或密码错误");
                return;
            }

            var localStorageHelper = new LocalStorage(JSRuntime);
            if (_Model.AutoLogin)
            {
                await localStorageHelper.SetLocalStorage(LocalStorage.AutoLogin, _Model.AutoLogin.ToString());//
            }
            await localStorageHelper.SetLocalStorage(LocalStorage.UserInfo, Crypto.AESEncrypt(new { user.Id, user.Account, user.Password, Expired = DateTime.Now.AddDays(Global.Expired) }.ToJson()));

            ExecuteLogin(user);
        }
        catch (Exception ex)
        {
            await Message.Error(ex.ToString());
        }
        finally
        {
            _LoginLoading = false;
        }
    }

    public async Task ExecuteLogin(SystemUser user, bool auto = false)
    {
        var host = _accessor.GetHost();
        Global.CurrentUser = _user.AsQueryable().Where(x => x.Account == user.Account && x.Password == user.Password).First();
        if (auto)
        {
            _log.Insert(new SystemLogLogin() { Info = $"用户【{Global.CurrentUser.Account}】通过自动登录功能登录账号", UserId = Global.CurrentUser.Id, IpAddress= host, CreateTime = DateTime.Now });
        }
        else
        {
            _log.Insert(new SystemLogLogin() { Info = $"用户【{Global.CurrentUser.Account}】登录账号", UserId = Global.CurrentUser.Id, IpAddress = host, CreateTime = DateTime.Now });
        }

        //AuthStateProvider.SignIn(Global.CurrentUser);

        NavigationManager.NavigateTo("/");
    }

    /// <summary>
    /// 发送短信验证码
    /// </summary>
    private async Task SendSM()
    {
        _Loading = true;
        if (!string.IsNullOrWhiteSpace(_Model?.Mobile))
        {
            await Task.Delay(8000);
        }
        else
        {
            await MessageService.Error("请输入手机号");
        }
        _Loading = false;
    }
}