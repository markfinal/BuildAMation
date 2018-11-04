#region License
// Copyright (c) 2010-2018, Mark Final
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
            Resource,
            Assembler
        }

        public VSSettingsGroup(
            VSProject project,
            Bam.Core.Module module,
            ESettingsGroup group,
            Bam.Core.TokenizedString include = null)
        {
            this.Project = project;
            this.Module = module;
            this.Group = group;
            this.Include = include;
            if (null != include)
            {
                this.RelativeDirectory = module.CreateTokenizedString(
                    "@isrelative(@trimstart(@relativeto(@dir($(0)),$(packagedir)),../),@dir($(0)))",
                    include
                );
                lock (this.RelativeDirectory)
                {
                    if (!this.RelativeDirectory.IsParsed)
                    {
                        // may have been parsed already, e.g. a common header
                        this.RelativeDirectory.Parse();
                    }

                    // this can happen if the source file lies directly in the package directory
                    // rather than in subdirectories
                    // project filters called '.' look weird
                    if (this.RelativeDirectory.ToString().Equals(".", System.StringComparison.Ordinal))
                    {
                        this.RelativeDirectory = null;
                    }
                }
            }
            this.Settings = new Bam.Core.Array<VSSetting>();
        }

        private VSProject Project
        {
            get;
            set;
        }

        public Bam.Core.Module Module
        {
            get;
            private set;
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

        public Bam.Core.TokenizedString RelativeDirectory
        {
            get;
            private set;
        }

        private Bam.Core.Array<VSSetting> Settings
        {
            get;
            set;
        }

        private VSProjectConfiguration Configuration
        {
            get
            {
                return this.Project.GetConfiguration(this.Module);
            }
        }

        public void
        AddSetting(
            string name,
            bool value,
            string condition = null)
        {
            lock (this.Settings)
            {
                var stringValue = value.ToString().ToLower();
                if (this.Settings.Any(item => item.Name.Equals(name, System.StringComparison.Ordinal) && System.String.Equals(item.Condition, condition, System.StringComparison.Ordinal) && !item.Value.Equals(stringValue, System.StringComparison.Ordinal)))
                {
                    throw new Bam.Core.Exception("Cannot change the value of existing boolean option {0} to {1}", name, value);
                }

                this.Settings.AddUnique(
                    new VSSetting(
                        name,
                        stringValue,
                        false,
                        condition: condition
                    )
                );
            }
        }

        public void
        AddSetting(
            string name,
            string value,
            string condition = null)
        {
            lock (this.Settings)
            {
                if (this.Settings.Any(item => item.Name.Equals(name, System.StringComparison.Ordinal) && item.Condition.Equals(condition, System.StringComparison.Ordinal) && !item.Value.Equals(value, System.StringComparison.Ordinal)))
                {
                    throw new Bam.Core.Exception("Cannot change the value of existing string option {0} to {1}", name, value);
                }

                this.Settings.AddUnique(
                    new VSSetting(
                        name,
                        value,
                        false,
                        condition: condition
                    )
                );
            }
        }

        public void
        AddSetting(
            string name,
            Bam.Core.TokenizedString path,
            string condition = null,
            bool inheritExisting = false,
            bool isPath = false)
        {
            lock (this.Settings)
            {
                var stringValue = path.ToString();
                if (this.Settings.Any(item => item.Name.Equals(name, System.StringComparison.Ordinal) && item.Condition.Equals(condition, System.StringComparison.Ordinal) && !item.Value.Equals(stringValue, System.StringComparison.Ordinal)))
                {
                    throw new Bam.Core.Exception("Cannot change the value of existing tokenized path option {0} to {1}", name, path.ToString());
                }

                this.Settings.AddUnique(
                    new VSSetting(
                        name,
                        stringValue,
                        isPath: isPath,
                        condition: condition
                    )
                );
            }
        }

        public void
        AddSetting(
            string name,
            Bam.Core.TokenizedStringArray value,
            string condition = null,
            bool inheritExisting = false,
            bool arePaths = false)
        {
            this.AddSetting(
                name,
                value.ToEnumerableWithoutDuplicates(),
                condition,
                inheritExisting,
                arePaths
            );
        }

        public void
        AddSetting(
            string name,
            System.Collections.Generic.IEnumerable<Bam.Core.TokenizedString> value,
            string condition = null,
            bool inheritExisting = false,
            bool arePaths = false)
        {
            lock (this.Settings)
            {
                if (!value.Any())
                {
                    return;
                }
                var linearized = new Bam.Core.TokenizedStringArray(value.Distinct()).ToString(';');
                if (this.Settings.Any(item => item.Name.Equals(name, System.StringComparison.Ordinal) && item.Condition.Equals(condition, System.StringComparison.Ordinal)))
                {
                    var settingOption = this.Settings.First(item => item.Name.Equals(name, System.StringComparison.Ordinal) && item.Condition.Equals(condition, System.StringComparison.Ordinal));
                    if (settingOption.Value.Contains(linearized))
                    {
                        return;
                    }
                    throw new Bam.Core.Exception("Cannot append {3}, to the option {0} as it already exists for condition {1}: {2}",
                        name,
                        condition,
                        settingOption.Value.ToString(),
                        linearized);
                }

                this.Settings.AddUnique(
                    new VSSetting(
                        name,
                        inheritExisting ? System.String.Format("{0};%({1})", linearized, name) : linearized,
                        arePaths,
                        condition
                    )
                );
            }
        }

        public void
        AddSetting(
            string name,
            Bam.Core.StringArray value,
            string condition = null,
            bool inheritExisting = false)
        {
            lock (this.Settings)
            {
                if (0 == value.Count)
                {
                    return;
                }
                var linearized = value.ToString(';');
                if (this.Settings.Any(item => item.Name.Equals(name, System.StringComparison.Ordinal) && item.Condition.Equals(condition, System.StringComparison.Ordinal)))
                {
                    var settingOption = this.Settings.First(item => item.Name.Equals(name, System.StringComparison.Ordinal) && item.Condition.Equals(condition, System.StringComparison.Ordinal));
                    if (settingOption.Value.Contains(linearized))
                    {
                        return;
                    }
                    throw new Bam.Core.Exception("Cannot append {3}, to the option {0} as it already exists for condition {1}: {2}",
                        name,
                        condition,
                        settingOption.Value.ToString(),
                        linearized);
                }

                this.Settings.AddUnique(
                    new VSSetting(
                        name,
                        inheritExisting ? System.String.Format("{0};%({1})", linearized, name) : linearized,
                        false,
                        condition
                    )
                );
            }
        }

        public void
        AddSetting(
            string name,
            C.PreprocessorDefinitions definitions,
            string condition = null,
            bool inheritExisting = false)
        {
            if (!definitions.Any())
            {
                return;
            }
            lock (this.Settings)
            {
                if (this.Settings.Any(item => item.Name.Equals(name) && item.Condition.Equals(condition, System.StringComparison.Ordinal)))
                {
                    throw new Bam.Core.Exception("Cannot append to the preprocessor define list {0}", name);
                }

                var defString = definitions.ToString();
                this.Settings.AddUnique(
                    new VSSetting(
                        name,
                        inheritExisting ? System.String.Format("{0}%({1})", defString, name) : defString,
                        false,
                        condition
                    )
                );
            }
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

                case ESettingsGroup.Assembler:
                    return "MASM";

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
                var rel_path = this.Configuration.ToRelativePath(this.Include);
                group.SetAttribute("Include", rel_path);
            }
            foreach (var setting in this.Settings.OrderBy(pair => pair.Name))
            {
                if (setting.IsPath)
                {
                    document.CreateVSElement(
                        setting.Name,
                        value: this.Configuration.ToRelativePath(setting.Value),
                        condition: setting.Condition,
                        parentEl: group
                    );
                }
                else
                {
                    document.CreateVSElement(
                        setting.Name,
                        value: setting.Value,
                        condition: setting.Condition,
                        parentEl: group
                    );
                }
            }
        }
    }
}
