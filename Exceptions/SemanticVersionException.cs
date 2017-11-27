using System;
using System.Runtime.Serialization;

namespace HgVersion.Exceptions
{
    [Serializable]
    public class SemanticVersionExceptionException : Exception
    {
        public SemanticVersionExceptionException() { }
        public SemanticVersionExceptionException(string message) : base(message) { }
        public SemanticVersionExceptionException(string message, Exception inner) : base(message, inner) { }

        protected SemanticVersionExceptionException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        { }
    }
}