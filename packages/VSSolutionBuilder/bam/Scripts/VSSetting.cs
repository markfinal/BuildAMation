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
using System.Linq;
namespace VSSolutionBuilder
{
    /// <summary>
    /// Class representing an individual setting within a group
    /// </summary>
    sealed class VSSetting
    {
        /// <summary>
        /// Construct a new setting.
        /// </summary>
        /// <param name="name"><Name of the setting.</param>
        /// <param name="value">Value of the setting.</param>
        /// <param name="isPath">Whether this setting represents a path.</param>
        /// <param name="inheritValue">Whether this setting inherits from a parent.</param>
        /// <param name="condition">Optional condition expression to which this setting requires. Defaults to null.</param>
        public VSSetting(
            string name,
            string value,
            bool isPath,
            bool inheritValue,
            string condition = null)
        {
            this.Name = name;
            this.Value = value;
            this.Condition = condition;
            this.IsPath = isPath;
            this.InheritValue = inheritValue;
        }

        /// <summary>
        /// Name of the setting.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Value of the setting.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Condition for the setting.
        /// </summary>
        public string Condition { get; private set; }

        /// <summary>
        /// Whether this setting represents a path.
        /// </summary>
        public bool IsPath { get; private set; }

        private bool InheritValue { get; set; }

        /// <summary>
        /// Serialize the setting to a string.
        /// </summary>
        /// <returns>String representation</returns>
        public string
        Serialize()
        {
            if (this.InheritValue)
            {
                return new string($"{this.Value};%({this.Name})");
            }
            return this.Value;
        }

        /// <summary>
        /// Append an additional value to the setting.
        /// </summary>
        /// <param name="toAppend">String to append.</param>
        /// <param name="separator">Optional separator to use. Default is the empty string</param>
        public void Append(
            string toAppend,
            string separator = "")
        {
            this.Value = new string($"{this.Value}{separator}{toAppend}");
        }
    }
}
