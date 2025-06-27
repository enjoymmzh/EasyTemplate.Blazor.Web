using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Text.RegularExpressions;

namespace EasyTemplate.Tool.Util;

/// <summary>
/// Add Dynamic WebApi
/// </summary>
public static class DynamicControllerRegister
{
    /// <summary>
    /// Use Dynamic WebApi to Configure
    /// </summary>
    /// <param name="application"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseDynamicWebApi(this IApplicationBuilder application)
    {
        var options = new DynamicWebApiOptions();
        options.Valid();

        AppConsts.DefaultAreaName = options.DefaultAreaName;
        AppConsts.DefaultHttpVerb = options.DefaultHttpVerb;
        AppConsts.DefaultApiPreFix = options.DefaultApiPrefix;
        AppConsts.ControllerPostfixes = options.RemoveControllerPostfixes;
        AppConsts.ActionPostfixes = options.RemoveActionPostfixes;
        AppConsts.FormBodyBindingIgnoredTypes = options.FormBodyBindingIgnoredTypes;
        AppConsts.GetRestFulActionName = options.GetRestFulActionName;
        AppConsts.AssemblyDynamicWebApiOptions = options.AssemblyDynamicWebApiOptions;

        var partManager = application.ApplicationServices.GetRequiredService<ApplicationPartManager>();

        // Add a custom controller checker
        var featureProviders = application.ApplicationServices.GetRequiredService<DynamicWebApiControllerFeatureProvider>();
        partManager.FeatureProviders.Add(featureProviders);

        var mvcOptions = application.ApplicationServices.GetRequiredService<IOptions<MvcOptions>>();
        var dynamicWebApiConvention = application.ApplicationServices.GetRequiredService<DynamicWebApiConvention>();

        mvcOptions.Value.Conventions.Add(dynamicWebApiConvention);

        return application;
    }

    public static IServiceCollection AddDynamicController<TSelectController, TActionRouteFactory>(this IServiceCollection services)
        where TSelectController : class, ISelectController
        where TActionRouteFactory : class, IActionRouteFactory
    {
        services.AddSingleton<ISelectController, TSelectController>();
        services.AddSingleton<IActionRouteFactory, TActionRouteFactory>();
        services.AddSingleton<DynamicWebApiConvention>();
        services.AddSingleton<DynamicWebApiControllerFeatureProvider>();
        return services;
    }

    /// <summary>
    /// Add Dynamic WebApi to Container
    /// </summary>
    /// <param name="services"></param>
    /// <param name="options">configuration</param>
    /// <returns></returns>
    public static IServiceCollection AddDynamicController(this IServiceCollection services, DynamicWebApiOptions options)
    {
        if (options == null)
        {
            throw new ArgumentException(nameof(options));
        }

        options.Valid();

        AppConsts.DefaultAreaName = options.DefaultAreaName;
        AppConsts.DefaultHttpVerb = options.DefaultHttpVerb;
        AppConsts.DefaultApiPreFix = options.DefaultApiPrefix;
        AppConsts.ControllerPostfixes = options.RemoveControllerPostfixes;
        AppConsts.ActionPostfixes = options.RemoveActionPostfixes;
        AppConsts.FormBodyBindingIgnoredTypes = options.FormBodyBindingIgnoredTypes;
        AppConsts.GetRestFulActionName = options.GetRestFulActionName;
        AppConsts.AssemblyDynamicWebApiOptions = options.AssemblyDynamicWebApiOptions;

        var partManager = services.GetSingletonInstanceOrNull<ApplicationPartManager>();

        if (partManager == null)
        {
            throw new InvalidOperationException("\"AddDynamicWebApi\" must be after \"AddMvc\".");
        }

        // Add a custom controller checker
        partManager.FeatureProviders.Add(new DynamicWebApiControllerFeatureProvider(options.SelectController));

        services.Configure<MvcOptions>(o =>
        {
            // Register Controller Routing Information Converter
            o.Conventions.Add(new DynamicWebApiConvention(options.SelectController, options.ActionRouteFactory));
        });

        return services;
    }

    public static IServiceCollection AddDynamicWebApi(this IServiceCollection services)
    {
        return services.AddDynamicController(new DynamicWebApiOptions());
    }

    public static IServiceCollection AddDynamicWebApi(this IServiceCollection services, Action<DynamicWebApiOptions> optionsAction)
    {
        var dynamicWebApiOptions = new DynamicWebApiOptions();

        optionsAction?.Invoke(dynamicWebApiOptions);

        return services.AddDynamicController(dynamicWebApiOptions);
    }

}

public class DynamicWebApiOptions
{
    public DynamicWebApiOptions()
    {
        RemoveControllerPostfixes = new List<string>() { "AppService", "ApplicationService" };
        RemoveActionPostfixes = new List<string>() { "Async" };
        FormBodyBindingIgnoredTypes = new List<Type>() { typeof(IFormFile) };
        DefaultHttpVerb = "POST";
        DefaultApiPrefix = "api";
        AssemblyDynamicWebApiOptions = new Dictionary<Assembly, AssemblyDynamicWebApiOptions>();
    }


    /// <summary>
    /// API HTTP Verb.
    /// <para></para>
    /// Default value is "POST".
    /// </summary>
    public string DefaultHttpVerb { get; set; }

    public string DefaultAreaName { get; set; }

    /// <summary>
    /// Routing prefix for all APIs
    /// <para></para>
    /// Default value is "api".
    /// </summary>
    public string DefaultApiPrefix { get; set; }

    /// <summary>
    /// Remove the dynamic API class(Controller) name postfix.
    /// <para></para>
    /// Default value is {"AppService", "ApplicationService"}.
    /// </summary>
    public List<string> RemoveControllerPostfixes { get; set; }

    /// <summary>
    /// Remove the dynamic API class's method(Action) postfix.
    /// <para></para>
    /// Default value is {"Async"}.
    /// </summary>
    public List<string> RemoveActionPostfixes { get; set; }

    /// <summary>
    /// Ignore MVC Form Binding types.
    /// </summary>
    public List<Type> FormBodyBindingIgnoredTypes { get; set; }

    /// <summary>
    /// The method that processing the name of the action.
    /// </summary>
    public Func<string, string> GetRestFulActionName { get; set; }

    /// <summary>
    /// Specifies the dynamic webapi options for the assembly.
    /// </summary>
    public Dictionary<Assembly, AssemblyDynamicWebApiOptions> AssemblyDynamicWebApiOptions { get; }

    public ISelectController SelectController { get; set; } = new DefaultSelectController();
    public IActionRouteFactory ActionRouteFactory { get; set; } = new DefaultActionRouteFactory();

    /// <summary>
    /// Verify that all configurations are valid
    /// </summary>
    public void Valid()
    {
        if (string.IsNullOrEmpty(DefaultHttpVerb))
        {
            throw new ArgumentException($"{nameof(DefaultHttpVerb)} can not be empty.");
        }

        if (string.IsNullOrEmpty(DefaultAreaName))
        {
            DefaultAreaName = string.Empty;
        }

        if (string.IsNullOrEmpty(DefaultApiPrefix))
        {
            DefaultApiPrefix = string.Empty;
        }

        if (FormBodyBindingIgnoredTypes == null)
        {
            throw new ArgumentException($"{nameof(FormBodyBindingIgnoredTypes)} can not be null.");
        }

        if (RemoveControllerPostfixes == null)
        {
            throw new ArgumentException($"{nameof(RemoveControllerPostfixes)} can not be null.");
        }
    }

    /// <summary>
    /// Add the dynamic webapi options for the assembly.
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="apiPreFix"></param>
    /// <param name="httpVerb"></param>
    public void AddAssemblyOptions(Assembly assembly, string apiPreFix = null, string httpVerb = null)
    {
        if (assembly == null)
        {
            throw new ArgumentException($"{nameof(assembly)} can not be null.");
        }

        AssemblyDynamicWebApiOptions[assembly] = new AssemblyDynamicWebApiOptions(apiPreFix, httpVerb);
    }

}

/// <summary>
/// Specifies the dynamic webapi options for the assembly.
/// </summary>
public class AssemblyDynamicWebApiOptions
{
    /// <summary>
    /// Routing prefix for all APIs
    /// <para></para>
    /// Default value is null.
    /// </summary>
    public string ApiPrefix { get; }

    /// <summary>
    /// API HTTP Verb.
    /// <para></para>
    /// Default value is null.
    /// </summary>
    public string HttpVerb { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="apiPrefix">Routing prefix for all APIs</param>
    /// <param name="httpVerb">API HTTP Verb.</param>
    public AssemblyDynamicWebApiOptions(string apiPrefix = null, string httpVerb = null)
    {
        ApiPrefix = apiPrefix;
        HttpVerb = httpVerb;
    }
}

public interface ISelectController
{
    bool IsController(Type type);
}

internal class DefaultSelectController : ISelectController
{
    public bool IsController(Type type)
    {
        var typeInfo = type.GetTypeInfo();

        if (!typeof(IDynamicWebApi).IsAssignableFrom(type) ||
            !typeInfo.IsPublic || typeInfo.IsAbstract || typeInfo.IsGenericType)
        {
            return false;
        }


        var attr = ReflectionHelper.GetSingleAttributeOrDefaultByFullSearch<DynamicWebApiAttribute>(typeInfo);

        if (attr == null)
        {
            return false;
        }

        if (ReflectionHelper.GetSingleAttributeOrDefaultByFullSearch<NonDynamicWebApiAttribute>(typeInfo) != null)
        {
            return false;
        }

        return true;
    }
}

public interface IActionRouteFactory
{
    string CreateActionRouteModel(string areaName, string controllerName, ActionModel action);
}

internal class DefaultActionRouteFactory : IActionRouteFactory
{
    private static string GetApiPreFix(ActionModel action)
    {
        var getValueSuccess = AppConsts.AssemblyDynamicWebApiOptions
            .TryGetValue(action.Controller.ControllerType.Assembly, out AssemblyDynamicWebApiOptions assemblyDynamicWebApiOptions);
        if (getValueSuccess && !string.IsNullOrWhiteSpace(assemblyDynamicWebApiOptions?.ApiPrefix))
        {
            return assemblyDynamicWebApiOptions.ApiPrefix;
        }

        return AppConsts.DefaultApiPreFix;
    }

    public string CreateActionRouteModel(string areaName, string controllerName, ActionModel action)
    {
        var apiPreFix = GetApiPreFix(action);
        var routeStr = $"{apiPreFix}/{areaName}/{controllerName}/{action.ActionName}".Replace("//", "/");
        return routeStr;
    }
}

public class DynamicWebApiConvention : IApplicationModelConvention
{
    private readonly ISelectController _selectController;
    private readonly IActionRouteFactory _actionRouteFactory;

    public DynamicWebApiConvention(ISelectController selectController, IActionRouteFactory actionRouteFactory)
    {
        _selectController = selectController;
        _actionRouteFactory = actionRouteFactory;
    }

    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            var type = controller.ControllerType.AsType();
            var dynamicWebApiAttr = ReflectionHelper.GetSingleAttributeOrDefaultByFullSearch<DynamicWebApiAttribute>(type.GetTypeInfo());

            if (!(_selectController is DefaultSelectController) && _selectController.IsController(type))
            {
                controller.ControllerName = controller.ControllerName.RemovePostFix(AppConsts.ControllerPostfixes.ToArray());
                ConfigureDynamicWebApi(controller, dynamicWebApiAttr);
            }
            else
            {
                if (typeof(IDynamicWebApi).GetTypeInfo().IsAssignableFrom(type))
                {
                    controller.ControllerName = controller.ControllerName.RemovePostFix(AppConsts.ControllerPostfixes.ToArray());
                    ConfigureArea(controller, dynamicWebApiAttr);
                    ConfigureDynamicWebApi(controller, dynamicWebApiAttr);
                }
                else
                {
                    if (dynamicWebApiAttr != null)
                    {
                        ConfigureArea(controller, dynamicWebApiAttr);
                        ConfigureDynamicWebApi(controller, dynamicWebApiAttr);
                    }
                }
            }
        }
    }

    private void ConfigureArea(ControllerModel controller, DynamicWebApiAttribute attr)
    {
        if (!controller.RouteValues.ContainsKey("area"))
        {
            if (attr == null)
            {
                throw new ArgumentException(nameof(attr));
            }

            if (!string.IsNullOrEmpty(attr.Module))
            {
                controller.RouteValues["area"] = attr.Module;
            }
            else if (!string.IsNullOrEmpty(AppConsts.DefaultAreaName))
            {
                controller.RouteValues["area"] = AppConsts.DefaultAreaName;
            }
        }

    }

    private void ConfigureDynamicWebApi(ControllerModel controller, DynamicWebApiAttribute controllerAttr)
    {
        ConfigureApiExplorer(controller);
        ConfigureSelector(controller, controllerAttr);
        ConfigureParameters(controller);
    }


    private void ConfigureParameters(ControllerModel controller)
    {
        foreach (var action in controller.Actions)
        {
            if (!CheckNoMapMethod(action))
                foreach (var para in action.Parameters)
                {
                    if (para.BindingInfo != null)
                    {
                        continue;
                    }

                    if (!TypeHelper.IsPrimitiveExtendedIncludingNullable(para.ParameterInfo.ParameterType))
                    {
                        if (CanUseFormBodyBinding(action, para))
                        {
                            para.BindingInfo = BindingInfo.GetBindingInfo(new[] { new FromBodyAttribute() });
                        }
                    }
                }
        }
    }


    private bool CanUseFormBodyBinding(ActionModel action, ParameterModel parameter)
    {
        if (AppConsts.FormBodyBindingIgnoredTypes.Any(t => t.IsAssignableFrom(parameter.ParameterInfo.ParameterType)))
        {
            return false;
        }

        foreach (var selector in action.Selectors)
        {
            if (selector.ActionConstraints == null)
            {
                continue;
            }

            foreach (var actionConstraint in selector.ActionConstraints)
            {

                var httpMethodActionConstraint = actionConstraint as HttpMethodActionConstraint;
                if (httpMethodActionConstraint == null)
                {
                    continue;
                }

                if (httpMethodActionConstraint.HttpMethods.All(hm => hm.IsIn("GET", "DELETE", "TRACE", "HEAD")))
                {
                    return false;
                }
            }
        }

        return true;
    }


    #region ConfigureApiExplorer

    private void ConfigureApiExplorer(ControllerModel controller)
    {
        if (controller.ApiExplorer.GroupName.IsNullOrEmpty())
        {
            controller.ApiExplorer.GroupName = controller.ControllerName;
        }

        if (controller.ApiExplorer.IsVisible == null)
        {
            controller.ApiExplorer.IsVisible = true;
        }

        foreach (var action in controller.Actions)
        {
            if (!CheckNoMapMethod(action))
                ConfigureApiExplorer(action);
        }
    }

    private void ConfigureApiExplorer(ActionModel action)
    {
        if (action.ApiExplorer.IsVisible == null)
        {
            action.ApiExplorer.IsVisible = true;
        }
    }

    #endregion
    /// <summary>
    /// //不映射指定的方法
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    private bool CheckNoMapMethod(ActionModel action)
    {
        bool isExist = false;
        var noMapMethod = ReflectionHelper.GetSingleAttributeOrDefault<NonDynamicMethodAttribute>(action.ActionMethod);

        if (noMapMethod != null)
        {
            action.ApiExplorer.IsVisible = false;//对应的Api不映射
            isExist = true;
        }

        return isExist;
    }
    private void ConfigureSelector(ControllerModel controller, DynamicWebApiAttribute controllerAttr)
    {

        if (controller.Selectors.Any(selector => selector.AttributeRouteModel != null))
        {
            return;
        }

        var areaName = string.Empty;

        if (controllerAttr != null)
        {
            areaName = controllerAttr.Module;
        }

        foreach (var action in controller.Actions)
        {
            if (!CheckNoMapMethod(action))
                ConfigureSelector(areaName, controller.ControllerName, action);
        }
    }

    private void ConfigureSelector(string areaName, string controllerName, ActionModel action)
    {

        var nonAttr = ReflectionHelper.GetSingleAttributeOrDefault<NonDynamicWebApiAttribute>(action.ActionMethod);

        if (nonAttr != null)
        {
            return;
        }

        if (action.Selectors.IsNullOrEmpty() || action.Selectors.Any(a => a.ActionConstraints.IsNullOrEmpty()))
        {
            if (!CheckNoMapMethod(action))
                AddAppServiceSelector(areaName, controllerName, action);
        }
        else
        {
            NormalizeSelectorRoutes(areaName, controllerName, action);
        }
    }

    private void AddAppServiceSelector(string areaName, string controllerName, ActionModel action)
    {

        var verb = GetHttpVerb(action);

        action.ActionName = GetRestFulActionName(action.ActionName);

        var appServiceSelectorModel = action.Selectors[0];

        if (appServiceSelectorModel.AttributeRouteModel == null)
        {
            appServiceSelectorModel.AttributeRouteModel = CreateActionRouteModel(areaName, controllerName, action);
        }

        if (!appServiceSelectorModel.ActionConstraints.Any())
        {
            appServiceSelectorModel.ActionConstraints.Add(new HttpMethodActionConstraint(new[] { verb }));
            switch (verb)
            {
                case "GET":
                    appServiceSelectorModel.EndpointMetadata.Add(new HttpGetAttribute());
                    break;
                case "POST":
                    appServiceSelectorModel.EndpointMetadata.Add(new HttpPostAttribute());
                    break;
                case "PUT":
                    appServiceSelectorModel.EndpointMetadata.Add(new HttpPutAttribute());
                    break;
                case "DELETE":
                    appServiceSelectorModel.EndpointMetadata.Add(new HttpDeleteAttribute());
                    break;
                default:
                    throw new Exception($"Unsupported http verb: {verb}.");
            }
        }


    }



    /// <summary>
    /// Processing action name
    /// </summary>
    /// <param name="actionName"></param>
    /// <returns></returns>
    private static string GetRestFulActionName(string actionName)
    {
        // custom process action name
        var appConstsActionName = AppConsts.GetRestFulActionName?.Invoke(actionName);
        if (appConstsActionName != null)
        {
            return appConstsActionName;
        }

        // default process action name.

        // Remove Postfix
        //actionName = actionName.RemovePostFix(AppConsts.ActionPostfixes.ToArray());

        // Remove Prefix
        //var verbKey = actionName.GetPascalOrCamelCaseFirstWord().ToLower();
        //if (AppConsts.HttpVerbs.ContainsKey(verbKey))
        //{
        //    if (actionName.Length == verbKey.Length)
        //    {
        //        return "";
        //    }
        //    else
        //    {
        //        return actionName.Substring(verbKey.Length);
        //    }
        //}
        //else
        //{
        //    return actionName;
        //}
        return actionName;
    }

    private void NormalizeSelectorRoutes(string areaName, string controllerName, ActionModel action)
    {
        action.ActionName = GetRestFulActionName(action.ActionName);
        foreach (var selector in action.Selectors)
        {
            //var aa = selector.EndpointMetadata.OfType<HttpMethodAttribute>().FirstOrDefault();
            //if (!string.IsNullOrWhiteSpace(aa.Template))
            //{
            //    var res = aa.Template.ToLower().Replace("[controller]", controllerName.ToLower().Replace("service", ""));
            //    res = res.Replace("[action]", action.DisplayName);
            //    selector.EndpointMetadata.Remove(aa);
            //    //selector.EndpointMetadata.Add(new HttpMethodAttribute(new[ "Get" ]));
            //}

            var bbbb = AttributeRouteModel.CombineAttributeRouteModel(CreateActionRouteModel(areaName, controllerName, action), selector.AttributeRouteModel);
            selector.AttributeRouteModel = selector.AttributeRouteModel is null ? CreateActionRouteModel(areaName, controllerName, action) : CreateActionRouteModel(areaName, controllerName, action, selector.AttributeRouteModel);
        }
    }

    private static string GetHttpVerb(ActionModel action)
    {
        var getValueSuccess = AppConsts.AssemblyDynamicWebApiOptions
            .TryGetValue(action.Controller.ControllerType.Assembly, out AssemblyDynamicWebApiOptions assemblyDynamicWebApiOptions);
        if (getValueSuccess && !string.IsNullOrWhiteSpace(assemblyDynamicWebApiOptions?.HttpVerb))
        {
            return assemblyDynamicWebApiOptions.HttpVerb;
        }


        var verbKey = action.ActionName.GetPascalOrCamelCaseFirstWord().ToLower();

        var verb = AppConsts.HttpVerbs.ContainsKey(verbKey) ? AppConsts.HttpVerbs[verbKey] : AppConsts.DefaultHttpVerb;
        return verb;
    }

    private AttributeRouteModel CreateActionRouteModel(string areaName, string controllerName, ActionModel action, AttributeRouteModel orgin = null)
    {
        if (orgin is null)
        {
            var route = _actionRouteFactory.CreateActionRouteModel(areaName, controllerName, action);
            return new AttributeRouteModel(new RouteAttribute(route));
        }
        else
        {
            controllerName = controllerName.Replace("Service", "");
            var route = orgin.Template.Replace("[Controller]", controllerName);
            route = route.Replace("[controller]", controllerName);
            route = route.Replace("[Action]", action.ActionName);
            route = route.Replace("[action]", action.ActionName);
            return new AttributeRouteModel(new RouteAttribute(route));
        }
    }
}

public static class AppConsts
{
    public static string DefaultHttpVerb { get; set; }

    public static string DefaultAreaName { get; set; }

    public static string DefaultApiPreFix { get; set; }

    public static List<string> ControllerPostfixes { get; set; }
    public static List<string> ActionPostfixes { get; set; }

    public static List<Type> FormBodyBindingIgnoredTypes { get; set; }

    public static Dictionary<string, string> HttpVerbs { get; set; }

    public static Func<string, string> GetRestFulActionName { get; set; }

    public static Dictionary<Assembly, AssemblyDynamicWebApiOptions> AssemblyDynamicWebApiOptions { get; set; }

    static AppConsts()
    {
        HttpVerbs = new Dictionary<string, string>()
        {
            ["add"] = "POST",
            ["create"] = "POST",
            ["post"] = "POST",

            ["get"] = "GET",
            ["find"] = "GET",
            ["fetch"] = "GET",
            ["query"] = "GET",

            ["update"] = "PUT",
            ["put"] = "PUT",

            ["delete"] = "DELETE",
            ["remove"] = "DELETE",
        };
    }
}

public class DynamicWebApiControllerFeatureProvider : ControllerFeatureProvider
{
    private ISelectController _selectController;

    public DynamicWebApiControllerFeatureProvider(ISelectController selectController)
    {
        _selectController = selectController;
    }

    protected override bool IsController(TypeInfo typeInfo)
    {
        return _selectController.IsController(typeInfo);
    }
}

internal static class ServiceCollectionExtensions
{
    public static bool IsAdded<T>(this IServiceCollection services)
    {
        return services.IsAdded(typeof(T));
    }

    public static bool IsAdded(this IServiceCollection services, Type type)
    {
        return services.Any(d => d.ServiceType == type);
    }

    public static T GetSingletonInstanceOrNull<T>(this IServiceCollection services)
    {
        return (T)services
            .FirstOrDefault(d => d.ServiceType == typeof(T))
            ?.ImplementationInstance;
    }

    public static T GetSingletonInstance<T>(this IServiceCollection services)
    {
        var service = services.GetSingletonInstanceOrNull<T>();
        if (service == null)
        {
            throw new InvalidOperationException("Could not find singleton service: " + typeof(T).AssemblyQualifiedName);
        }

        return service;
    }

    public static IServiceProvider BuildServiceProviderFromFactory(this IServiceCollection services)
    {
        foreach (var service in services)
        {
            var factoryInterface = service.ImplementationInstance?.GetType()
                .GetTypeInfo()
                .GetInterfaces()
                .FirstOrDefault(i => i.GetTypeInfo().IsGenericType &&
                                     i.GetGenericTypeDefinition() == typeof(IServiceProviderFactory<>));

            if (factoryInterface == null)
            {
                continue;
            }

            var containerBuilderType = factoryInterface.GenericTypeArguments[0];
            return (IServiceProvider)typeof(ServiceCollectionExtensions)
                .GetTypeInfo()
                .GetMethods()
                .Single(m => m.Name == nameof(BuildServiceProviderFromFactory) && m.IsGenericMethod)
                .MakeGenericMethod(containerBuilderType)
                .Invoke(null, new object[] { services, null });
        }

        return services.BuildServiceProvider();
    }

    public static IServiceProvider BuildServiceProviderFromFactory<TContainerBuilder>(this IServiceCollection services, Action<TContainerBuilder> builderAction = null)
    {

        var serviceProviderFactory = services.GetSingletonInstanceOrNull<IServiceProviderFactory<TContainerBuilder>>();
        if (serviceProviderFactory == null)
        {
            throw new Exception($"Could not find {typeof(IServiceProviderFactory<TContainerBuilder>).FullName} in {services}.");
        }

        var builder = serviceProviderFactory.CreateBuilder(services);
        builderAction?.Invoke(builder);
        return serviceProviderFactory.CreateServiceProvider(builder);
    }
}

internal static class ReflectionHelper
{

    public static TAttribute GetSingleAttributeOrDefaultByFullSearch<TAttribute>(TypeInfo info)
        where TAttribute : Attribute
    {
        var attributeType = typeof(TAttribute);
        if (info.IsDefined(attributeType, true))
        {
            return info.GetCustomAttributes(attributeType, true).Cast<TAttribute>().First();
        }
        else
        {
            foreach (var implInter in info.ImplementedInterfaces)
            {
                var res = GetSingleAttributeOrDefaultByFullSearch<TAttribute>(implInter.GetTypeInfo());

                if (res != null)
                {
                    return res;
                }
            }
        }

        return null;
    }

    public static TAttribute GetSingleAttributeOrDefault<TAttribute>(MemberInfo memberInfo, TAttribute defaultValue = default, bool inherit = true)
   where TAttribute : Attribute
    {
        var attributeType = typeof(TAttribute);
        if (memberInfo.IsDefined(typeof(TAttribute), inherit))
        {
            return memberInfo.GetCustomAttributes(attributeType, inherit).Cast<TAttribute>().First();
        }

        return defaultValue;
    }


    /// <summary>
    /// Gets a single attribute for a member.
    /// </summary>
    /// <typeparam name="TAttribute">Type of the attribute</typeparam>
    /// <param name="memberInfo">The member that will be checked for the attribute</param>
    /// <param name="inherit">Include inherited attributes</param>
    /// <returns>Returns the attribute object if found. Returns null if not found.</returns>
    public static TAttribute GetSingleAttributeOrNull<TAttribute>(this MemberInfo memberInfo, bool inherit = true)
        where TAttribute : Attribute
    {
        if (memberInfo == null)
        {
            throw new ArgumentNullException(nameof(memberInfo));
        }

        var attrs = memberInfo.GetCustomAttributes(typeof(TAttribute), inherit).ToArray();
        if (attrs.Length > 0)
        {
            return (TAttribute)attrs[0];
        }

        return default;
    }


    public static TAttribute GetSingleAttributeOfTypeOrBaseTypesOrNull<TAttribute>(this Type type, bool inherit = true)
        where TAttribute : Attribute
    {
        var attr = type.GetTypeInfo().GetSingleAttributeOrNull<TAttribute>();
        if (attr != null)
        {
            return attr;
        }

        if (type.GetTypeInfo().BaseType == null)
        {
            return null;
        }

        return type.GetTypeInfo().BaseType.GetSingleAttributeOfTypeOrBaseTypesOrNull<TAttribute>(inherit);
    }

}

internal static class ExtensionMethods
{
    public static bool IsNullOrEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }

    public static bool IsNullOrEmpty<T>(this ICollection<T> source)
    {
        return source == null || source.Count <= 0;
    }

    public static bool IsIn(this string str, params string[] data)
    {
        foreach (var item in data)
        {
            if (str == item)
            {
                return true;
            }
        }
        return false;
    }

    public static string RemovePostFix(this string str, params string[] postFixes)
    {
        if (str == null)
        {
            return null;
        }

        if (str == string.Empty)
        {
            return string.Empty;
        }

        if (postFixes.IsNullOrEmpty())
        {
            return str;
        }

        foreach (var postFix in postFixes)
        {
            if (str.EndsWith(postFix))
            {
                return str.Left(str.Length - postFix.Length);
            }
        }

        return str;
    }

    public static string RemovePreFix(this string str, params string[] preFixes)
    {
        if (str == null)
        {
            return null;
        }

        if (str == string.Empty)
        {
            return string.Empty;
        }

        if (preFixes.IsNullOrEmpty())
        {
            return str;
        }

        foreach (var preFix in preFixes)
        {
            if (str.StartsWith(preFix))
            {
                return str.Right(str.Length - preFix.Length);
            }
        }

        return str;
    }


    public static string Left(this string str, int len)
    {
        if (str == null)
        {
            throw new ArgumentNullException("str");
        }

        if (str.Length < len)
        {
            throw new ArgumentException("len argument can not be bigger than given string's length!");
        }

        return str.Substring(0, len);
    }


    public static string Right(this string str, int len)
    {
        if (str == null)
        {
            throw new ArgumentNullException("str");
        }

        if (str.Length < len)
        {
            throw new ArgumentException("len argument can not be bigger than given string's length!");
        }

        return str.Substring(str.Length - len, len);
    }

    public static string GetCamelCaseFirstWord(this string str)
    {
        if (str == null)
        {
            throw new ArgumentNullException(nameof(str));
        }

        if (str.Length == 1)
        {
            return str;
        }

        var res = Regex.Split(str, @"(?=\p{Lu}\p{Ll})|(?<=\p{Ll})(?=\p{Lu})");

        if (res.Length < 1)
        {
            return str;
        }
        else
        {
            return res[0];
        }
    }

    public static string GetPascalCaseFirstWord(this string str)
    {
        if (str == null)
        {
            throw new ArgumentNullException(nameof(str));
        }

        if (str.Length == 1)
        {
            return str;
        }

        var res = Regex.Split(str, @"(?=\p{Lu}\p{Ll})|(?<=\p{Ll})(?=\p{Lu})");

        if (res.Length < 2)
        {
            return str;
        }
        else
        {
            return res[1];
        }
    }

    public static string GetPascalOrCamelCaseFirstWord(this string str)
    {
        if (str == null)
        {
            throw new ArgumentNullException(nameof(str));
        }

        if (str.Length <= 1)
        {
            return str;
        }

        if (str[0] >= 65 && str[0] <= 90)
        {
            return str.GetPascalCaseFirstWord();
        }
        else
        {
            return str.GetCamelCaseFirstWord();
        }
    }


}

[Serializable]
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
public class DynamicWebApiAttribute : Attribute
{
    /// <summary>
    /// Equivalent to AreaName
    /// </summary>
    public string Module { get; set; }

    internal static bool IsExplicitlyEnabledFor(Type type)
    {
        var remoteServiceAttr = type.GetTypeInfo().GetSingleAttributeOrNull<DynamicWebApiAttribute>();
        return remoteServiceAttr != null;
    }

    internal static bool IsExplicitlyDisabledFor(Type type)
    {
        var remoteServiceAttr = type.GetTypeInfo().GetSingleAttributeOrNull<DynamicWebApiAttribute>();
        return remoteServiceAttr != null;
    }

    internal static bool IsMetadataExplicitlyEnabledFor(Type type)
    {
        var remoteServiceAttr = type.GetTypeInfo().GetSingleAttributeOrNull<DynamicWebApiAttribute>();
        return remoteServiceAttr != null;
    }

    internal static bool IsMetadataExplicitlyDisabledFor(Type type)
    {
        var remoteServiceAttr = type.GetTypeInfo().GetSingleAttributeOrNull<DynamicWebApiAttribute>();
        return remoteServiceAttr != null;
    }

    internal static bool IsMetadataExplicitlyDisabledFor(MethodInfo method)
    {
        var remoteServiceAttr = method.GetSingleAttributeOrNull<DynamicWebApiAttribute>();
        return remoteServiceAttr != null;
    }

    internal static bool IsMetadataExplicitlyEnabledFor(MethodInfo method)
    {
        var remoteServiceAttr = method.GetSingleAttributeOrNull<DynamicWebApiAttribute>();
        return remoteServiceAttr != null;
    }
}

[Serializable]
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Method)]
public class NonDynamicMethodAttribute : Attribute
{

}

[Serializable]
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Method)]
public class NonDynamicWebApiAttribute : Attribute
{

}

public interface IDynamicWebApi
{

}

internal static class TypeHelper
{
    public static bool IsFunc(object obj)
    {
        if (obj == null)
        {
            return false;
        }

        var type = obj.GetType();
        if (!type.GetTypeInfo().IsGenericType)
        {
            return false;
        }

        return type.GetGenericTypeDefinition() == typeof(Func<>);
    }

    public static bool IsFunc<TReturn>(object obj)
    {
        return obj != null && obj.GetType() == typeof(Func<TReturn>);
    }

    public static bool IsPrimitiveExtendedIncludingNullable(Type type, bool includeEnums = false)
    {
        if (IsPrimitiveExtended(type, includeEnums))
        {
            return true;
        }

        if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return IsPrimitiveExtended(type.GenericTypeArguments[0], includeEnums);
        }

        return false;
    }

    private static bool IsPrimitiveExtended(Type type, bool includeEnums)
    {
        if (type.GetTypeInfo().IsPrimitive)
        {
            return true;
        }

        if (includeEnums && type.GetTypeInfo().IsEnum)
        {
            return true;
        }

        return type == typeof(string) ||
               type == typeof(decimal) ||
               type == typeof(DateTime) ||
               type == typeof(DateTimeOffset) ||
               type == typeof(TimeSpan) ||
               type == typeof(Guid);
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class DynamicControllerAttribute : Attribute
{
    public DynamicControllerAttribute()
    {
        ServiceName = string.Empty;
    }

    public DynamicControllerAttribute(string serviceName)
    {
        ServiceName = serviceName;
    }

    public string ServiceName { get; }
}