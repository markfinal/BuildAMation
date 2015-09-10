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
    public sealed class PBXProject :
        XcodeNodeData,
        IWriteableNode
    {
        public
        PBXProject(
            Bam.Core.DependencyNode node) : base(node.ModuleName)
        {
#if false
            var package = node.Package;
            var projectFilename = "project.pbxproj";
            var rootDirectory = System.IO.Path.Combine(Bam.Core.State.BuildRoot, package.FullName);
            rootDirectory = System.IO.Path.Combine(rootDirectory, node.ModuleName) + ".xcodeproj";
            this.RootUri = new System.Uri(rootDirectory, System.UriKind.Absolute);
            this.Path = System.IO.Path.Combine(rootDirectory, projectFilename);

            var toolset = node.Target.Toolset;
            var xcodeDetails = toolset as IXcodeDetails;
            if (null != xcodeDetails)
            {
                this.LatestXcodeVersion = xcodeDetails.SupportedVersion;
            }
            else
            {
                this.LatestXcodeVersion = EXcodeVersion.V4dot6;
            }

            this.NativeTargets = new PBXNativeTargetSection();
            this.FileReferences = new PBXFileReferenceSection();
            this.BuildFiles = new PBXBuildFileSection();
            this.BuildConfigurations = new XCBuildConfigurationSection();
            this.ConfigurationLists = new XCConfigurationListSection();
            this.Groups = new PBXGroupSection();
            this.SourceBuildPhases = new PBXSourcesBuildPhaseSection();
            this.CopyFilesBuildPhases = new PBXCopyFilesBuildPhaseSection();
            this.FrameworksBuildPhases = new PBXFrameworksBuildPhaseSection();
            this.TargetDependencies = new PBXTargetDependencySection();
            this.ContainerItemProxies = new PBXContainerItemProxySection();
            this.ShellScriptBuildPhases = new PBXShellScriptBuildPhaseSection();

            this.InitializeGroups();
#endif
        }

        private void
        InitializeGroups()
        {
            // create a main group
            var mainGroup = this.Groups.Get(string.Empty);
            mainGroup.SourceTree = "<group>";
            this.MainGroup = mainGroup;

            // create a products group
            var productsGroup = this.Groups.Get("Products");
            productsGroup.SourceTree = "<group>";
            this.ProductsGroup = productsGroup;

            mainGroup.Children.Add(productsGroup);

            // create common build configurations for all targets
            // these settings are overriden by per-target build configurations
            var projectConfigurationList = this.ConfigurationLists.Get(this);
            this.BuildConfigurationList = projectConfigurationList;
            foreach (var config in Bam.Core.State.BuildConfigurations)
            {
                var genericBuildConfiguration = this.BuildConfigurations.Get(config.ToString(), "Generic");
                genericBuildConfiguration.Options["SYMROOT"].AddUnique(Bam.Core.State.BuildRoot);
                if (config == Bam.Core.EConfiguration.Debug)
                {
                    // Xcode 5 wants this setting for build performance in debug
                    genericBuildConfiguration.Options["ONLY_ACTIVE_ARCH"].AddUnique("YES");
                }
                projectConfigurationList.AddUnique(genericBuildConfiguration);
            }
        }

        public System.Uri RootUri
        {
            get;
            private set;
        }

        public string Path
        {
            get;
            private set;
        }

        private EXcodeVersion LatestXcodeVersion
        {
            get;
            set;
        }

        public PBXNativeTargetSection NativeTargets
        {
            get;
            private set;
        }

        public PBXFileReferenceSection FileReferences
        {
            get;
            private set;
        }

        public PBXBuildFileSection BuildFiles
        {
            get;
            private set;
        }

        public XCBuildConfigurationSection BuildConfigurations
        {
            get;
            private set;
        }

        public XCConfigurationListSection ConfigurationLists
        {
            get;
            private set;
        }

        public XCConfigurationList BuildConfigurationList
        {
            get;
            set;
        }

        public PBXGroupSection Groups
        {
            get;
            private set;
        }

        public PBXSourcesBuildPhaseSection SourceBuildPhases
        {
            get;
            private set;
        }

        public PBXCopyFilesBuildPhaseSection CopyFilesBuildPhases
        {
            get;
            private set;
        }

        public PBXFrameworksBuildPhaseSection FrameworksBuildPhases
        {
            get;
            private set;
        }

        public PBXTargetDependencySection TargetDependencies
        {
            get;
            private set;
        }

        public PBXContainerItemProxySection ContainerItemProxies
        {
            get;
            private set;
        }

        public PBXShellScriptBuildPhaseSection ShellScriptBuildPhases
        {
            get;
            private set;
        }

        public PBXGroup MainGroup
        {
            get;
            set;
        }

        public PBXGroup ProductsGroup
        {
            get;
            set;
        }

        private string
        GetLastUpgradeCheckVersion()
        {
            switch (this.LatestXcodeVersion)
            {
            case EXcodeVersion.V4dot6:
                return "0460";

            case EXcodeVersion.V5dot1:
                return "0510";

            case EXcodeVersion.V6dot0:
                return "0600";
            }

            throw new Bam.Core.Exception("Unrecognized Xcode version, {0}", this.LatestXcodeVersion.ToString());
        }

#region IWriteableNode implementation

        void
        IWriteableNode.Write(
            System.IO.TextWriter writer)
        {
            if (null == this.BuildConfigurationList)
            {
                throw new Bam.Core.Exception("Project build configuration list not assigned");
            }

            writer.WriteLine("// !$*UTF8*$!");
            writer.WriteLine("{");
            writer.WriteLine("\tarchiveVersion = 1;");
            writer.WriteLine("\tclasses = {");
            writer.WriteLine("\t};");
            writer.WriteLine("\tobjectVersion = 46;");
            writer.WriteLine("\tobjects = {");

            (this.BuildFiles as IWriteableNode).Write(writer);
            (this.ShellScriptBuildPhases as IWriteableNode).Write(writer);
            (this.ContainerItemProxies as IWriteableNode).Write(writer);
            (this.CopyFilesBuildPhases as IWriteableNode).Write(writer);
            (this.FileReferences as IWriteableNode).Write(writer);
            (this.FrameworksBuildPhases as IWriteableNode).Write(writer);
            (this.Groups as IWriteableNode).Write(writer);
            (this.NativeTargets as IWriteableNode).Write(writer);

            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXProject section */");
            writer.WriteLine("\t\t{0} /* Project object */ = {{", this.UUID);
            writer.WriteLine("\t\t\tisa = PBXProject;");
            writer.WriteLine("\t\t\tattributes = {");
            writer.WriteLine("\t\t\t\tLastUpgradeCheck = {0};", this.GetLastUpgradeCheckVersion());
            writer.WriteLine("\t\t\t\tORGANIZATIONNAME = \"Mark Final\";");
            writer.WriteLine("\t\t\t};");
            writer.WriteLine("\t\t\tbuildConfigurationList = {0} /* Build configuration list for PBXProject \"{1}\" */;", this.BuildConfigurationList.UUID, this.Name);
            writer.WriteLine("\t\t\tcompatibilityVersion = \"Xcode 3.2\";");
            writer.WriteLine("\t\t\tdevelopmentRegion = English;");
            writer.WriteLine("\t\t\thasScannedForEncodings = 0;");
            writer.WriteLine("\t\t\tknownRegions = (");
            writer.WriteLine("\t\t\t\ten,");
            writer.WriteLine("\t\t\t);");
            writer.WriteLine("\t\t\tmainGroup = {0};", this.MainGroup.UUID);
            writer.WriteLine("\t\t\tproductRefGroup = {0} /* {1} */;", this.ProductsGroup.UUID, this.ProductsGroup.Name);
            writer.WriteLine("\t\t\tprojectDirPath = \"\";");
            writer.WriteLine("\t\t\tprojectRoot = \"\";");
            writer.WriteLine("\t\t\ttargets = (");
            foreach (PBXNativeTarget target in this.NativeTargets)
            {
                writer.WriteLine("\t\t\t\t{0} /* {1} */,", target.UUID, target.Name);
            }
            writer.WriteLine("\t\t\t);");
            writer.WriteLine("\t\t};");
            writer.WriteLine("/* End PBXProject section */");

            (this.SourceBuildPhases as IWriteableNode).Write(writer);
            (this.TargetDependencies as IWriteableNode).Write(writer);
            (this.BuildConfigurations as IWriteableNode).Write(writer);
            (this.ConfigurationLists as IWriteableNode).Write(writer);

            writer.WriteLine("\t};");
            writer.WriteLine("\trootObject = {0} /* Project object */;", this.UUID);
            writer.WriteLine("}");
        }
#endregion
    }
}
