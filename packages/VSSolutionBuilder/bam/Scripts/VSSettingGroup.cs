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
                this.RelativeDirectory = module.CreateTokenizedString("@trimstart(@relativeto(@dir($(0)),$(packagedir)),../)", include);
                lock (this.RelativeDirectory)
                {
                    if (!this.RelativeDirectory.IsParsed)
                    {
                        // may have been parsed already, e.g. a common header
                        this.RelativeDirectory.Parse();
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

        public void
        AddSetting(
            string name,
            bool value,
            string condition = null)
        {
            lock (this.Settings)
            {
                var stringValue = value.ToString().ToLower();
                if (this.Settings.Any(item => item.Name == name && item.Condition == condition && item.Value != stringValue))
                {
                    throw new Bam.Core.Exception("Cannot change the value of existing boolean option {0} to {1}", name, value);
                }

                this.Settings.AddUnique(new VSSetting(name, stringValue, condition));
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
                if (this.Settings.Any(item => item.Name == name && item.Condition == condition && item.Value != value))
                {
                    throw new Bam.Core.Exception("Cannot change the value of existing string option {0} to {1}", name, value);
                }

                this.Settings.AddUnique(new VSSetting(name, value, condition));
            }
        }

        private string
        toRelativePath(
            Bam.Core.TokenizedString path)
        {
            var programFiles = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles);
            var programFilesX86 = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86);

            var pathString = path.ToString();
            if (pathString.StartsWith(programFiles) || pathString.StartsWith(programFilesX86))
            {
                return pathString;
            }

            var contatenated = new System.Text.StringBuilder();
            var relative = Bam.Core.RelativePathUtilities.GetPath(pathString, this.Project.ProjectPath);
            if (!Bam.Core.RelativePathUtilities.IsPathAbsolute(relative))
            {
                contatenated.Append("$(ProjectDir)");
            }
            contatenated.AppendFormat("{0}", relative);
            return contatenated.ToString();
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
                var stringValue = isPath ? toRelativePath(path) : path.ToString();
                if (this.Settings.Any(item => item.Name == name && item.Condition == condition && item.Value != stringValue))
                {
                    throw new Bam.Core.Exception("Cannot change the value of existing tokenized path option {0} to {1}", name, path.ToString());
                }

                this.Settings.AddUnique(new VSSetting(name, stringValue, condition));
            }
        }

        private string
        toRelativePaths(
            Bam.Core.TokenizedStringArray paths)
        {
            return toRelativePaths(paths.ToEnumerableWithoutDuplicates());
        }

        private string
        toRelativePaths(
            System.Collections.Generic.IEnumerable<Bam.Core.TokenizedString> paths)
        {
            var programFiles = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles);
            var programFilesX86 = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86);

            var contatenated = new System.Text.StringBuilder();
            foreach (var path in paths.Distinct())
            {
                var pathString = path.ToString();
                if (pathString.StartsWith(programFiles) || pathString.StartsWith(programFilesX86))
                {
                    contatenated.AppendFormat("{0};", pathString);
                    continue;
                }
                var relative = Bam.Core.RelativePathUtilities.GetPath(pathString, this.Project.ProjectPath);
                if (!Bam.Core.RelativePathUtilities.IsPathAbsolute(relative))
                {
                    contatenated.Append("$(ProjectDir)");
                }
                contatenated.AppendFormat("{0};", relative);
            }
            return contatenated.ToString();
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
                var linearized = arePaths ? this.toRelativePaths(value) : new Bam.Core.TokenizedStringArray(value.Distinct()).ToString(';');
                if (this.Settings.Any(item => item.Name == name && item.Condition == condition))
                {
                    var settingOption = this.Settings.First(item => item.Name == name && item.Condition == condition);
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

                this.Settings.AddUnique(new VSSetting(name, inheritExisting ? System.String.Format("{0};%({1})", linearized, name) : linearized, condition));
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
                if (this.Settings.Any(item => item.Name == name && item.Condition == condition))
                {
                    var settingOption = this.Settings.First(item => item.Name == name && item.Condition == condition);
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

                this.Settings.AddUnique(new VSSetting(name, inheritExisting ? System.String.Format("{0};%({1})", linearized, name) : linearized, condition));
            }
        }

        public void
        AddSetting(
            string name,
            C.PreprocessorDefinitions definitions,
            string condition = null,
            bool inheritExisting = false)
        {
            lock (this.Settings)
            {
                if (this.Settings.Any(item => item.Name == name && item.Condition == condition))
                {
                    throw new Bam.Core.Exception("Cannot append to the preprocessor define list {0}", name);
                }

                var defString = definitions.ToString();
                this.Settings.AddUnique(new VSSetting(name, inheritExisting ? System.String.Format("{0}%({1})", defString, name) : defString, condition));
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
                var path = this.Include.ToString();
                var relPath = Bam.Core.RelativePathUtilities.GetPath(path, this.Project.ProjectPath);
                if (Bam.Core.RelativePathUtilities.IsPathAbsolute(relPath))
                {
                    group.SetAttribute("Include", relPath);
                }
                else
                {
                    group.SetAttribute("Include", System.String.Format("$(ProjectDir){0}", relPath));
                }
            }
            foreach (var setting in this.Settings.OrderBy(pair => pair.Name))
            {
                document.CreateVSElement(setting.Name, value: setting.Value, condition: setting.Condition, parentEl: group);
            }
        }
    }
}
