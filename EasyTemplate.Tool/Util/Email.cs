using System.ComponentModel;
using System.Net.Mail;
using System.Net;
using System.Text;
using Masuit.Tools.Models;
using Masuit.Tools;

namespace EasyTemplate.Tool;

public class Email : Disposable
{
    private Action<string> _actionSendCompletedCallback;

    //
    // 摘要:
    //     发件人用户名
    public EmailAddress Username { get; set; }

    //
    // 摘要:
    //     发件人邮箱密码
    public string Password { get; set; }

    //
    // 摘要:
    //     发送服务器端口号，默认25
    public int SmtpPort { get; set; } = 25;


    //
    // 摘要:
    //     发送服务器地址
    public string SmtpServer { get; set; }

    //
    // 摘要:
    //     邮件标题
    public string Subject { get; set; }

    //
    // 摘要:
    //     邮件正文
    public string Body { get; set; }

    //
    // 摘要:
    //     收件人，多个收件人用英文逗号隔开
    public string Tos { get; set; }

    public List<string> CC { get; set; } = new List<string>();


    public List<string> BCC { get; set; } = new List<string>();


    //
    // 摘要:
    //     是否启用SSL，默认已启用
    public bool EnableSsl { get; set; } = true;


    //
    // 摘要:
    //     附件
    public List<Attachment> Attachments { get; set; } = new List<Attachment>();


    private MailMessage MailMessage => GetClient();

    public Email()
    {
        SmtpServer = "";// SMTP服务器
        SmtpPort = 587;// SMTP服务器端口
        EnableSsl = true;//使用SSL
        Username = "";// 邮箱用户名
        Password = "";// 邮箱密码

        SmtpClient = new SmtpClient()
        {
            UseDefaultCredentials = false,
            EnableSsl = EnableSsl,
            Host = SmtpServer,
            Port = SmtpPort,
            Credentials = new NetworkCredential(Username, Password),
            DeliveryMethod = SmtpDeliveryMethod.Network
        };
        SmtpClient.SendCompleted += SendCompletedCallback;
    }

    private SmtpClient SmtpClient { get; set; }

    //
    // 摘要:
    //     邮件消息对象
    private MailMessage GetClient()
    {
        if (string.IsNullOrEmpty(Tos))
        {
            return null;
        }

        MailMessage mailMessage = new MailMessage();
        string[] array = Tos.Split(',');
        foreach (string addresses in array)
        {
            mailMessage.To.Add(addresses);
        }

        foreach (string item in CC)
        {
            mailMessage.CC.Add(item);
        }

        foreach (string item2 in BCC)
        {
            mailMessage.Bcc.Add(item2);
        }

        mailMessage.From = new MailAddress(Username, Username);
        mailMessage.Subject = Subject;
        mailMessage.Body = Body;
        mailMessage.IsBodyHtml = true;
        mailMessage.BodyEncoding = Encoding.UTF8;
        mailMessage.SubjectEncoding = Encoding.UTF8;
        mailMessage.Priority = MailPriority.High;
        foreach (Attachment item3 in Attachments.AsNotNull())
        {
            mailMessage.Attachments.Add(item3);
        }

        return mailMessage;
    }

    //
    // 摘要:
    //     使用异步发送邮件
    //
    // 参数:
    //   completedCallback:
    //     邮件发送后的回调方法
    public void SendAsync(Action<string> completedCallback)
    {
        if (MailMessage != null)
        {
            _actionSendCompletedCallback = completedCallback;

            SmtpClient.SendAsync(MailMessage, "true");
        }
    }

    //
    // 摘要:
    //     使用同步发送邮件
    public void Send()
    {
        if (MailMessage != null)
        {
            SmtpClient.Send(MailMessage);
            Dispose(disposing: true);
        }
    }

    //
    // 摘要:
    //     异步操作完成后执行回调方法
    //
    // 参数:
    //   sender:
    //
    //   e:
    private void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
    {
        if (_actionSendCompletedCallback != null)
        {
            string obj = (e.Cancelled ? "异步操作取消" : ((e.Error == null) ? ((string)e.UserState) : $"UserState:{(string)e.UserState},Message:{e.Error}"));
            _actionSendCompletedCallback(obj);
            Dispose(disposing: true);
        }
    }

    //
    // 摘要:
    //     释放
    //
    // 参数:
    //   disposing:
    public override void Dispose(bool disposing)
    {
        MailMessage?.Dispose();
        SmtpClient?.Dispose();
        Attachments.ForEach(delegate (Attachment a)
        {
            a.Dispose();
        });
    }
}

public abstract class Disposable : IDisposable
{
    private bool isDisposed;

    //
    // 摘要:
    //     终结器
    ~Disposable()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        if (!isDisposed)
        {
            Dispose(disposing: true);
            isDisposed = true;
            GC.Collect();
        }
    }

    //
    // 摘要:
    //     释放
    //
    // 参数:
    //   disposing:
    public abstract void Dispose(bool disposing);
}
