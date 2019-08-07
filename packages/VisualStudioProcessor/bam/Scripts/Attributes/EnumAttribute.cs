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
namespace VisualStudioProcessor
{
    /// <summary>
    /// Attribute for enum settings properties.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = true)]
    public class EnumAttribute :
        BaseAttribute
    {
        /// <summary>
        /// The conversion mode of the enum.
        /// </summary>
        public enum EMode
        {
            AsString,               //<! Convert the enumeration value as a string
            AsInteger,              //<! Convert the enumeration value as an integer
            AsIntegerWithPrefix,    //<! Convert the enumeration value as an integer but with a prefix
            VerbatimString,         //<! Use a provided string verbatim
            Empty,                  //<! An empty value
            NoOp,                   //<! Do nothing with this value
            PassThrough             //<! Pass the value unchanged
        }

        /// <summary>
        /// Create an instance
        /// </summary>
        /// <param name="key">The value of the enumeration</param>
        /// <param name="property">Name of the property</param>
        /// <param name="mode">Conversion mode</param>
        /// <param name="inheritExisting">Optional, true if the value inherits from parents. Default is false.</param>
        /// <param name="verbatimString">Optional, use this verbatim string. Default is null.</param>
        /// <param name="prefix">Optional, use this integer prefix. Default is null.</param>
        /// <param name="target">Optional, target to write settings to. Default is settings.</param>
        public EnumAttribute(
            object key,
            string property,
            EMode mode,
            bool inheritExisting = false,
            string verbatimString = null,
            string prefix = null,
            TargetGroup target = TargetGroup.Settings)
            :
            base(property, inheritExisting, target)
        {
            this.Key = key as System.Enum;
            this.Mode = mode;
            this.VerbatimString = verbatimString;
            this.Prefix = prefix;
        }

        /// <summary>
        /// Get the enumeration value.
        /// </summary>
        public System.Enum Key { get; private set; }

        /// <summary>
        /// Get the conversion mode.
        /// </summary>
        public EMode Mode { get; private set; }

        /// <summary>
        /// Get the verbatim string.
        /// </summary>
        public string VerbatimString { get; private set; }

        /// <summary>
        /// Get the prefix
        /// </summary>
        public string Prefix { get; private set; }
    }
}
