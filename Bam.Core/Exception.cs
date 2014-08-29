#region License
// Copyright 2010-2014 Mark Final
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
    /// BuildAMation Exception class.
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
