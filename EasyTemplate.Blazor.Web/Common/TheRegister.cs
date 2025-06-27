using Blazored.LocalStorage;
using EasyTemplate.Blazor.Web.Components;
using EasyTemplate.Page.Common;
using EasyTemplate.Service.Common;
using EasyTemplate.Tool;
using EasyTemplate.Tool.Util;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace EasyTemplate.Blazor.Web.Common;

public static class TheRegister
{
    /// <summary>
    /// 注册配置
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static WebApplicationBuilder? RegistService(this WebApplicationBuilder? builder)
    {
        var configuration = builder.Configuration;

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddConfiguration();
        builder.Services.AddLocalLog();
        builder.Services.AddSqlSugar();
        builder.Services.AddRedis();

        builder.Services.AddAntDesign();
        builder.Services.AddInteractiveStringLocalizer();
        builder.Services.AddLocalization();

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Policy", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/account/login";     // 登录页路径
                options.AccessDeniedPath = "/error/404"; // 未授权页路径
                options.SlidingExpiration = true; // 滑动过期
            });

        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();

        builder.Services.AddAuthorization();

        builder.Services.Configure<ProSettings>(Setting.GetSection("ThemeSettings"));
        Global.SystemName = Setting.Get<string>("ThemeSettings:Title");

        builder.Services.AddControllers().AddNewtonsoftJson();//不加该注册，api传参易报错
        builder.Services.AddEndpointsApiExplorer();
        
        builder.Services.AddSwaggerGen(options =>
        {
            typeof(ApiGroupNames).GetFields().Skip(1).ToList().ForEach(f =>
            {
                //获取枚举值上的特性
                var info = f.GetCustomAttributes(typeof(GroupInfoAttribute), false).OfType<GroupInfoAttribute>().FirstOrDefault();
                options.SwaggerDoc(f.Name, new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = info?.Title,
                    Version = info?.Version,
                    Description = info?.Description
                });
            });
            options.SwaggerDoc("all", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "全部"
            });
            options.DocInclusionPredicate((docName, apiDescription) =>
            {
                if (docName != "all")
                {
                    return apiDescription.GroupName == docName;
                }
                return true;
            });
            options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme()
            {
                Description = "请输入正确的Token格式：Bearer token",
                Name = "Authorization",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            //options.DocInclusionPredicate((docName, description) => true);
            options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement() {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference()
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[]{ }
                    }
                });
            var baseDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var xmlFile = System.AppDomain.CurrentDomain.FriendlyName + ".xml";
            var xmlPath = Path.Combine(baseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
        });

        //将类库中razor注册到应用中
        builder.Services.AddAssembly();

        builder.Services.AddDynamicController<ServiceLocalSelectController, ServiceActionRouteFactory>();

        //builder.Services.AddCascadingAuthenticationState();
        //builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
        builder.Services.AddScoped<CustomAuthenticationStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(implementationFactory => implementationFactory.GetRequiredService<CustomAuthenticationStateProvider>());

        // 注册 Blazored.LocalStorage 服务
        builder.Services.AddBlazoredLocalStorage();

        return builder;
    }

    /// <summary>
    /// 注册应用
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static WebApplicationBuilder? RegistApp(this WebApplicationBuilder? builder)
    {
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
        }

        app.UseStatusCodePagesWithReExecute("/error/{0}");

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            //遍历ApiGroupNames所有枚举值生成接口文档，Skip(1)是因为Enum第一个FieldInfo是内置的一个Int值
            typeof(ApiGroupNames).GetFields().Skip(1).ToList().ForEach(f =>
            {
                //获取枚举值上的特性
                var info = f.GetCustomAttributes(typeof(GroupInfoAttribute), false).OfType<GroupInfoAttribute>().FirstOrDefault();
                options.SwaggerEndpoint($"/swagger/{f.Name}/swagger.json", info != null ? info.Title : f.Name);

            });
            options.SwaggerEndpoint("/swagger/all/swagger.json", "全部");
        });

        app.UseStaticFiles();
        
        app.UseCors("Policy");

        app.UseDynamicWebApi();

        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.MapControllers();
        
        app.Run();
        return builder;
    }

    private static FileExtensionContentTypeProvider GetFileExtensionContentTypeProvider()
    {
        var provider = new FileExtensionContentTypeProvider();
        provider.Mappings[".iec"] = "application/octet-stream";
        provider.Mappings[".patch"] = "application/octet-stream";
        provider.Mappings[".apk"] = "application/vnd.android.package-archive";
        provider.Mappings[".pem"] = "application/x-x509-user-cert";
        provider.Mappings[".gzip"] = "application/x-gzip";
        provider.Mappings[".7zip"] = "application/zip";
        provider.Mappings[".jpg2"] = "image/jp2";
        provider.Mappings[".et"] = "application/kset";
        provider.Mappings[".dps"] = "application/ksdps";
        provider.Mappings[".cdr"] = "application/x-coreldraw";
        provider.Mappings[".shtml"] = "text/html";
        provider.Mappings[".php"] = "application/x-httpd-php";
        provider.Mappings[".php3"] = "application/x-httpd-php";
        provider.Mappings[".php4"] = "application/x-httpd-php";
        provider.Mappings[".phtml"] = "application/x-httpd-php";
        provider.Mappings[".pcd"] = "image/x-photo-cd";
        provider.Mappings[".bcmap"] = "application/octet-stream";
        provider.Mappings[".properties"] = "application/octet-stream";
        provider.Mappings[".m3u8"] = "application/x-mpegURL";
        return provider;
    }
}

internal class ServiceLocalSelectController : ISelectController
{
    public bool IsController(Type type)
    {
        return type.IsPublic && type.GetCustomAttribute<DynamicControllerAttribute>() != null;
    }
}

internal class ServiceActionRouteFactory : IActionRouteFactory
{
    public string CreateActionRouteModel(string areaName, string controllerName, ActionModel action)
    {
        var controllerType = action.ActionMethod.DeclaringType;
        var serviceAttribute = controllerType.GetCustomAttribute<DynamicControllerAttribute>();

        var _controllerName = serviceAttribute.ServiceName == string.Empty ? controllerName.Replace("Service", "") : serviceAttribute.ServiceName.Replace("Service", "");

        return $"api/{_controllerName.Replace("Service", "")}/{action.ActionName.Replace("Async", "")}";
    }
}