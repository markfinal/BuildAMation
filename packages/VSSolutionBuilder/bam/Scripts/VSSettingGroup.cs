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
    /// Class representing a group of settings with a given meaning
    /// </summary>
    sealed class VSSettingsGroup
    {
        /// <summary>
        /// What type of group is represented
        /// </summary>
        public enum ESettingsGroup
        {
            Compiler,   //<! Group of source files to compile
            Header,     //<! Group of header files
            Librarian,  //<! Archive a number of object files
            Linker,     //<! Link a number of object files and libraries
            PreBuild,   //<! Perform a prebuild step
            PostBuild,  //<! Perform a postbuild step
            CustomBuild,//<! Customized build step
            Resource,   //<! Group of Windowsresource files
            Assembler   //<! Group of assembler files
        }

        /// <summary>
        /// Construct a new group instance
        /// </summary>
        /// <param name="project">Belongs to this project</param>
        /// <param name="module">Module associated with the group</param>
        /// <param name="group">Type of the group to make</param>
        /// <param name="path">The path to associate with the new settings group</param>
        public VSSettingsGroup(
            VSProject project,
            Bam.Core.Module module,
            ESettingsGroup group,
            Bam.Core.TokenizedString path)
        {
            this.Project = project;
            this.Module = module;
            this.Group = group;
            this.Path = path;
            if (null != path)
            {
                this.RelativeDirectory = module.CreateTokenizedString(
                    "@isrelative(@trimstart(@relativeto(@dir($(0)),$(packagedir)),../),@dir($(0)))",
                    path
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

        private VSProject Project { get; set; }

        /// <summary>
        /// Module associated with the group
        /// </summary>
        public Bam.Core.Module Module { get; private set; }

        /// <summary>
        /// Type of the group
        /// </summary>
        public ESettingsGroup Group { get; private set; }

        /// <summary>
        /// Path associated with the settings.
        /// </summary>
        public Bam.Core.TokenizedString Path { get; private set; }

        /// <summary>
        /// Relative directory of the group.
        /// </summary>
        public Bam.Core.TokenizedString RelativeDirectory { get; private set; }

        private Bam.Core.Array<VSSetting> Settings { get; set; }

        private VSProjectConfiguration Configuration => this.Project.GetConfiguration(this.Module);

        /// <summary>
        /// Add a new Boolean setting to the group.
        /// </summary>
        /// <param name="name">Name of the setting</param>
        /// <param name="value">Bool value of the setting.</param>
        /// <param name="condition">Optional condition for the setting. Default to null.</param>
        public void
        AddSetting(
            string name,
            bool value,
            string condition = null)
        {
            lock (this.Settings)
            {
                var stringValue = value.ToString().ToLower();
                if (this.Settings.Any(item =>
                        item.Name.Equals(name, System.StringComparison.Ordinal) &&
                        System.String.Equals(item.Condition, condition, System.StringComparison.Ordinal) &&
                        !item.Value.Equals(stringValue, System.StringComparison.Ordinal))
                    )
                {
                    throw new Bam.Core.Exception($"Cannot change the value of existing boolean option {name} to {value}");
                }

                this.Settings.AddUnique(
                    new VSSetting(
                        name,
                        stringValue,
                        false,
                        inheritValue: false,
                        condition: condition
                    )
                );
            }
        }

        /// <summary>
        /// Add a new string setting to the group.
        /// </summary>
        /// <param name="name">Name of the setting</param>
        /// <param name="value">String value of the setting.</param>
        /// <param name="condition">Optional condition for the setting. Default to null.</param>
        public void
        AddSetting(
            string name,
            string value,
            string condition = null)
        {
            lock (this.Settings)
            {
                if (this.Settings.Any(item =>
                        item.Name.Equals(name, System.StringComparison.Ordinal) &&
                        System.String.Equals(item.Condition, condition, System.StringComparison.Ordinal) &&
                        !item.Value.Equals(value, System.StringComparison.Ordinal))
                    )
                {
                    throw new Bam.Core.Exception($"Cannot change the value of existing string option {name} to {value}");
                }

                this.Settings.AddUnique(
                    new VSSetting(
                        name,
                        value,
                        false,
                        inheritValue: false,
                        condition: condition
                    )
                );
            }
        }

        /// <summary>
        /// Add a path setting to the group.
        /// </summary>
        /// <param name="name">Name of the setting</param>
        /// <param name="path">Path of the setting.</param>
        /// <param name="condition">Optional condition for the setting. Default to null.</param>
        /// <param name="inheritExisting">Optional whether the value inherits parent values. Default to false.</param>
        public void
        AddSetting(
            string name,
            Bam.Core.TokenizedString path,
            string condition = null,
            bool inheritExisting = false)
        {
            lock (this.Settings)
            {
                var stringValue = path.ToString();
                if (this.Settings.Any(item =>
                        item.Name.Equals(name, System.StringComparison.Ordinal) &&
                        System.String.Equals(item.Condition, condition, System.StringComparison.Ordinal) &&
                        !item.Value.Equals(stringValue, System.StringComparison.Ordinal))
                    )
                {
                    throw new Bam.Core.Exception($"Cannot change the value of existing tokenized path option {name} to {path.ToString()}");
                }

                this.Settings.AddUnique(
                    new VSSetting(
                        name,
                        stringValue,
                        isPath: true,
                        inheritValue: false,
                        condition: condition
                    )
                );
            }
        }

        /// <summary>
        /// Add an array of strings setting to the group.
        /// </summary>
        /// <param name="name">Name of the setting</param>
        /// <param name="value">Value of the setting.</param>
        /// <param name="condition">Optional condition for the setting. Default to null.</param>
        /// <param name="inheritExisting">Optional whether the value inherits parent values. Default to false.</param>
        /// <param name="arePaths">Optional whether the value is an array of paths. Default to false.</param>
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

        /// <summary>
        /// Add an enumeration of strings setting to the group.
        /// </summary>
        /// <param name="name">Name of the setting</param>
        /// <param name="value">Value of the setting.</param>
        /// <param name="condition">Optional condition for the setting. Default to null.</param>
        /// <param name="inheritExisting">Optional whether the value inherits parent values. Default to false.</param>
        /// <param name="arePaths">Optional whether the value is an enumeration of paths. Default to false.</param>
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
                if (this.Settings.Any(item => item.Name.Equals(name, System.StringComparison.Ordinal) && System.String.Equals(item.Condition, condition, System.StringComparison.Ordinal)))
                {
                    var settingOption = this.Settings.First(item => item.Name.Equals(name, System.StringComparison.Ordinal) && System.String.Equals(item.Condition, condition, System.StringComparison.Ordinal));
                    if (settingOption.Value.Contains(linearized))
                    {
                        return;
                    }
                    settingOption.Append(linearized, separator: ";");
                    return;
                }

                this.Settings.AddUnique(
                    new VSSetting(
                        name,
                        linearized,
                        arePaths,
                        inheritValue: inheritExisting,
                        condition
                    )
                );
            }
        }

        /// <summary>
        /// Add an array of strings setting to the group.
        /// </summary>
        /// <param name="name">Name of the setting</param>
        /// <param name="value">Value of the setting.</param>
        /// <param name="condition">Optional condition for the setting. Default to null.</param>
        /// <param name="inheritExisting">Optional whether the value inherits parent values. Default to false.</param>
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
                if (this.Settings.Any(item => item.Name.Equals(name, System.StringComparison.Ordinal) && System.String.Equals(item.Condition, condition, System.StringComparison.Ordinal)))
                {
                    var settingOption = this.Settings.First(item => item.Name.Equals(name, System.StringComparison.Ordinal) && System.String.Equals(item.Condition, condition, System.StringComparison.Ordinal));
                    if (settingOption.Value.Contains(linearized))
                    {
                        return;
                    }
                    settingOption.Append(linearized, separator: ";");
                    return;
                }

                this.Settings.AddUnique(
                    new VSSetting(
                        name,
                        linearized,
                        false,
                        inheritValue: inheritExisting,
                        condition
                    )
                );
            }
        }

        /// <summary>
        /// Add preprocessor definitions setting to the group.
        /// </summary>
        /// <param name="name">Name of the setting</param>
        /// <param name="definitions">Value of the setting.</param>
        /// <param name="condition">Optional condition for the setting. Default to null.</param>
        /// <param name="inheritExisting">Optional whether the value inherits parent values. Default to false.</param>
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
                if (this.Settings.Any(item => item.Name.Equals(name) && System.String.Equals(item.Condition, condition, System.StringComparison.Ordinal)))
                {
                    throw new Bam.Core.Exception($"Cannot append to the preprocessor define list {name}");
                }

                var defString = definitions.ToString();
                this.Settings.AddUnique(
                    new VSSetting(
                        name,
                        defString,
                        false,
                        inheritValue: inheritExisting,
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
                    throw new Bam.Core.Exception($"Unknown settings group, {this.Group.ToString()}");
            }
        }

        /// <summary>
        /// Serialize the settings group to an XML document.
        /// </summary>
        /// <param name="document">XML document to serialise to.</param>
        /// <param name="parentEl">Parent XML element for this group.</param>
        public void
        Serialize(
            System.Xml.XmlDocument document,
            System.Xml.XmlElement parentEl)
        {
            if ((this.Settings.Count == 0) && (this.Path == null))
            {
                return;
            }
            var group = document.CreateVSElement(this.GetGroupName(), parentEl: parentEl);
            if (null != this.Path)
            {
                // cannot use relative paths with macros here, see https://docs.microsoft.com/en-us/cpp/build/reference/vcxproj-file-structure?view=vs-2015
                group.SetAttribute("Include", this.Path.ToString());
            }
            foreach (var setting in this.Settings.OrderBy(pair => pair.Name))
            {
                if (setting.IsPath)
                {
                    document.CreateVSElement(
                        setting.Name,
                        value: this.Configuration.ToRelativePath(setting.Serialize()),
                        condition: setting.Condition,
                        parentEl: group
                    );
                }
                else
                {
                    document.CreateVSElement(
                        setting.Name,
                        value: setting.Serialize(),
                        condition: setting.Condition,
                        parentEl: group
                    );
                }
            }
        }
    }
}
