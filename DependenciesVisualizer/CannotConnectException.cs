using System;

namespace DependenciesVisualizer
{
    public class CannotConnectException : Exception
    {
        public CannotConnectException()
        {
        }
        public CannotConnectException(string message) : base(message)
        {
        }
        public CannotConnectException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

    }
}
