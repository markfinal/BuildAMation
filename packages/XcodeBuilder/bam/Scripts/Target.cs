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
    public sealed class Target :
        Object
    {
        public enum EProductType
        {
            StaticLibrary,
            Executable,
            DynamicLibrary,
            ApplicationBundle
        }

        public Target(
            Bam.Core.Module module,
            Project project,
            FileReference fileRef,
            EProductType type)
        {
            this.IsA = "PBXNativeTarget";
            this.Name = module.GetType().Name;
            this.FileReference = fileRef;
            this.Type = type;

            var configList = new ConfigurationList(this);
            project.ConfigurationLists.Add(configList);
            this.ConfigurationList = configList;

            this.BuildPhases = new System.Collections.Generic.List<BuildPhase>();
            this.SourcesBuildPhase = new SourcesBuildPhase();
            this.BuildPhases.Add(this.SourcesBuildPhase);

            this.TargetDependencies = new Bam.Core.Array<TargetDependency>();

            this.Project = project;
            this.Project.SourcesBuildPhases.Add(this.SourcesBuildPhase);
        }

        public SourcesBuildPhase SourcesBuildPhase
        {
            get;
            private set;
        }

        public FrameworksBuildPhase FrameworksBuildPhase
        {
            get;
            set;
        }

        public ShellScriptBuildPhase PreBuildBuildPhase
        {
            get;
            set;
        }

        public ShellScriptBuildPhase PostBuildBuildPhase
        {
            get;
            set;
        }

        public ConfigurationList ConfigurationList
        {
            get;
            private set;
        }

        public FileReference FileReference
        {
            get;
            private set;
        }

        public EProductType Type
        {
            get;
            private set;
        }

        public Project Project
        {
            get;
            set;
        }

        public System.Collections.Generic.List<BuildPhase> BuildPhases
        {
            get;
            private set;
        }

        public Bam.Core.Array<TargetDependency> TargetDependencies
        {
            get;
            private set;
        }

        public void
        SetCommonCompilationOptions(
            Bam.Core.Module module,
            Configuration configuration,
            Bam.Core.Settings settings)
        {
            (settings as XcodeProjectProcessor.IConvertToProject).Convert(module, configuration);
        }

        private string
        ProductTypeToString()
        {
            switch (this.Type)
            {
                case EProductType.StaticLibrary:
                    return "com.apple.product-type.library.static";

                case EProductType.Executable:
                    return "com.apple.product-type.tool";

                case EProductType.DynamicLibrary:
                    return "com.apple.product-type.library.dynamic";

                case EProductType.ApplicationBundle:
                    return "com.apple.product-type.application";
            }

            throw new Bam.Core.Exception("Unrecognized product type");
        }

        public void
        MakeApplicationBundle()
        {
            if (this.Type == EProductType.ApplicationBundle)
            {
                return;
            }
            if (this.Type != EProductType.Executable)
            {
                throw new Bam.Core.Exception("Can only change an executable to an application bundle");
            }
            this.Type = EProductType.ApplicationBundle;
            this.FileReference.MakeApplicationBundle();
        }

        public override void Serialize(System.Text.StringBuilder text, int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            var indent2 = new string('\t', indentLevel + 1);
            var indent3 = new string('\t', indentLevel + 2);
            text.AppendFormat("{0}{1} /* {2} */ = {{", indent, this.GUID, this.Name);
            text.AppendLine();
            text.AppendFormat("{0}isa = {1};", indent2, this.IsA);
            text.AppendLine();
            text.AppendFormat("{0}buildConfigurationList = {1} /* Build configuration list for {2} \"{3}\" */;", indent2, this.ConfigurationList.GUID, this.ConfigurationList.Parent.IsA, this.ConfigurationList.Parent.Name);
            text.AppendLine();
            if (this.BuildPhases.Count > 0)
            {
                text.AppendFormat("{0}buildPhases = (", indent2);
                text.AppendLine();
                System.Action<BuildPhase> dumpPhase = (phase) =>
                {
                    text.AppendFormat("{0}{1} /* {2} */,", indent3, phase.GUID, phase.Name);
                    text.AppendLine();
                };
                if (null != this.PreBuildBuildPhase)
                {
                    dumpPhase(this.PreBuildBuildPhase);
                }
                foreach (var phase in this.BuildPhases)
                {
                    dumpPhase(phase);
                }
                if (null != this.PostBuildBuildPhase)
                {
                    dumpPhase(this.PostBuildBuildPhase);
                }
                text.AppendFormat("{0});", indent2);
                text.AppendLine();
            }
            text.AppendFormat("{0}buildRules = (", indent2);
            text.AppendLine();
            text.AppendFormat("{0});", indent2);
            text.AppendLine();
            text.AppendFormat("{0}dependencies = (", indent2);
            text.AppendLine();
            foreach (var dependency in this.TargetDependencies)
            {
                text.AppendFormat("{0}{1} /* {2} */,", indent3, dependency.GUID, dependency.Name);
                text.AppendLine();
            }
            text.AppendFormat("{0});", indent2);
            text.AppendLine();
            text.AppendFormat("{0}name = {1};", indent2, this.Name);
            text.AppendLine();
            text.AppendFormat("{0}productName = {1};", indent2, this.Name);
            text.AppendLine();
            text.AppendFormat("{0}productReference = {1} /* {2} */;", indent2, this.FileReference.GUID, this.FileReference.Name);
            text.AppendLine();
            text.AppendFormat("{0}productType = \"{1}\";", indent2, this.ProductTypeToString());
            text.AppendLine();
            text.AppendFormat("{0}}};", indent);
            text.AppendLine();
        }
    }
}
