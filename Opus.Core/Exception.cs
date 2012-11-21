// <copyright file="Exception.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus application exceptions.</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    /// <summary>
    /// Opus Exception class.
    /// </summary>
    [System.Serializable]
    public class Exception : System.Exception
    {
        /// <summary>
        /// Initializes a new instance of the Exception class.
        /// </summary>
        public Exception()
        {
            this.RequiresStackTrace = true;
        }

        /// <summary>
        /// Initializes a new instance of the Exception class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public Exception(string message)
            : base(message)
        {
            this.RequiresStackTrace = true;
        }

        /// <summary>
        /// Initializes a new instance of the Exception class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="requiresStackTrace">True if a stack trace is required.</param>
        public Exception(string message, bool requiresStackTrace)
            : base(message)
        {
            this.RequiresStackTrace = requiresStackTrace;
        }
 
        /// <summary>
        /// Initializes a new instance of the Exception class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public Exception(string message, System.Exception innerException)
            : base(message, innerException)
        {
            this.RequiresStackTrace = true;
        }
        
        /// <summary>
        /// Initializes a new instance of the Exception class.
        /// </summary>
        /// <param name="serializationInfo">Serialization information.</param>
        /// <param name="streamingContext">Streaming context.</param>
        protected Exception(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
            this.RequiresStackTrace = true;
        }

        public bool RequiresStackTrace
        {
            get;
            private set;
        }

        public static bool DisplayException(System.Exception exception)
        {
            if (null == exception)
            {
                return false;
            }

            bool anyInnerExceptions = DisplayException(exception.InnerException);
            Log.ErrorMessage("{0} ({1})", exception.Message, exception.GetType().ToString());
            if (!anyInnerExceptions)
            {
                Log.ErrorMessage(exception.StackTrace.ToString());
            }

            return true;
        }

        public static void DisplayException(System.Exception exception, string prefix)
        {
            Log.ErrorMessage(prefix);
            DisplayException(exception);
        }
    }
}