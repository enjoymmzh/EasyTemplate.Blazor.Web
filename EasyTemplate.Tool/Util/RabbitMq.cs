using System.Text;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace EasyTemplate.Tool;

public static class RabbitMq
{
    /// <summary>
    /// 队列初始化
    /// </summary>
    /// <param name="services"></param>
    public static void AddRabbitMq(this IServiceCollection services)
    {
        services.AddSingleton<IRabbitMQProducer, RabbitMQProducer>();
    }
}

/// <summary>
/// 定义生产者接口
/// </summary>
public interface IRabbitMQProducer
{
    // 定义方法，如发送和接收消息
    bool SendMessage(string message);
}

/// <summary>
/// 定义生产者实现
/// </summary>
public class RabbitMQProducer : IRabbitMQProducer
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMQProducer()
    {
        var factory = new ConnectionFactory
        {
            HostName = Setting.Get<string>("rabbitMQ:host"), // RabbitMQ服务器地址
            UserName = Setting.Get<string>("rabbitMQ:user"), // 用户名
            Password = Setting.Get<string>("rabbitMQ:password"), // 密码
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        //direct：根据 routingKey 将消息传递到队列。
        //topic：有点复杂。根据消息路由键与用于将队列绑定到交换器的模式之间的匹配将消息路由到一个或多个队列。
        //headers：?
        //fanout：只要绑定即可，不需要理会路由。
        //定义交换器
        _channel.ExchangeDeclare(Setting.Get<string>("rabbitMQ:exchange"), ExchangeType.Fanout);

        var aaa = Setting.Get<bool>("rabbitMQ:durable");

        // 定义队列
        _channel.QueueDeclare(
            queue: "myqueue",//队列的名称
            durable: Setting.Get<bool>("rabbitMQ:durable"),//设置是否持久化。持久化的队列会存盘，在服务器重启的时候可以保证不丢失相关信息

            // 连接关闭时被删除该队列
            exclusive: false,//设置是否排他。如果一个队列被声明为排他队列，该队列仅对首次声明它的连接可见，并在连接断开时自动删除,该配置是基于 IConnection 的，同一个 IConnection 创建的不同通道 (IModel) ，也会遵守此规则
            autoDelete: false,//设置是否自动删除。当最后一个消费者(如果有的话)退订时，是否应该自动删除这个队列，自动删除的前提是至少有一个消费者连接到这个队列，之后所有与这个队列连接的消费者都断开时，才会自动删除
            arguments: null//设置队列的其他一些参数，如队列的消息过期时间等
            );//如果队列已经存在，不需要再执行 QueueDeclare()。重复调用 QueueDeclare()，如果参数相同，不会出现副作用，已经推送的消息也不会出问题。但是，如果 QueueDeclare() 参数如果跟已存在的队列配置有差异，则可能会报错。
        _channel.QueueBind(queue: "myqueue", exchange: Setting.Get<string>("rabbitMQ:exchange"), routingKey: string.Empty);
    }

    /// <summary>
    /// 生产者发送数据到队列
    /// </summary>
    /// <param name="message"></param>
    public bool SendMessage(string message)
    {
        try
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(Setting.Get<string>("rabbitMQ:exchange"), string.Empty, null, body);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex);
            return false;
        }
    }

}
