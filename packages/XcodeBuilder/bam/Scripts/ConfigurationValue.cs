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
namespace XcodeBuilder
{
    /// <summary>
    /// Base class to all Configuration values.
    /// </summary>
    public abstract class ConfigurationValue
    {
        private static char[] SpecialChars = { '$', '@', '=', '+', ' ' };

        /// <summary>
        /// Determine if the provided string needs quoting.
        /// </summary>
        /// <param name="input">The string to check.</param>
        /// <returns>true if the string needs quoting.</returns>
        protected bool
        StringRequiresQuoting(
            string input) => (-1 != input.IndexOfAny(SpecialChars));

        /// <summary>
        /// Merge new values into the current state.
        /// </summary>
        /// <param name="value">The new value to merge in.</param>
        /// <returns>True if the merge occurred. False if something went wrong.</returns>
        public abstract bool
        Merge(
            ConfigurationValue value);
    }

    /// <summary>
    /// Class corresponding to a value with just a single value.
    /// </summary>
    public sealed class UniqueConfigurationValue :
        ConfigurationValue
    {
        /// <summary>
        /// Construct an instance
        /// </summary>
        /// <param name="value">Value to hold</param>
        public UniqueConfigurationValue(
            string value) => this.Value = value;

        /// <summary>
        /// Merge values.
        /// </summary>
        /// <param name="value">New value to merge in.</param>
        ///

        /// <summary>
        /// Merge new values into the current state.
        /// Since this is a single value, any existing value is replaced - false is retured in this case.
        /// </summary>
        /// <param name="value">The new value to merge in.</param>
        /// <returns>True if the merge occurred. False if something went wrong.</returns>
        public override bool
        Merge(
            ConfigurationValue value)
        {
            var newValue = (value as UniqueConfigurationValue).Value;
            if (this.Value.Equals(newValue, System.StringComparison.Ordinal))
            {
                return true;
            }
            this.Value = (value as UniqueConfigurationValue).Value;
            return false;
        }

        private string Value { get; set; }

        /// <summary>
        /// Convert the value into human readable text.
        /// </summary>
        /// <returns>The string conversion.</returns>
        public override string
        ToString()
        {
            if (System.String.IsNullOrEmpty(this.Value))
            {
                return null;
            }
            if (StringRequiresQuoting(this.Value))
            {
                return $"\"{this.Value}\"";
            }
            return this.Value;
        }
    }

    /// <summary>
    /// Class corresponding to a value with multiple entries.
    /// </summary>
    public sealed class MultiConfigurationValue :
        ConfigurationValue
    {
        /// <summary>
        /// Construct an instance, starting from empty.
        /// </summary>
        public MultiConfigurationValue() => this.Value = new Bam.Core.StringArray();

        /// <summary>
        /// Construct an instance, starting with a single value.
        /// </summary>
        /// <param name="value">Value to hold.</param>
        public MultiConfigurationValue(
            string value)
            :
            this()
        {
            this.Value.AddUnique(value);
        }

        /// <summary>
        /// Merge new values into the current state.
        /// </summary>
        /// <param name="value">The new value to merge in.</param>
        /// <returns>Always true</returns>
        public override bool
        Merge(
            ConfigurationValue value)
        {
            this.Value.AddRangeUnique((value as MultiConfigurationValue).Value);
            return true;
        }

        private Bam.Core.StringArray Value { get; set; }

        /// <summary>
        /// Add a new value in.
        /// </summary>
        /// <param name="value"></param>
        public void
        Add(
            string value) => this.Value.AddUnique(value);

        /// <summary>
        /// Convert the value into a human readable string.
        /// </summary>
        /// <returns>The converted string.</returns>
        public override string
        ToString()
        {
            if (!this.Value.Any())
            {
                return null;
            }
            var value = new System.Text.StringBuilder();
            value.Append("(");
            foreach (var item in this.Value)
            {
                if (System.String.IsNullOrEmpty(item))
                {
                    // to avoid pbxproj values such as ", ," which will not parse
                    continue;
                }
                if (StringRequiresQuoting(item))
                {
                    value.Append($"\"{item}\", ");
                }
                else
                {
                    value.Append($"{item}, ");
                }
            }
            value.Append(")");
            return value.ToString();
        }
    }
}
