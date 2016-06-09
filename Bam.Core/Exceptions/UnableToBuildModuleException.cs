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
    /// Exception class to throw for a module that is unable to build for some reason.
    /// This is most useful for plugins, which could be considerd optional in a build
    /// as they are dynamic runtime extensions. This exception type copes with developers
    /// not having all dependencies in order to build a plugin.
    /// </summary>
    public class UnableToBuildModuleException :
        Exception
    {
        /// <summary>
        /// Construct an exception, with a message of failure, and the type of Module failing.
        /// </summary>
        /// <param name="message">Failure message.</param>
        /// <param name="moduleType">Type of module that is unable to build.</param>
        public UnableToBuildModuleException(
            string message,
            System.Type moduleType) :
            base(message)
        {
            this.ModuleType = moduleType;
        }

        /// <summary>
        /// Construct an exception, with a message of failure. The type of module failing can be set later.
        /// </summary>
        /// <param name="message">Failure message.</param>
        public UnableToBuildModuleException(
            string message) :
            base(message)
        {
        }

        /// <summary>
        /// Get or set the type of module unable to build.
        /// </summary>
        public System.Type ModuleType
        {
            get;
            set;
        }
    }
}
