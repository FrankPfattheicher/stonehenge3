using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.Hosting;
using IctBaden.Stonehenge3.Resources;
using IctBaden.Stonehenge3.ViewModel;
using Newtonsoft.Json;
// ReSharper disable MemberCanBePrivate.Global

namespace IctBaden.Stonehenge3.Vue.Client
{
    internal class VueAppCreator
    {
        private readonly string _appTitle;
        private string _startPage;
        private readonly StonehengeHostOptions _options;
        private readonly Dictionary<string, Resource> _vueContent;

        private static readonly string ControllerTemplate = LoadResourceText("IctBaden.Stonehenge3.Vue.Client.stonehengeComponent.js");
        private static readonly string ElementTemplate = LoadResourceText("IctBaden.Stonehenge3.Vue.Client.stonehengeElement.js");

        public VueAppCreator(string appTitle, string startPage, StonehengeHostOptions options, Dictionary<string, Resource> vueContent)
        {
            _appTitle = appTitle;
            _startPage = startPage;
            _options = options;
            _vueContent = vueContent;
        }

        private static string LoadResourceText(string resourceName)
        {
            return LoadResourceText(Assembly.GetAssembly(typeof(VueAppCreator)), resourceName);
        }

        private static string LoadResourceText(Assembly assembly, string resourceName)
        {
            var resourceText = string.Empty;

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
              if (stream == null) return resourceText;
              using (var reader = new StreamReader(stream))
              {
                resourceText = reader.ReadToEnd();
              }
            }

            return resourceText;
        }

        public void CreateApplication()
        {
            var applicationJs = LoadResourceText("IctBaden.Stonehenge3.Vue.Client.stonehengeApp.js");
            applicationJs = InsertRoutes(applicationJs);
            applicationJs = InsertElements(applicationJs);

            var resource = new Resource("app.js", "VueResourceProvider", ResourceType.Html, applicationJs, Resource.Cache.Revalidate);
            _vueContent.Add("app.js", resource);
        }

        private string InsertRoutes(string pageText)
        {
            const string routesInsertPoint = "//stonehengeAppRoutes";
            const string stonehengeAppTitleInsertPoint = "stonehengeAppTitle";
            const string stonehengeRootPageInsertPoint = "stonehengeRootPage";
            const string pageTemplate = "{{ path: '{0}', name: '{1}', title: '{2}', component: () => Promise.resolve(stonehengeLoadComponent('{3}')), visible: {4} }}";

            var contentPages = _vueContent
                .Where(res => res.Value.ViewModel?.ElementName == null)
                .Select(res => new {res.Value.Name, Vm = res.Value.ViewModel})
                .OrderBy(route => route.Vm.SortIndex)
                .ToList();
            
            var pages = contentPages
                .Select(route => string.Format(pageTemplate,
                                            "/" + route.Name,
                                            route.Name,
                                            route.Vm.Title,
                                            route.Name,
                                            route.Vm.Visible ? "true" : "false" ))
                .ToList();

            if (!contentPages.Any())
            {
                Trace.TraceError("Stonehenge3.VueAppCreator: No content pages found.");
            }
            else if (string.IsNullOrEmpty(_startPage))
            {
                _startPage = contentPages.First().Name;
            }
            
            var startPage = _vueContent.FirstOrDefault(page => page.Value.Name == _startPage);
            if(startPage.Key != null)
            {
                pages.Insert(0, string.Format(pageTemplate, "", "", startPage.Value.ViewModel.Title, startPage.Value.Name, "false"));
            }
            
            var routes = string.Join("," + Environment.NewLine, pages);
            pageText = pageText
                .Replace(routesInsertPoint, routes)
                .Replace(stonehengeAppTitleInsertPoint, _appTitle)
                .Replace(stonehengeRootPageInsertPoint, _startPage);

            return pageText;
        }


        public void CreateComponents()
        {
            var viewModels = _vueContent
                .Where(res => res.Value.ViewModel?.VmName != null)
                .Select(res => res.Value)
                .Distinct()
                .ToList();

            foreach (var viewModel in viewModels)
            {
                var controllerJs = GetController(viewModel.ViewModel.VmName);
                if (!string.IsNullOrEmpty(controllerJs))
                {
                    if (string.IsNullOrEmpty(viewModel.ViewModel.VmName))
                    {
                        Trace.TraceError($"VueAppCreator.CreateControllers: <UNKNOWN VM> => src.{viewModel.Name}.js");
                    }
                    else
                    {
                        Trace.TraceInformation($"VueAppCreator.CreateControllers: {viewModel.ViewModel.VmName} => src.{viewModel.Name}.js");

                        var assembly = Assembly.GetEntryAssembly();
                        var name = assembly?.GetManifestResourceNames()
                            .FirstOrDefault(rn => rn.EndsWith($".app.{viewModel.Name}_user.js"));
                        if (!string.IsNullOrEmpty(name))
                        {
                            var userJs = LoadResourceText(assembly, name);
                            if (!string.IsNullOrWhiteSpace(userJs))
                            {
                                controllerJs += userJs;
                            }
                        }

                        var resource = new Resource($"{viewModel.Name}.js", "VueResourceProvider", ResourceType.Js, controllerJs, Resource.Cache.Revalidate);
                        _vueContent.Add(resource.Name, resource);
                    }
                }
            }
        }

        private static Type[] GetAssemblyTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (Exception)
            {
                return new Type[0];
            }
        }

        private static Type GetVmType(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(GetAssemblyTypes)
                .FirstOrDefault(type => type.Name == name);
        }

        private static readonly bool DebugBuild = Assembly.GetEntryAssembly()?.GetCustomAttributes(false)
                                                      .OfType<DebuggableAttribute>().Any(da => da.IsJITTrackingEnabled) ?? true;
        
        private string GetController(string vmName)
        {
            var vmType = GetVmType(vmName);
            if (vmType == null)
            {
                Trace.TraceError($"No VM for type {vmName} defined.");
                Debug.Assert(false, $"No VM for type {vmName} defined.");
                // ReSharper disable once HeuristicUnreachableCode
                return null;
            }

            var viewModel = CreateViewModel(vmType);
            
            var text = ControllerTemplate
                .Replace("stonehengeDebugBuild", DebugBuild ? "true" : "false")
                .Replace("stonehengeViewModelName", vmName)
                .Replace("stonehengePollDelay", _options.GetPollDelayMs().ToString());

            var propertyNames = GetPropNames(viewModel);
            if (propertyNames.Count > 0)
            {
                var propDefinitions = propertyNames.Select(pn => pn + " : ''\r\n");
                text = text.Replace("//stonehengeProperties", "," + string.Join(",", propDefinitions));
            }
            
            var postBackPropNames = GetPostBackPropNames(viewModel, propertyNames)
                .Select(name => "'" + name + "'");
            text = text.Replace("'propNames'", string.Join(",", postBackPropNames));

            // supply functions for action methods
            const string methodTemplate = @"stonehengeMethodName: function({paramNames}) { app.stonehengeViewModelName.StonehengePost('/ViewModel/stonehengeViewModelName/stonehengeMethodName{paramValues}'); }";

            var actionMethods = new List<string>();
            foreach (var methodInfo in vmType.GetMethods().Where(methodInfo => methodInfo.GetCustomAttributes(false).OfType<ActionMethodAttribute>().Any()))
            {
                //var method = (methodInfo.GetParameters().Length > 0)
                //  ? "%method%: function (data, event, param) { if(!IsLoading()) post_ViewModelName_Data(self, event.currentTarget, '%method%', param); },".Replace("%method%", methodInfo.Name)
                //  : "%method%: function (data, event) { if(!IsLoading()) post_ViewModelName_Data(self, event.currentTarget, '%method%', null); },".Replace("%method%", methodInfo.Name);

                var paramNames = methodInfo.GetParameters().Select(p => p.Name).ToArray();
                var paramValues = paramNames.Any()
                ? "?" + string.Join("&", paramNames.Select(n => string.Format("{0}='+encodeURIComponent({0})+'", n)))
                : string.Empty;

                var method = methodTemplate
                    .Replace("stonehengeViewModelName", vmName)
                    .Replace("stonehengeMethodName", methodInfo.Name)
                    .Replace("stonehengePollDelay", _options.GetPollDelayMs().ToString())
                    .Replace("{paramNames}", string.Join(",", paramNames))
                    .Replace("{paramValues}", paramValues)
                    .Replace("+''", string.Empty);

                actionMethods.Add(method);
            }

            var disposeVm = viewModel as IDisposable;
            disposeVm?.Dispose();

            return text.Replace("/*commands*/", string.Join("," + Environment.NewLine, actionMethods));
        }

        private object CreateViewModel(Type vmType)
        {
            try
            {
                var session = new AppSession(null, _options);
                var viewModel = session.CreateType(vmType);
                return viewModel;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Failed to create ViewModel '{vmType.Name}' : " + ex.Message);
                return null;
            }
        }
        
        
        private static List<string> GetPropNames(object viewModel)
        {
            // properties
            if(viewModel == null) return new List<string>();

            var vmProps = new List<PropertyDescriptor>();
            if (viewModel is ActiveViewModel activeVm)
            {
                vmProps.AddRange(from PropertyDescriptor prop in activeVm.GetProperties() select prop);
            }
            else
            {
                vmProps.AddRange(TypeDescriptor.GetProperties(viewModel, true).Cast<PropertyDescriptor>());
            }

            // ReSharper disable once IdentifierTypo
            var propertyNames = (from prop in vmProps
                let bindable = prop.Attributes.OfType<BindableAttribute>().ToArray()
                where (bindable.Length <= 0) || bindable[0].Bindable
                select prop.Name).ToList();

            return propertyNames;
        }
        
        private static List<string> GetPostBackPropNames(object viewModel, IEnumerable<string> propertyNames)
        {
            if(viewModel == null) return new List<string>();
            
            var postBackPropNames = new List<string>();
            var activeVm = viewModel as ActiveViewModel;
            var vmType = viewModel.GetType();
            foreach (var propName in propertyNames)
            {
                // do not send ReadOnly or OneWay bound properties back
                var prop = vmType.GetProperty(propName);
                if (prop == null)
                {
                    if (activeVm == null)
                        continue;
                    prop = activeVm.GetPropertyInfo(propName);
                    if ((prop != null) && activeVm.IsPropertyReadOnly(propName))
                        continue;
                }

                if (prop?.GetSetMethod(false) == null) // not public writable
                    continue;
                // ReSharper disable once IdentifierTypo
                var bindable = prop.GetCustomAttributes(typeof(BindableAttribute), true);
                if ((bindable.Length > 0) && ((BindableAttribute)bindable[0]).Direction == BindingDirection.OneWay)
                    continue;
                postBackPropNames.Add(propName);
            }

            return postBackPropNames;
        }

        private string InsertElements(string pageText)
        {
            const string elementsInsertPoint = "//stonehengeElements";
            var elements = CreateElements();

            return pageText.Replace(elementsInsertPoint, string.Join(Environment.NewLine, elements));
        }

        public List<string> CreateElements()
        {
            var customElements = _vueContent
               .Where(res => res.Value.ViewModel?.ElementName != null)
               .Select(res => res.Value)
               .Distinct()
               .ToList();

            var assembly = Assembly.GetEntryAssembly();
            var elements = new List<string>();
            foreach (var element in customElements)
            {
                var elementJs = ElementTemplate.Replace("stonehengeCustomElementName", element.ViewModel.ElementName);
                
                var source = Path.GetFileNameWithoutExtension(ResourceLoader.RemoveResourceProtocol(element.Source));
                elementJs = elementJs.Replace("stonehengeViewModelName", source);

                var bindings = element.ViewModel?.Bindings?.Select(b => $"'{b}'") ?? new List<string>() { string.Empty };
                elementJs = elementJs.Replace("stonehengeCustomElementProps", string.Join(",", bindings));

                var template = LoadResourceText(assembly, $"{source}.html");
                template = JsonConvert.SerializeObject(template);
                elementJs = elementJs.Replace("'stonehengeElementTemplate'", template);

                var methods = LoadResourceText(assembly, $"{source}.js");
                if (!string.IsNullOrEmpty(methods)) methods = "," + methods;
                elementJs = elementJs.Replace("//stonehengeElementMethods", methods);

                elements.Add(elementJs);

                var resource = new Resource($"{element.Name}.js", "VueResourceProvider", ResourceType.Js, elementJs, Resource.Cache.Revalidate);
                _vueContent.Add(resource.Name, resource);
            }

            return elements;
        }
    }
}