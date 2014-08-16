// <copyright file="Exception.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus application exceptions.</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    /// <summary>
    /// Opus Exception class.
    /// </summary>
    [System.Serializable]
    public class Exception :
        System.Exception
    {
        /// <summary>
        /// Initializes a new instance of the Exception class.
        /// </summary>
        public
        Exception()
        {}

        /// <summary>
        /// Initializes a new instance of the Exception class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public
        Exception(
            string message) : base(message)
        {}

        /// <summary>
        /// Initialize a new instance of the Exception class.
        /// </summary>
        /// <param name="format">Format string.</param>
        /// <param name="args">Variable number of arguments to satisfy the format string.</param>
        public
        Exception(
            string format,
            params object[] args) : base(System.String.Format(format, args))
        {}

        /// <summary>
        /// Initializes a new instance of the Exception class.
        /// </summary>
        /// <param name="innerException">Inner exception.</param>
        /// <param name="format">Format string.</param>
        /// <param name="args">Variable number of arguments to satisfy the format string.</param>
        public
        Exception(
            System.Exception innerException,
            string format,
            params object[] args) : base(System.String.Format(format, args), innerException)
        {}

        /// <summary>
        /// Initializes a new instance of the Exception class.
        /// </summary>
        /// <param name="serializationInfo">Serialization information.</param>
        /// <param name="streamingContext">Streaming context.</param>
        protected
        Exception(
            System.Runtime.Serialization.SerializationInfo serializationInfo,
            System.Runtime.Serialization.StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {}

        public static bool
        DisplayException(
            System.Exception exception)
        {
            if (null == exception)
            {
                return false;
            }

            var anyInnerExceptions = DisplayException(exception.InnerException);
            Log.ErrorMessage("({0}) {1}", exception.GetType().ToString(), exception.Message);
            if (!anyInnerExceptions)
            {
                Log.ErrorMessage("{0}{1}", System.Environment.NewLine, exception.StackTrace.ToString());
            }

            return true;
        }

        public static void
        DisplayException(
            System.Exception exception,
            string prefix,
            params object[] args)
        {
            Log.ErrorMessage(prefix, args);
            DisplayException(exception);
        }
    }
}
