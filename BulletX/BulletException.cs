using System;

namespace BulletX
{
    class BulletException : Exception
    {
        public BulletException() : base() { }
        public BulletException(string message) : base(message) { }
        public BulletException(string message, Exception innerException) : base(message, innerException) { }
    }
}
