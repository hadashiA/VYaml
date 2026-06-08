using System;

namespace VYaml
{
    public class YamlException : Exception
    {
        public YamlException(string message) : base(message)
        {
        }

        public YamlException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
