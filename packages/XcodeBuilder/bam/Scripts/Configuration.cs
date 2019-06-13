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
    public sealed class Configuration :
        Object
    {
        private readonly System.Collections.Generic.Dictionary<string, ConfigurationValue> Settings = new System.Collections.Generic.Dictionary<string, ConfigurationValue>();

        public Configuration(
            Bam.Core.EConfiguration config,
            Project project,
            Target target)
            :
            base(project, config.ToString(), "XCBuildConfiguration", project.GUID, (target != null) ? target.GUID : string.Empty)
        {
            this.Config = config;
            this.PreBuildCommands = new Bam.Core.StringArray();
            this.PostBuildCommands = new Bam.Core.StringArray();
            this.BuildFiles = new Bam.Core.Array<BuildFile>();
        }

        public Bam.Core.EConfiguration Config { get; private set; }
        private Bam.Core.StringArray PreBuildCommands { get; set; }

        public void
        AppendPreBuildCommands(
            Bam.Core.StringArray commands)
        {
            lock (this.PreBuildCommands)
            {
                this.PreBuildCommands.AddRange(commands);
            }
        }

        private Bam.Core.StringArray PostBuildCommands { get; set; }

        public void
        AppendPostBuildCommands(
            Bam.Core.StringArray commands)
        {
            lock (this.PostBuildCommands)
            {
                this.PostBuildCommands.AddRange(commands);
            }
        }

        public Bam.Core.Array<BuildFile> BuildFiles { get; private set; }

        public ConfigurationValue this[string key]
        {
            get
            {
                return this.Settings[key];
            }

            set
            {
                if (!this.Settings.ContainsKey(key))
                {
                    this.Settings[key] = value;
                }
                else
                {
                    this.Settings[key].Merge(key, value);
                }
            }
        }

        public void
        SetProductName(
            Bam.Core.TokenizedString productName)
        {
            this["PRODUCT_NAME"] = new UniqueConfigurationValue(productName.ToString());
        }

        public override void
        Serialize(
            System.Text.StringBuilder text,
            int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            var indent2 = new string('\t', indentLevel + 1);
            var indent3 = new string('\t', indentLevel + 2);
            text.AppendLine($"{indent}{this.GUID} /* {this.Name} */ = {{");
            text.AppendLine($"{indent2}isa = {this.IsA};");
            text.AppendLine($"{indent2}buildSettings = {{");
            foreach (var setting in this.Settings.OrderBy(key => key.Key))
            {
                var value = setting.Value.ToString();
                if (null == value)
                {
                    continue;
                }
                text.AppendLine($"{indent3}{setting.Key} = {this.CleansePaths(value)};");
            }
            text.AppendLine($"{indent2}}};");
            text.AppendLine($"{indent2}name = {this.Name};");
            text.AppendLine($"{indent}}};");
        }

        private void
        SerializeCommmandList(
            System.Text.StringBuilder text,
            int indentLevel,
            Bam.Core.StringArray commandList)
        {
            var indent = new string(' ', 2 * indentLevel);
            foreach (var line in commandList)
            {
                text.Append($"{indent}{line.Replace("\\", "\\\\").Replace("\"", "\\\"")}\\n");
            }
        }

        public void
        SerializePreBuildCommands(
            System.Text.StringBuilder text,
            int indentLevel)
        {
            this.SerializeCommmandList(text, indentLevel, this.PreBuildCommands);
        }

        public void
        SerializePostBuildCommands(
            System.Text.StringBuilder text,
            int indentLevel)
        {
            this.SerializeCommmandList(text, indentLevel, this.PostBuildCommands);
        }
    }
}
