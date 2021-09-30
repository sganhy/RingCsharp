using System;
using System.Diagnostics;

namespace Ring.Data.Models
{
    internal sealed class Logger
    {
        public readonly Type Type;
        public readonly StackTrace StackTrace;

        public Logger(Type type)
        {
            Type = type;
            StackTrace = new StackTrace();
        }

    }
}
