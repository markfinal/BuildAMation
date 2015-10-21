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
namespace XcodeBuilder
{
    public abstract class ConfigurationValue
    {
        public abstract void
        Merge(
            ConfigurationValue value);
    }

    public sealed class UniqueConfigurationValue :
        ConfigurationValue
    {
        public UniqueConfigurationValue(
            string value)
        {
            this.Value = value;
        }

        public override void
        Merge(
            ConfigurationValue value)
        {
            this.Value = (value as UniqueConfigurationValue).Value;
        }

        private string Value
        {
            get;
            set;
        }

        public override string
        ToString()
        {
            return this.Value;
        }
    }

    public sealed class MultiConfigurationValue :
        ConfigurationValue
    {
        public MultiConfigurationValue()
        {
            this.Value = new Bam.Core.StringArray();
        }

        public MultiConfigurationValue(
            string value)
            : this()
        {
            this.Value.AddUnique(value);
        }

        public override void
        Merge(
            ConfigurationValue value)
        {
            this.Value.AddRangeUnique((value as MultiConfigurationValue).Value);
        }

        private Bam.Core.StringArray Value
        {
            get;
            set;
        }

        public void
        Add(
            string value)
        {
            this.Value.AddUnique(value);
        }

        public override string
        ToString()
        {
            return this.Value.ToString(' ');
        }
    }
}
