using System;

namespace UpdateNight.Exceptions
{
    public class UpdateNightException : Exception
    {
        public UpdateNightException(string message) : base(message) {}
        
        public UpdateNightException(string message, Exception exception) : base(message, exception) {} 
    }
}