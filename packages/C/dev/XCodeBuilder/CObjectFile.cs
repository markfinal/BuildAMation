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

            var sourceFile = moduleToBuild.SourceFile.AbsolutePath;

            var fileRef = this.Project.FileReferences.Get(moduleName, PBXFileReference.EType.SourceFile, sourceFile, this.ProjectRootUri);

            var data = this.Project.BuildFiles.Get(moduleName, fileRef);

            var sourcesBuildPhase = this.Project.SourceBuildPhases.Get("Sources", moduleName);
            sourcesBuildPhase.Files.AddUnique(data);
            data.BuildPhase = sourcesBuildPhase;

            Opus.Core.BaseOptionCollection complementOptionCollection = null;
            if (node.EncapsulatingNode.Module is Opus.Core.ICommonOptionCollection)
            {
                var commonOptions = (node.EncapsulatingNode.Module as Opus.Core.ICommonOptionCollection).CommonOptionCollection;
                if (commonOptions is C.ICCompilerOptions)
                {
                    complementOptionCollection = moduleToBuild.Options.Complement(commonOptions);
                }
            }

            if ((complementOptionCollection != null) && !complementOptionCollection.Empty)
            {
                // there is an option delta to write for this file
                Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
                CommandLineProcessor.ToCommandLine.Execute(complementOptionCollection, commandLineBuilder, target, null);
                Opus.Core.Log.MessageAll("Complement command line: '{0}'", commandLineBuilder.ToString(' '));

                if (commandLineBuilder.Count > 0)
                {
                    data.Settings["COMPILER_FLAGS"].AddRangeUnique(commandLineBuilder);
                }
            }
            else
            {
                // fill out the build configuration for the singular file
                var buildConfiguration = this.Project.BuildConfigurations.Get(baseTarget.ConfigurationName('='), moduleName);
                XcodeProjectProcessor.ToXcodeProject.Execute(moduleToBuild.Options, this.Project, data, buildConfiguration, target);
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
            }

            // add the source file to the configuration
            var config = this.Project.BuildConfigurations.Get(baseTarget.ConfigurationName('='), moduleName);
            config.SourceFiles.AddUnique(data);

            success = true;
            return data;
        }
    }
}
