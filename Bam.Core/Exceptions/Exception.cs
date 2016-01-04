#region License
// Copyright (c) 2010-2016, Mark Final
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
            string message) :
            base(message)
        {}

        /// <summary>
        /// Initialize a new instance of the Exception class.
        /// </summary>
        /// <param name="format">Format string.</param>
        /// <param name="args">Variable number of arguments to satisfy the format string.</param>
        public
        Exception(
            string format,
            params object[] args) :
            base(System.String.Format(format, args))
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
            params object[] args) :
            base(System.String.Format(format, args), innerException)
        {}

        /// <summary>
        /// Initializes a new instance of the Exception class.
        /// </summary>
        /// <param name="serializationInfo">Serialization information.</param>
        /// <param name="streamingContext">Streaming context.</param>
        protected
        Exception(
            System.Runtime.Serialization.SerializationInfo serializationInfo,
            System.Runtime.Serialization.StreamingContext streamingContext) :
            base(serializationInfo, streamingContext)
        {}

        /// <summary>
        /// Utility method to display an exception message, and any details from any inner exceptions.
        /// </summary>
        /// <returns><c>true</c>, if the exception is non-null, <c>false</c> otherwise, allowing a recursive invocation for inner exceptions.</returns>
        /// <param name="exception">Exception to display information about.</param>
        public static bool
        DisplayException(
            System.Exception exception)
        {
            if (null == exception)
            {
                return false;
            }

            Log.ErrorMessage("({0}) {1}", exception.GetType().ToString(), exception.Message);
            var anyInnerExceptions = DisplayException(exception.InnerException);
            if (!anyInnerExceptions)
            {
                Log.ErrorMessage("{0}{1}", System.Environment.NewLine, exception.StackTrace.ToString());
            }

            return true;
        }

        /// <summary>
        /// Utility method to display an exception message (and inner exceptions), with a specified error message prefix.
        /// </summary>
        /// <returns><c>true</c>, if the exception is non-null, <c>false</c> otherwise, allowing a recursive invocation for inner exceptions.</returns>
        /// <param name="exception">Exception to display.</param>
        /// <param name="prefix">Prefix format string to display</param>
        /// <param name="args">Arguments to the prefix format string</param>
        public static bool
        DisplayException(
            System.Exception exception,
            string prefix,
            params object[] args)
        {
            Log.ErrorMessage(prefix, args);
            return DisplayException(exception);
        }
    }
}
