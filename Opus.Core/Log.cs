// <copyright file="Log.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus logging.</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    /// <summary>
    /// Opus logging static class.
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// Log a message, either to the debugger output window or the console.
        /// </summary>
        /// <param name="messageValue">Message to output.</param>
        private static void Message(string messageValue)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debug.WriteLine(messageValue);
            }
            else
            {
                System.Console.WriteLine(messageValue);
            }
        }

        /// <summary>
        /// Log a message, either to the debugger output window or the console.
        /// </summary>
        /// <param name="level">Verbose level to use</param>
        /// <param name="format">Format of message to output.</param>
        /// <param name="args">Array of objects used in the formatting.</param>
        public static void Message(EVerboseLevel level, string format, params object[] args)
        {
            if (State.VerbosityLevel >= level)
            {
                string formattedMessage = System.String.Format(format, args);
                Message(formattedMessage);
            }
        }

        public static void MessageAll(string format, params object[] args)
        {
            string formattedMessage = System.String.Format(format, args);
            Message(formattedMessage);
        }

        public static void Info(string format, params object[] args)
        {
            if (State.VerbosityLevel >= EVerboseLevel.Info)
            {
                string formattedMessage = System.String.Format(format, args);
                Message(formattedMessage);
            }
        }

        public static void Detail(string format, params object[] args)
        {
            if (State.VerbosityLevel >= EVerboseLevel.Detail)
            {
                string formattedMessage = System.String.Format(format, args);
                Message(formattedMessage);
            }
        }

        public static void ErrorMessage(string format, params object[] args)
        {
            string formattedMessage = System.String.Format(format, args);
            string errorMessage = System.String.Format("\nERROR: {0}", formattedMessage);
            Message(errorMessage);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void DebugMessage(string format, params object[] args)
        {
            if (State.VerbosityLevel > EVerboseLevel.Detail)
            {
                string formattedMessage = System.String.Format(format, args);
                Message(formattedMessage);
            }
        }
    }
}