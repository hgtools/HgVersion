using System;
using System.Runtime.Serialization;

namespace HgVersion.Configuration
{
    [Serializable]
    public class HgConfigrationException : Exception
    {
        public HgConfigrationException()
        { }

        public HgConfigrationException(string message) : base(message)
        { }

        public HgConfigrationException(string message, Exception inner) : base(message, inner)
        { }

        protected HgConfigrationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        { }
    }
}