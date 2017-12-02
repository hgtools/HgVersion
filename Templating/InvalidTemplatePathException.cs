using System;

namespace HgVersion.Templating
{
    [Serializable]
    public class InvalidTemplatePathException : Exception
    {
        public InvalidTemplatePathException() { }
        public InvalidTemplatePathException(string message) : base(message) { }
        public InvalidTemplatePathException(string message, Exception inner) : base(message, inner) { }
        protected InvalidTemplatePathException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
