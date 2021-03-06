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
    /// Base class to all Xcode attribute associated with settings properties.
    /// </summary>
    abstract class BaseAttribute :
        System.Attribute
    {
        /// <summary>
        /// Xcode values can either be a single value or multivalued.
        /// </summary>
        public enum ValueType
        {
            Unique,         //!< Xcode project value has a unique value
            MultiValued     //!< Xcode project value has many values
        }

        /// <summary>
        /// Construct an instance of the base attribute.
        /// </summary>
        /// <param name="property">Name of the property in the Xcode project.</param>
        /// <param name="type">Whether the value is single or multi-valued.</param>
        /// <param name="ignore">This property may be ignored.</param>
        protected BaseAttribute(
            string property,
            ValueType type,
            bool ignore = false)
        {
            this.Property = property;
            this.Type = type;
            this.Ignore = ignore;
        }

        /// <summary>
        /// Get the property name.
        /// </summary>
        public string Property { get; private set; }

        /// <summary>
        /// Get whether the property is singley or multi-valued.
        /// </summary>
        public ValueType Type { get; private set; }

        /// <summary>
        /// Get whether the property should be ignored.
        /// </summary>
        public bool Ignore { get; private set; }
    }
}
