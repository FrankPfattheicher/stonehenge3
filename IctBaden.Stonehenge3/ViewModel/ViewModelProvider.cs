using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.Hosting;
using IctBaden.Stonehenge3.Resources;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IctBaden.Stonehenge3.ViewModel
{
    public class ViewModelProvider : IStonehengeResourceProvider
    {
        private readonly ILogger _logger;

        public ViewModelProvider(ILogger logger)
        {
            _logger = logger;
        }
        
        public void InitProvider(StonehengeResourceLoader loader, StonehengeHostOptions options)
        {
        }

        public void Dispose()
        {
        }

        public Resource Post(AppSession session, string resourceName, 
            Dictionary<string, string> parameters, Dictionary<string, string> formData)
        {
            if (resourceName.StartsWith("Command/"))
            {
                var commandName = resourceName.Substring(8);
                var appCommandsType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(type => type.GetInterfaces().Contains(typeof(IStonehengeAppCommands)));
                if (appCommandsType != null)
                {
                    var appCommands = Activator.CreateInstance(appCommandsType);
                    var commandHandler = appCommands?.GetType().GetMethod(commandName);
                    if (commandHandler != null)
                    {
                        commandHandler.Invoke(appCommands, new object[] {session});
                        return new Resource(commandName, "Command", ResourceType.Json, "{ 'executed': true }",
                            Resource.Cache.None);
                    }
                    else
                    {
                        return new Resource(commandName, "Command", ResourceType.Json, "{ 'executed': false }",
                            Resource.Cache.None);
                    }
                }
                else
                {
                    return new Resource(commandName, "Command", ResourceType.Json, "{ 'executed': false }",
                        Resource.Cache.None);
                }
            }
            else if (resourceName.StartsWith("Data/"))
            {
                return PostDataResource(session, resourceName.Substring(5), parameters, formData);
            }

            if (!resourceName.StartsWith("ViewModel/")) return null;

            var parts = resourceName.Split('/');
            if (parts.Length != 3) return null;

            var vmTypeName = parts[1];
            var methodName = parts[2];

            if (session.ViewModel == null)
            {
                _logger.LogWarning($"ViewModelProvider: Set VM={vmTypeName}, no current VM");
                session.SetViewModelType(vmTypeName);
            }

            foreach (var data in formData)
            {
                _logger.LogDebug($"ViewModelProvider: Set {data.Key}={data.Value}");
                SetPropertyValue(_logger, session.ViewModel, data.Key, data.Value);
            }

            var vmType = session.ViewModel.GetType();
            if (vmType.Name != vmTypeName)
            {
                _logger.LogWarning($"ViewModelProvider: Request for VM={vmTypeName}, current VM={vmType.Name}");
                return new Resource(resourceName, "ViewModelProvider", ResourceType.Json, "{ \"StonehengeContinuePolling\":false }", Resource.Cache.None);
            }

            var method = vmType.GetMethod(methodName);
            if (method == null)
            {
                _logger.LogWarning($"ViewModelProvider: ActionMethod {methodName} not found.");
                return null;
            }

            try
            {
                var attribute = method
                    .GetCustomAttributes(typeof(ActionMethodAttribute), true)
                    .FirstOrDefault() as ActionMethodAttribute;
                var executeAsync = attribute?.ExecuteAsync ?? false;
                var methodParams = method.GetParameters()
                        .Zip(
                            parameters.Values,
                            (parameterInfo, postParam) =>
                                new KeyValuePair<Type, object>(parameterInfo.ParameterType, postParam))
                        .Select(parameterPair => Convert.ChangeType(parameterPair.Value, parameterPair.Key))
                        .ToArray();
                if (executeAsync)
                {
                    Task.Run(() => method.Invoke(session.ViewModel, methodParams));
                    return GetEvents(session, resourceName);
                }
                else
                {
                    method.Invoke(session.ViewModel, methodParams);
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) ex = ex.InnerException;
                
                _logger.LogError($"ViewModelProvider: ActionMethod {methodName} has {method.GetParameters().Length} params.");
                _logger.LogError($"ViewModelProvider: Called with {parameters.Count} params.");
                _logger.LogError("ViewModelProvider: " + ex.Message);
                _logger.LogError("ViewModelProvider: " + ex.StackTrace);

                var exResource = new JObject
                {
                    ["Message"] = ex.Message,
                    ["StackTrace"] = ex.StackTrace
                };
                return new Resource(resourceName, "ViewModelProvider", ResourceType.Json, GetViewModelJson(exResource),
                    Resource.Cache.None);
            }

            return new Resource(resourceName, "ViewModelProvider", ResourceType.Json,
                GetViewModelJson(session.ViewModel), Resource.Cache.None);
        }

        public Resource Get(AppSession session, string resourceName, Dictionary<string, string> parameters)
        {
            if (resourceName.StartsWith("ViewModel/"))
            {
                if (SetViewModel(session, resourceName))
                {
                    if (session.ViewModel is ActiveViewModel avm)
                    {
                        avm.OnLoad();
                    }

                    return GetViewModel(session, resourceName);
                }
            }
            else if (resourceName.StartsWith("Events/"))
            {
                return GetEvents(session, resourceName);
            }
            else if (resourceName.StartsWith("Data/"))
            {
                return GetDataResource(session, resourceName.Substring(5), parameters);
            }
            return null;
        }

        private bool SetViewModel(AppSession session, string resourceName)
        {
            var vmTypeName = Path.GetFileNameWithoutExtension(resourceName);
            if ((session.ViewModel != null) && (session.ViewModel.GetType().Name == vmTypeName)) return true;
            if (session.SetViewModelType(vmTypeName) != null)
            {
                return true;
            }

            _logger.LogError("Could not set ViewModel type to " + vmTypeName);
            return false;
        }

        private Resource GetViewModel(AppSession session, string resourceName)
        {
            session.EventsClear(true);

            return new Resource(resourceName, "ViewModelProvider", ResourceType.Json,
                GetViewModelJson(session.ViewModel),
                Resource.Cache.None);
        }

        private static Resource GetEvents(AppSession session, string resourceName)
        {
            var parts = resourceName.Split('/');
            if (parts.Length < 2) return null;

            var vmTypeName = parts[1];
            var vmType = session.ViewModel?.GetType();

            var json = "{ \"StonehengeContinuePolling\":false }";
            if (vmTypeName != vmType?.Name)
            {
                // view model changed !
                return new Resource(resourceName, "ViewModelProvider", ResourceType.Json, json, Resource.Cache.None);
            }

            var data = new List<string> {"\"StonehengeContinuePolling\":true"};
            var events = session.CollectEvents();
            
            if (vmTypeName != vmType?.Name)
            {
                // view model changed !
                return new Resource(resourceName, "ViewModelProvider", ResourceType.Json, json, Resource.Cache.None);
            }

            if (session.ViewModel is ActiveViewModel activeVm)
            {
                try
                {
                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (var property in events)
                    {
                        var value = activeVm.TryGetMember(property);
                        data.Add($"\"{property}\":{JsonSerializer.SerializeObjectString(null, value)}");
                    }

                    AddStonehengeInternalProperties(data, activeVm);
                }
                catch
                {
                    // ignore for events
                }
            }

            json = "{" + string.Join(",", data) + "}";
            return new Resource(resourceName, "ViewModelProvider", ResourceType.Json, json, Resource.Cache.None);
        }

        private static void AddStonehengeInternalProperties(ICollection<string> data, ActiveViewModel activeVm)
        {
            if (!string.IsNullOrEmpty(activeVm.MessageBoxTitle) || !string.IsNullOrEmpty(activeVm.MessageBoxText))
            {
                var title = activeVm.MessageBoxTitle ?? "";
                var text = activeVm.MessageBoxText ?? "";
                var script = $"alert('{HttpUtility.JavaScriptStringEncode(title)}\\r\\n{HttpUtility.JavaScriptStringEncode(text)}');";
                data.Add($"\"StonehengeEval\":{JsonSerializer.SerializeObjectString(null, script)}");
                activeVm.MessageBoxTitle = null;
                activeVm.MessageBoxText = null;
            }

            if (!string.IsNullOrEmpty(activeVm.NavigateToRoute))
            {
                var route = activeVm.NavigateToRoute;
                data.Add($"\"StonehengeNavigate\":{JsonSerializer.SerializeObjectString(null, route)}");
                activeVm.NavigateToRoute = null;
            }
            else if (!string.IsNullOrEmpty(activeVm.ClientScript))
            {
                var script = activeVm.ClientScript;
                data.Add($"\"StonehengeEval\":{JsonSerializer.SerializeObjectString(null, script)}");
                activeVm.ClientScript = null;
            }
        }

        private static Resource GetDataResource(AppSession session, string resourceName,
            Dictionary<string, string> parameters)
        {
            var vm = session.ViewModel as ActiveViewModel;
            var method = vm?.GetType()
                .GetMethods()
                .FirstOrDefault(m =>
                    string.Compare(m.Name, "GetDataResource", StringComparison.InvariantCultureIgnoreCase) == 0);
            if (method == null || method.ReturnType != typeof(Resource)) return null;

            Resource data;
            if (method.GetParameters().Length == 2)
            {
                data = (Resource) method.Invoke(vm, new object[] {resourceName, parameters});
            }
            else
            {
                data = (Resource) method.Invoke(vm, new object[] {resourceName});
            }

            return data;
        }

        private static Resource PostDataResource(AppSession session, string resourceName,
            Dictionary<string, string> parameters, Dictionary<string, string> formData)
        {
            var vm = session.ViewModel as ActiveViewModel;
            var method = vm?.GetType()
                .GetMethods()
                .FirstOrDefault(m =>
                    string.Compare(m.Name, "PostDataResource", StringComparison.InvariantCultureIgnoreCase) == 0);
            if (method == null || method.ReturnType != typeof(Resource)) return null;

            Resource data;
            if (method.GetParameters().Length == 3)
            {
                data = (Resource) method.Invoke(vm, new object[] {resourceName, parameters, formData});
            }
            else if (method.GetParameters().Length == 2)
            {
                data = (Resource) method.Invoke(vm, new object[] {resourceName, parameters});
            }
            else
            {
                data = (Resource) method.Invoke(vm, new object[] {resourceName});
            }

            return data;
        }


        private static object DeserializePropertyValue(ILogger logger, string propValue, Type propType)
        {
            try
            {
                if (propType == typeof(string))
                    return propValue;
                if (propType == typeof(bool))
                    return bool.Parse(propValue);
                if (propType == typeof(DateTime))
                {
                    if (DateTime.TryParse(propValue, out var dt))
                        return dt;
                    if (DateTime.TryParse(propValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                        return dt;
                }

                return JsonConvert.DeserializeObject(propValue, propType);
            }
            catch (Exception ex)
            {
                logger.LogError("DeserializePropertyValue: " + ex.Message);
            }

            return null;
        }

        private static void SetPropertyValue(ILogger logger, object vm, string propName, string newValue)
        {
            try
            {
                if (vm is ActiveViewModel activeVm)
                {
                    var pi = activeVm.GetPropertyInfo(propName);
                    if ((pi == null) || !pi.CanWrite)
                        return;

                    if (pi.PropertyType.IsValueType && !pi.PropertyType.IsPrimitive &&
                        (pi.PropertyType.Namespace != "System")) // struct
                    {
                        object structObj = activeVm.TryGetMember(propName);
                        if (structObj != null)
                        {
                            if (JsonConvert.DeserializeObject(newValue, typeof(Dictionary<string, string>)) is
                                Dictionary<string, string> members)
                            {
                                foreach (var member in members)
                                {
                                    var mProp = pi.PropertyType.GetProperty(member.Key);
                                    if (mProp != null)
                                    {
                                        var val = DeserializePropertyValue(logger, member.Value, mProp.PropertyType);
                                        mProp.SetValue(structObj, val, null);
                                    }
                                }
                            }

                            activeVm.TrySetMember(propName, structObj);
                        }
                    }
                    else if (pi.PropertyType.IsGenericType && pi.PropertyType.Name.StartsWith("Notify`"))
                    {
                        var val = DeserializePropertyValue(logger, newValue, pi.PropertyType.GenericTypeArguments[0]);
                        var type = typeof(Notify<>).MakeGenericType(pi.PropertyType.GenericTypeArguments[0]);
                        var notify = Activator.CreateInstance(type, new[] { activeVm, pi.Name, val });
                        var valueField = type.GetField("_value", BindingFlags.Instance | BindingFlags.NonPublic);
                        valueField?.SetValue(notify, val);
                        activeVm.TrySetMember(propName, notify);
                    }
                    else
                    {
                        var val = DeserializePropertyValue(logger, newValue, pi.PropertyType);
                        activeVm.TrySetMember(propName, val);
                    }
                }
                else
                {
                    var pi = vm.GetType().GetProperty(propName);
                    if ((pi == null) || !pi.CanWrite)
                        return;

                    var val = DeserializePropertyValue(logger, newValue, pi.PropertyType);
                    pi.SetValue(vm, val, null);
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"SetPropertyValue({propName}): " + ex.Message);
            }
        }

        private string GetViewModelJson(object viewModel)
        {
            var watch = new Stopwatch();
            watch.Start();
            
            var ty = viewModel.GetType();
            _logger.LogDebug("ViewModelProvider: ViewModel=" + ty.Name);

            var data = new List<string>();
            var context = "";
            try
            {
                if (viewModel is ActiveViewModel activeVm)
                {
                    foreach (var model in activeVm.ActiveModels)
                    {
                        context = model.TypeName;
                        data.AddRange(JsonSerializer.SerializeObject(model.Prefix, model.Model));
                    }

                    context = "internal properties";
                    AddStonehengeInternalProperties(data, activeVm);

                    context = "dictionary names";
                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (var name in activeVm.GetDictionaryNames())
                    {
                        // ReSharper disable once UseStringInterpolation
                        data.Add(string.Format("\"{0}\":{1}", name,
                            JsonConvert.SerializeObject(activeVm.TryGetMember(name))));
                    }
                }

                context = "view model";
                data.AddRange(JsonSerializer.SerializeObject(null, viewModel));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception serializing ViewModel({ty.Name}) : {context}");
                _logger.LogError(ex.Message);
                _logger.LogError(ex.StackTrace);

                var exResource = new JObject
                {
                    ["Message"] = ex.Message,
                    ["StackTrace"] = ex.StackTrace
                };
                return JsonConvert.SerializeObject(exResource);
            }

            var json = "{" + string.Join(",", data) + "}";
            
            watch.Stop();
            _logger.LogTrace($"GetViewModelJson: {watch.ElapsedMilliseconds}ms");
            return json;
        }
    }
}