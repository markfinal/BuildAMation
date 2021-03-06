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
    /// Class corresponding to a XCConfigurationList in an Xcode project.
    /// </summary>
    sealed class ConfigurationList :
        Object,
        System.Collections.Generic.IEnumerable<Configuration>
    {
        /// <summary>
        /// Construct an instance of the object
        /// </summary>
        /// <param name="parent">The parent Object</param>
        public ConfigurationList(
            Object parent)
            :
            base(parent.Project, null, "XCConfigurationList", parent.GUID)
        {
            this.Parent = parent;
            this.Configurations = new Bam.Core.Array<Configuration>();
        }

        /// <summary>
        /// Get the Configuration by index.
        /// </summary>
        /// <param name="index">A valid index into the list of Configurations.</param>
        /// <returns>The Configuration</returns>
        public Configuration this[int index] => this.Configurations[index];

        /// <summary>
        /// Get the parent Object.
        /// </summary>
        public Object Parent { get; private set; }

        private Bam.Core.Array<Configuration> Configurations { get; set; }

        /// <summary>
        /// Add a Configuration to the list.
        /// </summary>
        /// <param name="config">Configuration to add</param>
        public void
        AddConfiguration(
            Configuration config)
        {
            lock (this.Configurations)
            {
                var existingConfig = this.Configurations.FirstOrDefault(item => item.GUID.Equals(config.GUID, System.StringComparison.Ordinal));
                if (null != existingConfig)
                {
                    return;
                }
                this.Configurations.Add(config);
            }
        }

        /// <summary>
        /// Serialize the ConfigurationList.
        /// </summary>
        /// <param name="text">StringBuilder to write to.</param>
        /// <param name="indentLevel">Number of tabs to indent by.</param>
        public override void
        Serialize(
            System.Text.StringBuilder text,
            int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            var indent2 = new string('\t', indentLevel + 1);
            var indent3 = new string('\t', indentLevel + 2);
            text.AppendLine($"{indent}{this.GUID} /* Build configuration list for {this.Parent.IsA} \"{this.Parent.Name}\" */ = {{");
            text.AppendLine($"{indent2}isa = {this.IsA};");
            if (this.Configurations.Any())
            {
                text.AppendLine($"{indent2}buildConfigurations = (");
                foreach (var config in this.Configurations)
                {
                    text.AppendLine($"{indent3}{config.GUID} /* {config.Name} */,");
                }
                text.AppendLine($"{indent2});");
            }
            text.AppendLine($"{indent}}};");
        }

        /// <summary>
        /// Allow enumeration across the list.
        /// </summary>
        /// <returns>Each Configuration in turn.</returns>
        public System.Collections.Generic.IEnumerator<Configuration>
        GetEnumerator()
        {
            foreach (var config in this.Configurations)
            {
                yield return config;
            }
        }

        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
