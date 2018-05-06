using System;
using System.Diagnostics;

namespace IctBaden.Stonehenge3.Aurelia.SampleCore
{
    /// <inheritdoc />
    /// <summary>
    /// Not included in .NET Standard 2.0
    /// </summary>
    internal class ConsoleTraceListener : TextWriterTraceListener
    {
        public ConsoleTraceListener() : base(Console.Out)
        {
        }
        // ReSharper disable once UnusedMember.Global
        public ConsoleTraceListener(bool useErrorStream) : base(useErrorStream ? Console.Error : Console.Out)
        {
        }
        public override void Close()
        {
            // No resources to clean up.
        }
    }
}