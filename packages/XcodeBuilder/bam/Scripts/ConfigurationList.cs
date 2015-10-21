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
using System.Linq;
namespace XcodeBuilder
{
    public sealed class ConfigurationList :
        Object,
        System.Collections.Generic.IEnumerable<Configuration>
    {
        public ConfigurationList(
            Object parent)
        {
            this.IsA = "XCConfigurationList";
            this.Parent = parent;
            this.Configurations = new System.Collections.Generic.List<Configuration>();
        }

        public Configuration this[int index]
        {
            get
            {
                return this.Configurations[index];
            }
        }

        public Object Parent
        {
            get;
            private set;
        }

        private System.Collections.Generic.List<Configuration> Configurations
        {
            get;
            set;
        }

        public void
        AddConfiguration(
            Configuration config)
        {
            var existingConfig = this.Configurations.Where(item => item.GUID == config.GUID).FirstOrDefault();
            if (null != existingConfig)
            {
                return;
            }
            this.Configurations.Add(config);
        }

        public override void
        Serialize(
            System.Text.StringBuilder text,
            int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            var indent2 = new string('\t', indentLevel + 1);
            var indent3 = new string('\t', indentLevel + 2);
            text.AppendFormat("{0}{1} /* Build configuration list for {2} \"{3}\" */ = {{", indent, this.GUID, this.Parent.IsA, this.Parent.Name);
            text.AppendLine();
            text.AppendFormat("{0}isa = {1};", indent2, this.IsA);
            text.AppendLine();
            if (this.Configurations.Count > 0)
            {
                text.AppendFormat("{0}buildConfigurations = (", indent2);
                text.AppendLine();
                foreach (var config in this.Configurations)
                {
                    text.AppendFormat("{0}{1} /* {2} */,", indent3, config.GUID, config.Name);
                    text.AppendLine();
                }
                text.AppendFormat("{0});", indent2);
                text.AppendLine();
            }
            text.AppendFormat("{0}}};", indent);
            text.AppendLine();
        }

        public System.Collections.Generic.IEnumerator<Configuration>
        GetEnumerator()
        {
            foreach (var config in this.Configurations)
            {
                yield return config;
            }
        }

        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
