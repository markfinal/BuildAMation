// <copyright file="CObjectFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed partial class XcodeBuilder
    {
        public object Build(C.ObjectFile moduleToBuild, out bool success)
        {
            var node = moduleToBuild.OwningNode;
            var moduleName = node.ModuleName;
            var target = node.Target;
            var baseTarget = (Opus.Core.BaseTarget)target;

            Opus.Core.Log.MessageAll("ObjectFile {0}", moduleName);
            var sourceFile = moduleToBuild.SourceFile.AbsolutePath;
            var fileRef = new PBXFileReference(moduleName, PBXFileReference.EType.SourceFile, sourceFile, this.ProjectRootUri);
            this.Project.FileReferences.Add(fileRef);

            var buildConfiguration = this.Project.BuildConfigurations.Get(baseTarget.ConfigurationName('='), moduleName);
            // TODO: what to do when there are multiple configurations
            if (target.HasPlatform(Opus.Core.EPlatform.OSX64))
            {
                buildConfiguration.Options["ARCHS"] = new Opus.Core.StringArray("\"$(ARCHS_STANDARD_64_BIT)\"");
            }
            else
            {
                buildConfiguration.Options["ARCHS"] = new Opus.Core.StringArray("\"$(ARCHS_STANDARD_32_BIT)\"");
            }
            buildConfiguration.Options["ONLY_ACTIVE_ARCH"] = new Opus.Core.StringArray("YES");
            buildConfiguration.Options["MACOSX_DEPLOYMENT_TARGET"] = new Opus.Core.StringArray("10.8");
            buildConfiguration.Options["SDKROOT"] = new Opus.Core.StringArray("macosx");

            if (target.HasToolsetType(typeof(LLVMGcc.Toolset)))
            {
                if (target.Toolset.Version(baseTarget).StartsWith("4.2"))
                {
                    buildConfiguration.Options["GCC_VERSION"] = new Opus.Core.StringArray("com.apple.compilers.llvmgcc42");
                }
                else
                {
                    throw new Opus.Core.Exception("Not supporting LLVM Gcc version {0}", target.Toolset.Version(baseTarget));
                }
            }
            else
            {
                // clang GCC_VERSION = com.apple.compilers.llvm.clang.1_0
                throw new Opus.Core.Exception("Cannot identify toolchain {0}", target.ToolsetName('='));
            }

            var options = moduleToBuild.Options as C.ICCompilerOptions;
            buildConfiguration.Options["HEADER_SEARCH_PATHS"] = new Opus.Core.StringArray(options.IncludePaths.ToStringArray());

            var data = new PBXBuildFile(moduleName);
            data.FileReference = fileRef;
            this.Project.BuildFiles.Add(data);

            var sourcesBuildPhase = this.Project.SourceBuildPhases.Get("Sources", moduleName);
            sourcesBuildPhase.Files.Add(data);
            data.BuildPhase = sourcesBuildPhase;

            success = true;
            return data;
        }
    }
}
