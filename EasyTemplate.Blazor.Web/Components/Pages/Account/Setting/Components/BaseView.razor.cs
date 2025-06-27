using EasyTemplate.Tool;

namespace EasyTemplate.Blazor.Web.Components.Pages.Account.Setting;

public partial class BaseView
{
    [Inject] private SqlSugarRepository<SystemUser> _user { get; set; }
    [Inject] private IMessageService MessageService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    private void HandleFinish()
    {
        var flag = _user.AsUpdateable(Global.CurrentUser)
            .UpdateColumns(x => new { x.NickName, x.Email, x.Mobile, x.Address, x.Signature })
            .Where(x => x.Id == Global.CurrentUser.Id)
            .ExecuteCommand() > 0;
        if (flag)
            MessageService.Info("保存成功");
        else
            MessageService.Error("保存失败");
    }

    void OnSingleCompleted(UploadInfo fileinfo)
    {
        if (fileinfo.FileList?.Count > 0)
        {
            var res = fileinfo.FileList[0].Response.ToEntity<Tool.Entity.Result>();
            if (!res.IsSuccess)
            {
                return;
            }
            fileinfo.File.Url = res.Data.ToString();
            _user.AsUpdateable()
                .SetColumns(x => x.Avatar == fileinfo.File.Url)
                .Where(x => x.Id == Global.CurrentUser.Id)
                .ExecuteCommand();
            Global.CurrentUser = _user.GetFirst(x => x.Id == Global.CurrentUser.Id);
        }
    }
}