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
namespace XcodeProjectProcessor
{
    /// <summary>
    /// Attribute representing an array of strings in Xcode.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public sealed class StringArrayAttribute :
        BaseAttribute
    {
        /// <summary>
        /// Construct an instance of the attribute.
        /// </summary>
        /// <param name="property">Name of the property.</param>
        /// <param name="prefix">Optional prefix to all strings. Default is null.</param>
        /// <param name="spacesSeparate">Optional whether spaces in the value separate into lines in the project file. Default is false.</param>
        /// <param name="ignore"></param>
        public StringArrayAttribute(
            string property,
            string prefix = null,
            bool spacesSeparate = false,
            bool ignore = false
        )
            :
            base(property, ValueType.MultiValued, ignore: ignore)
        {
            this.Prefix = prefix;
            this.SpacesSeparate = spacesSeparate;
        }

        /// <summary>
        /// Get the prefix.
        /// </summary>
        public string Prefix { get; private set; }

        /// <summary>
        /// Whether spaces in the value separate into lines in the Xcode project.
        /// </summary>
        public bool SpacesSeparate { get; private set; }
    }
}
