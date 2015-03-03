#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
namespace Bam.Core
{
    /// <summary>
    /// BuildAMation logging static class.
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// Log a message, either to the debugger output window or the console.
        /// </summary>
        /// <param name="messageValue">Message to output.</param>
        /// <param name="isError">True if an error message, false if standard output.</param>
        private static void
        Message(
            string messageValue,
            bool isError)
        {
            if (System.Diagnostics.Debugger.IsAttached && !State.RunningMono)
            {
                // In MonoDevelop, this goes nowhere that I can find...
                System.Diagnostics.Debug.WriteLine(messageValue);
            }
            else
            {
                if (isError)
                {
                    System.Console.Error.WriteLine(messageValue);
                }
                else
                {
                    System.Console.Out.WriteLine(messageValue);
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
            var escapedText = System.Text.RegularExpressions.Regex.Replace(incoming, @"{([^[0-9]]*)|{$", @"{{$1");
            escapedText = System.Text.RegularExpressions.Regex.Replace(escapedText, @"^}|([^[0-9]]*)}", @"$1}}");
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
            if (State.VerbosityLevel >= level)
            {
                var formattedMessage = new System.Text.StringBuilder();
                formattedMessage.AppendFormat(EscapeString(format), args);
                Message(formattedMessage.ToString(), false);
            }
        }

        public static void
        MessageAll(
            string format,
            params object[] args)
        {
            var formattedMessage = new System.Text.StringBuilder();
            formattedMessage.AppendFormat(EscapeString(format), args);
            Message(formattedMessage.ToString(), false);
        }

        public static void
        Info(
            string format,
            params object[] args)
        {
            if (State.VerbosityLevel >= EVerboseLevel.Info)
            {
                var formattedMessage = new System.Text.StringBuilder();
                formattedMessage.AppendFormat(EscapeString(format), args);
                Message(formattedMessage.ToString(), false);
            }
        }

        public static void
        Detail(
            string format,
            params object[] args)
        {
            if (State.VerbosityLevel >= EVerboseLevel.Detail)
            {
                var formattedMessage = new System.Text.StringBuilder();
                formattedMessage.AppendFormat(EscapeString(format), args);
                Message(formattedMessage.ToString(), false);
            }
        }

        public static void
        Full(
            string format,
            params object[] args)
        {
            if (EVerboseLevel.Full == State.VerbosityLevel)
            {
                var formattedMessage = new System.Text.StringBuilder();
                formattedMessage.AppendFormat(EscapeString(format), args);
                Message(formattedMessage.ToString(), false);
            }
        }

        public static void
        ErrorMessage(
            string format,
            params object[] args)
        {
            var formattedMessage = new System.Text.StringBuilder();
            formattedMessage.AppendFormat(EscapeString("ERROR: " + format), args);
            Message(formattedMessage.ToString(), true);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void
        DebugMessage(
            string format,
            params object[] args)
        {
            if (State.VerbosityLevel > EVerboseLevel.Detail)
            {
                var formattedMessage = new System.Text.StringBuilder();
                formattedMessage.AppendFormat(EscapeString(format), args);
                Message(formattedMessage.ToString(), false);
            }
        }
    }
}
