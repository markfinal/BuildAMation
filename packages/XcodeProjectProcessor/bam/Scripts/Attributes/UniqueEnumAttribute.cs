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
    /// Attribute corresponding to a single value enumeration property.
    /// </summary>
    public sealed class UniqueEnumAttribute :
        EnumAttribute
    {
        /// <summary>
        /// Construct an instance of the attribute.
        /// </summary>
        /// <param name="key">Enum object.</param>
        /// <param name="property">Name of the property.</param>
        /// <param name="value">Value of the property.</param>
        /// <param name="ignore">Optionally, whether the property is ignored.</param>
        public UniqueEnumAttribute(
            object key,
            string property,
            string value,
            bool ignore = false
        )
            :
            base(key, property, value, ValueType.Unique, ignore: ignore)
        {}

        /// <summary>
        /// Construct an instance of the attribute. With a second value.
        /// </summary>
        /// <param name="key">Enum object.</param>
        /// <param name="property">Name of the property.</param>
        /// <param name="value">Value of the property.</param>
        /// <param name="property2">Second property name.</param>
        /// <param name="value2">Second value.</param>
        public UniqueEnumAttribute(
            object key,
            string property,
            string value,
            string property2,
            string value2
        )
            :
            this(key, property, value)
        {
            this.Property2 = property2;
            this.Value2 = value2;
        }

        /// <summary>
        /// Get the second property name.
        /// </summary>
        public string Property2 { get; private set; }

        /// <summary>
        /// Get the second value.
        /// </summary>
        public string Value2 { get; private set; }
    }
}
