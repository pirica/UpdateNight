using System;

namespace UpdateNight.Exceptions
{
    public class RuntimeException : Exception
    {
        public RuntimeException(string message) : base(message) { }

        public RuntimeException(string message, Exception exception) : base(message, exception) { }
    }

    public class RuntimeNotFoundException : Exception
    {
        public RuntimeNotFoundException(string message) : base(message) { }

        public RuntimeNotFoundException(string message, Exception exception) : base(message, exception) { }
    }

    public class InvalidRuntimeException : Exception
    {
        public InvalidRuntimeException(string message) : base(message) { }

        public InvalidRuntimeException(string message, Exception exception) : base(message, exception) { }
    }
}