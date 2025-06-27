using System.Text;
using CSRedis;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.DependencyInjection;

namespace EasyTemplate.Tool;

public static class Redis
{
    /// <summary>
    /// 使用缓存
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static void AddRedis(this IServiceCollection services)
    {
        //csredis的两种使用方式
        var csredis = new CSRedisClient(Global.CacheConnectionString);
        services.AddSingleton(csredis);
        RedisHelper.Initialization(csredis);

        //基于redis初始化IDistributedCache
        services.AddSingleton<IDistributedCache>(new CSRedisCache(csredis));

        Global.Redis = csredis;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="cacheKey"></param>
    /// <returns></returns>
    public static async Task<bool> ExistsAsync(string cacheKey)
    {
        return await Global.Redis.ExistsAsync(cacheKey);
    }

    /// <summary>
    /// 获取缓存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="cacheKey"></param>
    /// <returns></returns>
    public static async Task<T> GetAsync<T>(string cacheKey)
    {
        var res = await Global.Redis.GetAsync(cacheKey);
        return res == null ? default : res.ToEntity<T>();
    }

    /// <summary>
    /// 设置缓存
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="value"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public static async Task SetAsync(string cacheKey, object value, long time = 604800)
    {
        await Global.Redis.SetAsync(cacheKey, Encoding.UTF8.GetBytes(value.ToJson()), (int)time);
    }

    /// <summary>
    /// 数据自增等操作，基于分布式锁，解决并发问题
    /// </summary>
    /// <param name="lockKey">标识，用于确认哪一条数据</param>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <returns></returns>
    public static async Task SetWithLockAsync(string lockKey, string key, object value)
    {
        var id = Guid.NewGuid().ToString("N");
        var start = DateTime.Now;
        while (true)
        {
            //设置锁1秒后过期
            var success = RedisHelper.Set(lockKey, id, expireSeconds: 1, exists: RedisExistence.Nx);
            if (success) break;

            //超出2秒立即跳出，防止阻塞
            if (DateTime.Now.Subtract(start).TotalSeconds >= 2)
                break;
        }

        try
        {
            await SetAsync(key, value);
        }
        catch { }
        finally
        {
            RedisHelper.Eval("if redis.call('get', KEYS[1]) == ARGV[1] then return redis.call('del', KEYS[1]) else return 0 end", lockKey, id);//释放锁的redis脚本
        }
    }
}
