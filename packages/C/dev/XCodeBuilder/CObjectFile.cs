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

            // fill out the build configuration
            var buildConfiguration = this.Project.BuildConfigurations.Get(baseTarget.ConfigurationName('='), moduleName);
            XcodeProjectProcessor.ToXcodeProject.Execute(moduleToBuild.Options, buildConfiguration, target);
            // TODO: not sure where all these will come from
#if true
            // TODO: what to do when there are multiple configurations
            if (target.HasPlatform(Opus.Core.EPlatform.OSX64))
            {
                buildConfiguration.Options["ARCHS"].AddUnique("\"$(ARCHS_STANDARD_64_BIT)\"");
            }
            else
            {
                buildConfiguration.Options["ARCHS"].AddUnique("\"$(ARCHS_STANDARD_32_BIT)\"");
            }
            buildConfiguration.Options["ONLY_ACTIVE_ARCH"].AddUnique("YES");
            buildConfiguration.Options["MACOSX_DEPLOYMENT_TARGET"].AddUnique("10.8");
            buildConfiguration.Options["SDKROOT"].AddUnique("macosx");

            if (target.HasToolsetType(typeof(LLVMGcc.Toolset)))
            {
                if (target.Toolset.Version(baseTarget).StartsWith("4.2"))
                {
                    buildConfiguration.Options["GCC_VERSION"].AddUnique("com.apple.compilers.llvmgcc42");
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
#endif
            Opus.Core.Log.MessageAll("Options");
            foreach (var o in buildConfiguration.Options)
            {
                Opus.Core.Log.MessageAll("  {0} {1}", o.Key, o.Value);
            }

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
