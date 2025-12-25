using System.Runtime.Serialization;

namespace Base.Common.Helpers;
[Serializable]
public class AppException : Exception
{
    public AppException(string message) : base(message)
    {

    }

#pragma warning disable SYSLIB0051 // Type or member is obsolete
    protected AppException(SerializationInfo info, StreamingContext context) : base(info, context)
#pragma warning restore SYSLIB0051 // Type or member is obsolete
    {
        
    }

    public AppException()
    {
    }

    public AppException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
