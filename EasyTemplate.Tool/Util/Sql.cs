using System.Collections;
using System.Reflection;
using EasyTemplate.Tool.Entity;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace EasyTemplate.Tool;

public static class Sql
{
    /// <summary>
    /// SqlSugar 上下文初始化
    /// </summary>
    /// <param name="services"></param>
    public static void AddSqlSugar(this IServiceCollection services)
    {
        var scope = Connect();
        services.AddSingleton<ISqlSugarClient>(scope);//单例注册
        services.AddScoped(typeof(SqlSugarRepository<>)); //仓储注册

        //初始化数据库表结构及种子数据
        InitDatabase(scope);
    }

    /// <summary>
    /// 初始化数据库
    /// </summary>
    /// <param name="db"></param>
    /// <param name="config"></param>
    public static void InitDatabase(SqlSugarScope db)
    {
        StaticConfig.CodeFirst_MySqlCollate = "utf8mb4_unicode_ci";
        db.DbMaintenance.CreateDatabase();

        var sugar_tables = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => type.GetCustomAttributes<SugarTable>().Any());
        var create_seed = Setting.Get<bool>("dbConnection:connectionConfigs:0:seedSettings:enableInitSeed");
        foreach (var sugar_table in sugar_tables)
        {
            db.CodeFirst.InitTables(sugar_table);
            if (create_seed)
            {
                CreateSeedData(sugar_table, db);
            }
        }
    }

    /// <summary>
    /// 连接数据库并获取实例
    /// </summary>
    /// <returns></returns>
    public static SqlSugarScope Connect()
    {
        var enableUnderLine = Setting.Get<bool>("DbConnection:ConnectionConfigs:0:DbSettings:EnableUnderLine");
        var dbtypeString = Setting.Get<string>("dbConnection:connectionConfigs:0:dbType");
        if (!Enum.TryParse(dbtypeString, true, out DbType dbtype))
        {
            throw new ArgumentException($"无效的数据库类型: {dbtypeString}");
        }

        var option = new ConnectionConfig()
        {
            ConfigId = Global.ConfigId,
            //ConnectionString = "Data Source=localhost;Port=3306;Initial Catalog=the_manage_system;Persist Security Info=True;User ID=root;Password=123456;Pooling=True;charset=utf8mb4;MAX Pool Size=200;Min Pool Size=1;Connection Lifetime=30;AllowLoadLocalInfile=true;", //Global.connectionString,Data Source=filename;
            ConnectionString = Global.ConnectionString,
            DbType = dbtype,
            InitKeyType = InitKeyType.Attribute,
            IsAutoCloseConnection = true,
            ConfigureExternalServices = new ConfigureExternalServices
            {
                EntityNameService = (type, entity) => //处理表
                {
                    entity.IsDisabledDelete = true; //禁止删除非sqlsugar创建的列
                                                    //只处理贴了特性[SugarTable]表
                    if (!type.GetCustomAttributes<SugarTable>().Any())
                    {
                        return;
                    }

                    if (enableUnderLine && !entity.DbTableName.Contains('_'))
                    {
                        entity.DbTableName = UtilMethods.ToUnderLine(entity.DbTableName); //驼峰转下划线
                    }
                },
                EntityService = (type, column) => //处理列
                {
                    if (!string.IsNullOrWhiteSpace(column.DbColumnName))
                    {
                        if (enableUnderLine && !column.DbColumnName.Contains('_'))
                        {
                            column.DbColumnName = UtilMethods.ToUnderLine(column.DbColumnName); //驼峰转下划线
                        }

                        //只处理贴了特性[SugarColumn]列
                        if (!type.GetCustomAttributes<SugarColumn>().Any())
                        {
                            return;
                        }
                        if (new NullabilityInfoContext().Create(type).WriteState is NullabilityState.Nullable)
                        {
                            column.IsNullable = true;
                        }
                    }

                }
            },
        };

        var scope = new SqlSugarScope(option, db =>
        {
            db.Aop.DataExecuting = (oldValue, entityInfo) =>
            {
                switch (entityInfo.OperationType)
                {
                    case DataFilterType.UpdateByObject:
                        {
                            if (entityInfo.PropertyName == nameof(EntityBase.UpdateTime))
                                entityInfo.SetValue(DateTime.Now);
                            else if (entityInfo.PropertyName == nameof(EntityBase.UpdateUserId))
                            {
                                var updateUserId = ((dynamic)entityInfo.EntityValue).UpdateUserId;
                                if (updateUserId == null)
                                {
                                    var value = Global.UserId > 0 ? Global.UserId : 1;
                                    entityInfo.SetValue(value);
                                }
                            }
                        }
                        break;
                    case DataFilterType.InsertByObject:
                        {
                            if (entityInfo.PropertyName == nameof(EntityBase.CreateTime))
                                entityInfo.SetValue(DateTime.Now);
                            else if (entityInfo.PropertyName == nameof(EntityBase.CreateUserId))
                            {
                                var createUserId = ((dynamic)entityInfo.EntityValue).CreateUserId;
                                if (createUserId == null)
                                {
                                    var value = Global.UserId > 0 ? Global.UserId : 1;
                                    entityInfo.SetValue(value);
                                }
                            }
                        }
                        break;
                    default:
                    case DataFilterType.DeleteByObject:
                        break;
                }
            };
            db.QueryFilter.AddTableFilter<IDeletedFilter>(u => u.IsDelete == false);
        });

        return scope;
    }

    /// <summary>
    /// 创建种子数据
    /// </summary>
    /// <param name="sugar_table"></param>
    /// <returns></returns>
    public static bool CreateSeedData(Type sugar_table, SqlSugarScope db)
    {
        try
        {
            var type = Type.GetType($"{sugar_table.Namespace}.{sugar_table.Name}SeedData");
            if (type != null)
            {
                var instance = Activator.CreateInstance(type);
                var hasDataMethod = type.GetMethod("Generate");
                var seedData = ((IEnumerable)hasDataMethod?.Invoke(instance, null))?.Cast<object>();
                if (seedData != null)
                {
                    var entityType = type.GetInterfaces().First().GetGenericArguments().First();
                    var entityInfo = db.EntityMaintenance.GetEntityInfo(entityType);
                    if (entityInfo.Columns.Any(u => u.IsIdentity))
                    {
                        //按主键进行批量增加和更新
                        var storage = db.StorageableByObject(seedData.ToList()).ToStorage();
                        storage.AsInsertable.ExecuteCommand();
                        storage.AsUpdateable.ExecuteCommand();
                    }
                    else
                    {
                        // 无主键则只进行插入
                        if (!db.Queryable(entityInfo.DbTableName, entityInfo.DbTableName).Any())
                        {
                            db.InsertableByObject(seedData.ToList()).ExecuteCommand();
                        }
                    }
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex);
            return false;
        }
    }

    /// <summary>
    /// 仓储假删除
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="db"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static bool FakeDelete<T>(this ISqlSugarClient db, T entity) where T : EntityBase, new()
    {
        return db.Updateable(entity).ReSetValue(x => { x.IsDelete = true; })
            .IgnoreColumns(ignoreAllNullColumns: true)
            .UpdateColumns(x => new { x.IsDelete, x.UpdateTime, x.UpdateUserId }, true) //允许更新的字段-AOP拦截自动设置UpdateTime、UpdateUserId
            .ExecuteCommand() > 0;
    }
    /// <summary>
    /// 仓储假删除
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="db"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static async Task<bool> FakeDeleteAsync<T>(this ISqlSugarClient db, T entity) where T : EntityBase, new()
    {
        return await db.Updateable(entity).ReSetValue(x => { x.IsDelete = true; })
            .IgnoreColumns(ignoreAllNullColumns: true)
            .UpdateColumns(x => x.IsDelete, true) //允许更新的字段-AOP拦截自动设置UpdateTime、UpdateUserId
            .ExecuteCommandAsync() > 0;
    }
}

public class SqlSugarRepository<T> : SimpleClient<T> where T : class, new()
{
    public SqlSugarRepository(ISqlSugarClient db)
    {
        //var iTenant = Sql.ITenant;

        ////若实体贴有系统表特性，则返回默认库连接
        //if (typeof(T).IsDefined(typeof(MainTableAttribute), false))
        //{
        //    base.Context = iTenant.GetConnectionScope(Global.mainConfigId);
        //    return;
        //}

        //if (typeof(T).IsDefined(typeof(SubTableAttribute), false))
        //{
        //    if (!iTenant.IsAnyConnection(Global.subConfigId))
        //    {
        //        var aaa = 1;
        //    }
        //        base.Context = iTenant.GetConnectionScope(Global.subConfigId);
        //    return;
        //}

        base.Context = db;
    }
}

/// <summary>
/// 主表
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class MainTableAttribute : Attribute
{

}

/// <summary>
/// 子表
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class SubTableAttribute : Attribute
{

}
