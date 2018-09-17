using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.Resources;
using Newtonsoft.Json;

namespace IctBaden.Stonehenge3.ViewModel
{
    public class ViewModelProvider : IStonehengeResourceProvider
    {
        public void Dispose()
        {
        }

        public Resource Post(AppSession session, string resourceName, Dictionary<string, string> parameters, Dictionary<string, string> formData)
        {
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
                Trace.TraceWarning($"Stonehenge3.ViewModelProvider: Request for VM={vmTypeName}, current VM={vmType.Name}");
                return new Resource(resourceName, "ViewModelProvider", 
                    ResourceType.Json, "{ \"StonehengeContinuePolling\":false }", Resource.Cache.None);
            }

            var method = vmType.GetMethod(methodName);
            if (method == null) return null;

            try
            {
                var methodParams =
                    method.GetParameters()
                        .Zip(
                            parameters.Values,
                            (parameterInfo, postParam) =>
                            new KeyValuePair<Type, object>(parameterInfo.ParameterType, postParam))
                        .Select(parameterPair => Convert.ChangeType(parameterPair.Value, parameterPair.Key))
                        .ToArray();
                method.Invoke(session.ViewModel, methodParams);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) ex = ex.InnerException;
                Trace.TraceError("Stonehenge3.ViewModelProvider: " + ex.Message);
                Trace.TraceError("Stonehenge3.ViewModelProvider: " + ex.StackTrace);
                Debug.Assert(false);
                // ReSharper disable once HeuristicUnreachableCode
                return new Resource(resourceName, "ViewModelProvider", ResourceType.Json, GetViewModelJson(ex), Resource.Cache.None);
            }
            return new Resource(resourceName, "ViewModelProvider", ResourceType.Json, GetViewModelJson(session.ViewModel), Resource.Cache.None);
        }

        public Resource Get(AppSession session, string resourceName, Dictionary<string, string> parameters)
        {
            if (resourceName.StartsWith("ViewModel/"))
            {
                if(SetViewModel(session, resourceName))
                    return GetViewModel(session, resourceName);
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
            if (session.SetViewModelType(vmTypeName) != null) return true;

            Trace.TraceError("Could not set ViewModel type to " + vmTypeName);
            return false;
        }

        private Resource GetViewModel(AppSession session, string resourceName)
        {
            session.EventsClear(true);

            return new Resource(resourceName, "ViewModelProvider", ResourceType.Json, GetViewModelJson(session.ViewModel),
                Resource.Cache.None);
        }

        private static Resource GetEvents(AppSession session, string resourceName)
        {
            var parts = resourceName.Split('/');
            if (parts.Length < 2) return null;

            var vmTypeName = parts[1];
            var vmType = session.ViewModel.GetType();

            string json;
            if (vmTypeName != vmType.Name)
            {
                json = "{ \"StonehengeContinuePolling\":false }";
                return new Resource(resourceName, "ViewModelProvider", ResourceType.Json, json, Resource.Cache.None);
            }

            var data = new List<string> { "\"StonehengeContinuePolling\":true" };
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

        private static Resource GetDataResource(AppSession session, string resourceName, Dictionary<string, string> parameters)
        {
            var vm = session.ViewModel as ActiveViewModel;
            var method = vm?.GetType()
                .GetMethods()
                .FirstOrDefault(m => string.Compare(m.Name, "GetDataResource", StringComparison.InvariantCultureIgnoreCase) == 0);
            if (method == null || method.ReturnType != typeof(Resource)) return null;

            Resource data;
            if (method.GetParameters().Length == 2)
            {
                data = (Resource)method.Invoke(vm, new object[] { resourceName, parameters });
            }
            else
            {
                data = (Resource)method.Invoke(vm, new object[] { resourceName });
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

                    if (pi.PropertyType.IsValueType && !pi.PropertyType.IsPrimitive && (pi.PropertyType.Namespace != "System")) // struct
                    {
                        object structObj = activeVm.TryGetMember(propName);
                        if (structObj != null)
                        {
                            if (JsonConvert.DeserializeObject(newValue, typeof(Dictionary<string, string>)) is Dictionary<string, string> members)
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
            var ty = viewModel.GetType();
            Trace.TraceInformation("Stonehenge3.ViewModelProvider: viewModel=" + ty.Name);

            var data = new List<string>();
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
                    data.Add(string.Format("\"{0}\":{1}", name, JsonConvert.SerializeObject(activeVm.TryGetMember(name))));
                }
            }

            data.AddRange(JsonSerializer.SerializeObject(null, viewModel));

            var json = "{" + string.Join(",", data) + "}";
            return json;
        }

    }
}