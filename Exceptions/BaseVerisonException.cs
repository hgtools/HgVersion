using System;
using System.Runtime.Serialization;

namespace HgVersion.Exceptions
{
    [Serializable]
    public class BaseVerisonException : Exception
    {
        public BaseVerisonException() { }
        public BaseVerisonException(string message) : base(message) { }
        public BaseVerisonException(string message, Exception inner) : base(message, inner) { }

        protected BaseVerisonException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        { }
    }
}