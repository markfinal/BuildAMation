#region License
// Copyright (c) 2010-2019, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
namespace Bam.Core
{
    /// <summary>
    /// BuildAMation logging static class.
    /// </summary>
    public static class Log
    {
        static private readonly bool ProgressLogEnabled = !CommandLineProcessor.Evaluate(new Options.DisableProgressLogging());

        static Log()
        {
            try
            {
                System.Console.SetCursorPosition(System.Console.CursorLeft, System.Console.CursorTop);
            }
            catch (System.Exception)
            {
                // TravisCI reports System.ArgumentOutOfRangeException for the 'left' parameter
                ProgressLogEnabled = false;
            }
            if (OSUtilities.IsLinuxHosting)
            {
                if (!System.String.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("DISPLAY")))
                {
                    // Linux, .NET core 2.1, X windows - SetCursorPosition is _really_ slow
                    // https://github.com/dotnet/corefx/issues/32174
                    ProgressLogEnabled = false;
                }
            }
        }

        /// <summary>
        /// Log a message, either to the debugger output window or the console.
        /// </summary>
        /// <param name="messageValue">Message to output.</param>
        /// <param name="isError">True if an error message, false if standard output.</param>
        /// <param name="isProgressMeter">True if the text represents a progress meter. Cursor management on the console is required for this. Defaults to false.</param>
        private static void
        Message(
            string messageValue,
            bool isError,
            bool isProgressMeter = false)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                if (!isProgressMeter)
                {
                    // In MonoDevelop, this goes nowhere that I can find...
                    System.Diagnostics.Debug.WriteLine(messageValue);
                }
            }
            else
            {
                if (isError)
                {
                    System.Console.Error.WriteLine(messageValue);
                }
                else
                {
                    if (isProgressMeter)
                    {
                        if (ProgressLogEnabled)
                        {
                            System.Console.Write(messageValue);
                            // there seems to be some issues (the cursor position not updating) in .NET core
                            System.Console.SetCursorPosition(0, System.Console.CursorTop);
                        }
                    }
                    else
                    {
                        System.Console.Out.WriteLine(messageValue);
                    }
                }
            }
        }

        /// <summary>
        /// Escapes invalid characters to use as a string format.
        /// </summary>
        /// <param name="incoming">Original string</param>
        /// <returns>Escaped string</returns>
        private static string
        EscapeString(
            string incoming)
        {
            // based on http://stackoverflow.com/questions/3787875/regex-to-match-delimited-alphanumeric-words
            // match a GUID
            var escapedText = System.Text.RegularExpressions.Regex.Replace(incoming, @"{([A-Za-z0-9]+(?:-[A-Za-z0-9]+)+)}", @"{{$1}}");
            // match just alphabetic blocks
            escapedText = System.Text.RegularExpressions.Regex.Replace(escapedText, @"{([A-Za-z]+)}", @"{{$1}}");
            return escapedText;
        }

        /// <summary>
        /// Log a message, either to the debugger output window or the console.
        /// </summary>
        /// <param name="level">Verbose level to use</param>
        /// <param name="format">Format of message to output.</param>
        /// <param name="args">Array of objects used in the formatting.</param>
        public static void
        Message(
            EVerboseLevel level,
            string format,
            params object[] args)
        {
            if (Graph.Instance.VerbosityLevel < level)
            {
                return;
            }
            if (args.Length > 0)
            {
                var formattedMessage = new System.Text.StringBuilder();
                formattedMessage.AppendFormat(EscapeString(format), args);
                Message(formattedMessage.ToString(), false);
            }
            else
            {
                Message(format, false);
            }
        }

        /// <summary>
        /// Utility function to log a message in all verbosity levels.
        /// </summary>
        /// <param name="format">Format.</param>
        /// <param name="args">Arguments.</param>
        public static void
        MessageAll(
            string format,
            params object[] args)
        {
            if (args.Length > 0)
            {
                var formattedMessage = new System.Text.StringBuilder();
                formattedMessage.AppendFormat(EscapeString(format), args);
                Message(formattedMessage.ToString(), false);
            }
            else
            {
                Message(format, false);
            }
        }

        /// <summary>
        /// Utility function to log a message only when verbosity mode is at least Info.
        /// </summary>
        /// <param name="format">Format.</param>
        /// <param name="args">Arguments.</param>
        public static void
        Info(
            string format,
            params object[] args)
        {
            if (Graph.Instance.VerbosityLevel < EVerboseLevel.Info)
            {
                return;
            }
            if (args.Length > 0)
            {
                var formattedMessage = new System.Text.StringBuilder();
                formattedMessage.AppendFormat(EscapeString(format), args);
                Message(formattedMessage.ToString(), false);
            }
            else
            {
                Message(format, false);
            }
        }

        /// <summary>
        /// Utility function to log a message only when verbosity mode is at least Detail.
        /// </summary>
        /// <param name="format">Format.</param>
        /// <param name="args">Arguments.</param>
        public static void
        Detail(
            string format,
            params object[] args)
        {
            if (Graph.Instance.VerbosityLevel < EVerboseLevel.Detail)
            {
                return;
            }
            if (args.Length > 0)
            {
                var formattedMessage = new System.Text.StringBuilder();
                formattedMessage.AppendFormat(EscapeString(format), args);
                Message(formattedMessage.ToString(), false);
            }
            else
            {
                Message(format, false);
            }
        }

        /// <summary>
        /// Utility function to log a progress meter only when verbosity mode is at least Detail.
        /// Progress meters require console cursor management.
        /// </summary>
        /// <param name="format">Format of the progress meter.</param>
        /// <param name="args">Arguments to fulfil the format string.</param>
        public static void
        DetailProgress(
            string format,
            params object[] args)
        {
            if (Graph.Instance.VerbosityLevel < EVerboseLevel.Detail)
            {
                return;
            }
            if (args.Length > 0)
            {
                var formattedMessage = new System.Text.StringBuilder();
                formattedMessage.AppendFormat(EscapeString(format), args);
                Message(formattedMessage.ToString(), false, true);
            }
            else
            {
                Message(format, false, true);
            }
        }

        /// <summary>
        /// Utility function to log a message only when verbosity mode is at least Full.
        /// </summary>
        /// <param name="format">Format.</param>
        /// <param name="args">Arguments.</param>
        public static void
        Full(
            string format,
            params object[] args)
        {
            if (EVerboseLevel.Full != Graph.Instance.VerbosityLevel)
            {
                return;
            }
            if (args.Length > 0)
            {
                var formattedMessage = new System.Text.StringBuilder();
                formattedMessage.AppendFormat(EscapeString(format), args);
                Message(formattedMessage.ToString(), false);
            }
            else
            {
                Message(format, false);
            }
        }

        /// <summary>
        /// Utility function to display a message, prefixed with ERROR:, in all verbosity levels.
        /// </summary>
        /// <param name="format">Format.</param>
        /// <param name="args">Arguments.</param>
        public static void
        ErrorMessage(
            string format,
            params object[] args)
        {
            if (args.Length > 0)
            {
                var formattedMessage = new System.Text.StringBuilder();
                formattedMessage.AppendFormat(EscapeString($"ERROR: {format}"), args);
                Message(formattedMessage.ToString(), true);
            }
            else
            {
                Message(format, true);
            }
        }

        /// <summary>
        /// Utility function to log a message only when verbosity mode is at least Detail AND in debug builds.
        /// </summary>
        /// <param name="format">Format.</param>
        /// <param name="args">Arguments.</param>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void
        DebugMessage(
            string format,
            params object[] args)
        {
            if (Graph.Instance.VerbosityLevel <= EVerboseLevel.Detail)
            {
                return;
            }
            if (args.Length > 0)
            {
                var formattedMessage = new System.Text.StringBuilder();
                formattedMessage.AppendFormat(EscapeString(format), args);
                Message(formattedMessage.ToString(), false);
            }
            else
            {
                Message(format, false);
            }
        }
    }
}
