using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.Hosting;
using IctBaden.Stonehenge3.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IctBaden.Stonehenge3.ViewModel
{
    public class ViewModelProvider : IStonehengeResourceProvider
    {
        public void InitProvider(StonehengeResourceLoader loader, StonehengeHostOptions options)
        {
        }

        public void Dispose()
        {
        }

        public Resource Post(AppSession session, string resourceName, Dictionary<string, string> parameters,
            Dictionary<string, string> formData)
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
                    var commandHandler = appCommands.GetType().GetMethod(commandName);
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

            if (!resourceName.StartsWith("ViewModel/")) return null;

            var parts = resourceName.Split('/');
            if (parts.Length != 3) return null;

            var vmTypeName = parts[1];
            var methodName = parts[2];

            if (session.ViewModel == null)
            {
                session.SetViewModelType(vmTypeName);
            }

            foreach (var data in formData)
            {
                SetPropertyValue(session.ViewModel, data.Key, data.Value);
            }

            var vmType = session.ViewModel.GetType();
            if (vmType.Name != vmTypeName)
            {
                Trace.TraceWarning(
                    $"Stonehenge3.ViewModelProvider: Request for VM={vmTypeName}, current VM={vmType.Name}");
                return new Resource(resourceName, "ViewModelProvider",
                    ResourceType.Json, "{ \"StonehengeContinuePolling\":false }", Resource.Cache.None);
            }

            var method = vmType.GetMethod(methodName);
            if (method == null) return null;

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
                Trace.TraceError("Stonehenge3.ViewModelProvider: " + ex.Message);
                Trace.TraceError("Stonehenge3.ViewModelProvider: " + ex.StackTrace);

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

        private static bool SetViewModel(AppSession session, string resourceName)
        {
            var vmTypeName = Path.GetFileNameWithoutExtension(resourceName);
            if ((session.ViewModel != null) && (session.ViewModel.GetType().Name == vmTypeName)) return true;
            if (session.SetViewModelType(vmTypeName) != null)
            {
                return true;
            }

            Trace.TraceError("Could not set ViewModel type to " + vmTypeName);
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

            string json;
            if (vmTypeName != vmType?.Name)
            {
                json = "{ \"StonehengeContinuePolling\":false }";
                return new Resource(resourceName, "ViewModelProvider", ResourceType.Json, json, Resource.Cache.None);
            }

            var data = new List<string> {"\"StonehengeContinuePolling\":true"};
            var events = session.CollectEvents();
            if (events.Count > 0)
            {
                if (session.ViewModel is ActiveViewModel activeVm)
                {
                    foreach (var property in events)
                    {
                        var value = activeVm.TryGetMember(property);
                        data.Add($"\"{property}\":{JsonSerializer.SerializeObjectString(null, value)}");
                    }

                    AddStonehengeInternalProperties(data, activeVm);
                }
            }

            json = "{" + string.Join(",", data) + "}";
            return new Resource(resourceName, "ViewModelProvider", ResourceType.Json, json, Resource.Cache.None);
        }

        private static void AddStonehengeInternalProperties(ICollection<string> data, ActiveViewModel activeVm)
        {
            if (!string.IsNullOrEmpty(activeVm.MessageBoxTitle) || !string.IsNullOrEmpty(activeVm.MessageBoxText))
            {
                var title = activeVm.MessageBoxTitle;
                var text = activeVm.MessageBoxText;
                var script =
                    $"alert('{HttpUtility.JavaScriptStringEncode(title)}\\r\\n{HttpUtility.JavaScriptStringEncode(text)}');";
                data.Add($"\"StonehengeEval\":{JsonSerializer.SerializeObjectString(null, script)}");
                activeVm.MessageBoxTitle = null;
                activeVm.MessageBoxText = null;
            }

            if (!string.IsNullOrEmpty(activeVm.NavigateToRoute))
            {
                var route = activeVm.NavigateToRoute;
                data.Add($"\"StonehengeNavigate\":{JsonSerializer.SerializeObjectString(null, route)}");
                activeVm.NotifyPropertyChanged("StonehengeNavigate");
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

        private static object DeserializePropertyValue(string propValue, Type propType)
        {
            if (propType == typeof(string))
                return propValue;
            if (propType == typeof(bool))
                return bool.Parse(propValue);

            try
            {
                return JsonConvert.DeserializeObject(propValue, propType);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
        }

        private static void SetPropertyValue(object vm, string propName, string newValue)
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
                                        var val = DeserializePropertyValue(member.Value, mProp.PropertyType);
                                        mProp.SetValue(structObj, val, null);
                                    }
                                }
                            }

                            activeVm.TrySetMember(propName, structObj);
                        }
                    }
                    else if (pi.PropertyType.IsGenericType && pi.PropertyType.Name.StartsWith("Notify`"))
                    {
                        var val = DeserializePropertyValue(newValue, pi.PropertyType.GenericTypeArguments[0]);
                        var type = typeof(Notify<>).MakeGenericType(pi.PropertyType.GenericTypeArguments[0]);
                        var notify = Activator.CreateInstance(type);
                        var valueField = type.GetField("_value", BindingFlags.Instance | BindingFlags.NonPublic);
                        valueField?.SetValue(notify, val);
                        activeVm.TrySetMember(propName, notify);
                    }
                    else
                    {
                        var val = DeserializePropertyValue(newValue, pi.PropertyType);
                        activeVm.TrySetMember(propName, val);
                    }
                }
                else
                {
                    var pi = vm.GetType().GetProperty(propName);
                    if ((pi == null) || !pi.CanWrite)
                        return;

                    var val = DeserializePropertyValue(newValue, pi.PropertyType);
                    pi.SetValue(vm, val, null);
                }
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            // ReSharper restore EmptyGeneralCatchClause
        }

        private static string GetViewModelJson(object viewModel)
        {
            var watch = new Stopwatch();
            watch.Start();
            
            var ty = viewModel.GetType();
            Trace.TraceInformation("Stonehenge3.ViewModelProvider: viewModel=" + ty.Name);

            var data = new List<string>();
            try
            {
                if (viewModel is ActiveViewModel activeVm)
                {
                    foreach (var model in activeVm.ActiveModels)
                    {
                        data.AddRange(JsonSerializer.SerializeObject(model.Prefix, model.Model));
                    }

                    AddStonehengeInternalProperties(data, activeVm);

                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (var name in activeVm.GetDictionaryNames())
                    {
                        // ReSharper disable once UseStringInterpolation
                        data.Add(string.Format("\"{0}\":{1}", name,
                            JsonConvert.SerializeObject(activeVm.TryGetMember(name))));
                    }
                }

                data.AddRange(JsonSerializer.SerializeObject(null, viewModel));
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                Trace.TraceError(ex.StackTrace);

                var exResource = new JObject
                {
                    ["Message"] = ex.Message,
                    ["StackTrace"] = ex.StackTrace
                };
                return JsonConvert.SerializeObject(exResource);
            }

            var json = "{" + string.Join(",", data) + "}";
            
            watch.Stop();
            Debug.WriteLine($"GetViewModelJson: {watch.ElapsedMilliseconds}ms");
            return json;
        }
    }
}