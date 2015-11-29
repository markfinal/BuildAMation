#region License
// Copyright (c) 2010-2015, Mark Final
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
    /// A mandatory attribute applied to all settings interfaces for tool settings.
    /// This is the metadata that links a settings interface to the static class containing
    /// the extension methods, such as defining default values, associated with
    /// each property of the interface.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public sealed class SettingsExtensionsAttribute :
        System.Attribute
    {
        /// <summary>
        /// Construct an instance of the attribute.
        /// </summary>
        /// <param name="extensionClass">Type of the static class defining the extension methods for this interface.</param>
        public SettingsExtensionsAttribute(
            System.Type extensionClass)
        {
            this.ClassType = extensionClass;
        }

        private System.Type ClassType
        {
            get;
            set;
        }

        /// <summary>
        /// The name of the static extensions class for the interface.
        /// </summary>
        /// <value>The name of class.</value>
        public string ExtensionsClassName
        {
            get
            {
                return this.ClassType.FullName;
            }
        }

        /// <summary>
        /// Given a method name, and an array of parameter types, find that method on the extensions class.
        /// </summary>
        /// <returns>The requested method, or null, if not found.</returns>
        /// <param name="name">Name of the method to find.</param>
        /// <param name="inputs">Array of parameter types that the method takes as input.</param>
        public System.Reflection.MethodInfo
        GetMethod(
            string name,
            params System.Type[] inputs)
        {
            return this.ClassType.GetMethod(name, inputs);
        }
    }
}
