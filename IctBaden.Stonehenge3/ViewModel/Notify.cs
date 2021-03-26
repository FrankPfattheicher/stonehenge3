using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IctBaden.Stonehenge3.ViewModel
{
    /// <summary>
    /// Add view model property support
    /// for NotifyPropertyChanged.
    /// Works with ActiveViewModel classes only !
    /// Use property.Update(newValue) to use.
    /// </summary>
    /// <typeparam name="T">Type of the property.</typeparam>
    [JsonConverter(typeof(NotifyJsonConverter))]
    public class Notify<T>
    {
        private readonly ActiveViewModel _viewModel;
        private readonly string _name;
        private T _value;

        // ReSharper disable once UnusedMember.Global
        public Notify(ActiveViewModel viewModel, string name)
            // ReSharper disable once IntroduceOptionalParameters.Global
            :this(viewModel, name, default)
        {
        }
        // ReSharper disable once MemberCanBePrivate.Global
        public Notify(ActiveViewModel viewModel, string name, T value)
        {
            _viewModel = viewModel;
            _name = name;
            _value = value;
        }

        public void Update(T value)
        {
            _value = value;
            _viewModel?.NotifyPropertyChanged(_name);
        }

        // public static implicit operator Notify<T>(T value)
        // {
        //     var stackTrace = new StackTrace();
        //     var vmType = stackTrace.GetFrame(1)?.GetMethod()?.DeclaringType;
        //     var propertyName = // no idea to find out
        //     var n = new Notify<T>(null, "", value);
        //     return n;
        // }

        public static implicit operator T(Notify<T> value)
        {
            return value._value;
        }
    }
 
    public class NotifyJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            var vt = value.GetType();
            var valueField = vt.GetField("_value", BindingFlags.Instance | BindingFlags.NonPublic);
            value = valueField?.GetValue(value);

            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var t = JToken.FromObject(value);

            if (t.Type != JTokenType.Object)
            {
                t.WriteTo(writer);
            }
            else
            {
                var o = (JObject)t;
                IList<string> propertyNames = o.Properties().Select(p => p.Name).ToList();

                o.AddFirst(new JProperty("Keys", new JArray(propertyNames)));

                o.WriteTo(writer);
            }
        }

        public override bool CanRead => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {

            return existingValue.ToString();
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
    
    
}