using System;

namespace Haste.Exceptions
{
    public class NotInitializedException : Exception
    {
        public NotInitializedException(string message) : base(message)
        {
            
        }
    }
}