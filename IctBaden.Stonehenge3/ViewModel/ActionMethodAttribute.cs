using System;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace IctBaden.Stonehenge3.ViewModel
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ActionMethodAttribute : Attribute
    {
        /// <summary>
        /// Executes handler method in separate task if set to true.
        /// </summary>
        public bool ExecuteAsync { get; set; }

    }
}