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
namespace VSSolutionBuilder
{
    public sealed class VSSettingsGroup
    {
        public enum ESettingsGroup
        {
            Compiler,
            Header,
            Librarian,
            Linker,
            PreBuild,
            PostBuild,
            CustomBuild,
            Resource
        }

        public VSSettingsGroup(
            ESettingsGroup group,
            Bam.Core.TokenizedString include = null)
        {
            this.Group = group;
            this.Include = include;
            this.Settings = new Bam.Core.Array<VSSetting>();
        }

        public ESettingsGroup Group
        {
            get;
            private set;
        }

        public Bam.Core.TokenizedString Include
        {
            get;
            private set;
        }

        private Bam.Core.Array<VSSetting> Settings
        {
            get;
            set;
        }

        public void
        AddSetting(
            string name,
            bool value,
            string condition = null)
        {
            var stringValue = value.ToString().ToLower();
            if (this.Settings.Any(item => item.Name == name && item.Condition == condition && item.Value != stringValue))
            {
                throw new Bam.Core.Exception("Cannot change the value of existing boolean option {0} to {1}", name, value);
            }

            this.Settings.AddUnique(new VSSetting(name, stringValue, condition));
        }

        public void
        AddSetting(
            string name,
            string value,
            string condition = null)
        {
            if (this.Settings.Any(item => item.Name == name && item.Condition == condition && item.Value != value))
            {
                throw new Bam.Core.Exception("Cannot change the value of existing string option {0} to {1}", name, value);
            }

            this.Settings.AddUnique(new VSSetting(name, value, condition));
        }

        public void
        AddSetting(
            string name,
            Bam.Core.TokenizedString path,
            string condition = null,
            bool inheritExisting = false)
        {
            var stringValue = path.Parse();
            if (this.Settings.Any(item => item.Name == name && item.Condition == condition && item.Value != stringValue))
            {
                throw new Bam.Core.Exception("Cannot change the value of existing tokenized path option {0} to {1}", name, stringValue);
            }

            this.Settings.AddUnique(new VSSetting(name, stringValue, condition));
        }

        public void
        AddSetting(
            string name,
            Bam.Core.TokenizedStringArray value,
            string condition = null,
            bool inheritExisting = false)
        {
            if (0 == value.Count)
            {
                return;
            }
            if (this.Settings.Any(item => item.Name == name && item.Condition == condition))
            {
                throw new Bam.Core.Exception("Cannot append to the option {0}", name);
            }

            var linearized = value.ToString(';');
            this.Settings.AddUnique(new VSSetting(name, inheritExisting ? System.String.Format("{0};%({1})", linearized, name) : linearized, condition));
        }

        public void
        AddSetting(
            string name,
            Bam.Core.StringArray value,
            string condition = null,
            bool inheritExisting = false)
        {
            if (0 == value.Count)
            {
                return;
            }
            if (this.Settings.Any(item => item.Name == name && item.Condition == condition))
            {
                throw new Bam.Core.Exception("Cannot append to the option {0}", name);
            }

            var linearized = value.ToString(';');
            this.Settings.AddUnique(new VSSetting(name, inheritExisting ? System.String.Format("{0};%({1})", linearized, name) : linearized, condition));
        }

        public void
        AddSetting(
            string name,
            C.PreprocessorDefinitions definitions,
            string condition = null,
            bool inheritExisting = false)
        {
            if (this.Settings.Any(item => item.Name == name && item.Condition == condition))
            {
                throw new Bam.Core.Exception("Cannot append to the preprocessor define list {0}", name);
            }

            var linearized = definitions.ToString();
            this.Settings.AddUnique(new VSSetting(name, inheritExisting ? System.String.Format("{0}%({1})", linearized, name) : linearized, condition));
        }

        private string
        GetGroupName()
        {
            switch (this.Group)
            {
                case ESettingsGroup.Compiler:
                    return "ClCompile";

                case ESettingsGroup.Header:
                    return "ClInclude";

                case ESettingsGroup.Librarian:
                    return "Lib";

                case ESettingsGroup.Linker:
                    return "Link";

                case ESettingsGroup.PreBuild:
                    return "PreBuildEvent";

                case ESettingsGroup.PostBuild:
                    return "PostBuildEvent";

                case ESettingsGroup.CustomBuild:
                    return "CustomBuild";

                case ESettingsGroup.Resource:
                    return "ResourceCompile";

                default:
                    throw new Bam.Core.Exception("Unknown settings group, {0}", this.Group.ToString());
            }
        }

        public void
        Serialize(
            System.Xml.XmlDocument document,
            System.Xml.XmlElement parentEl)
        {
            if ((this.Settings.Count == 0) && (this.Include == null))
            {
                return;
            }
            var group = document.CreateVSElement(this.GetGroupName(), parentEl: parentEl);
            if (null != this.Include)
            {
                group.SetAttribute("Include", this.Include.Parse());
            }
            foreach (var setting in this.Settings.OrderBy(pair => pair.Name))
            {
                document.CreateVSElement(setting.Name, value: setting.Value, condition: setting.Condition, parentEl: group);
            }
        }
    }
}
