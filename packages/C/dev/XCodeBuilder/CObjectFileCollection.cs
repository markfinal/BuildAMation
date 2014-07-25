// <copyright file="ObjectFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed partial class XcodeBuilder
    {
        public object
        Build(
            C.ObjectFileCollectionBase moduleToBuild,
            out bool success)
        {
            var node = moduleToBuild.OwningNode;
            var moduleName = node.ModuleName;
            var target = node.Target;
            var baseTarget = (Opus.Core.BaseTarget)target;

            var project = this.Workspace.GetProject(node);

            Opus.Core.BaseOptionCollection commonOptions = null;
            if (node.EncapsulatingNode.Module is Opus.Core.ICommonOptionCollection)
            {
                commonOptions = (node.EncapsulatingNode.Module as Opus.Core.ICommonOptionCollection).CommonOptionCollection;
                if (null == commonOptions)
                {
                    success = true;
                    return null;
                }
            }

            // fill out the build configuration on behalf of all of it's children
            var buildConfiguration = project.BuildConfigurations.Get(baseTarget.ConfigurationName('='), moduleName);

            var basePath = Opus.Core.State.BuildRoot + System.IO.Path.DirectorySeparatorChar;
            var outputDirLoc = moduleToBuild.Locations[C.ObjectFile.OutputDir];
            var relPath = Opus.Core.RelativePathUtilities.GetPath(outputDirLoc, basePath);
            buildConfiguration.Options["CONFIGURATION_TEMP_DIR"].AddUnique("$SYMROOT/" + relPath);
            buildConfiguration.Options["TARGET_TEMP_DIR"].AddUnique("$CONFIGURATION_TEMP_DIR");

            if (commonOptions != null)
            {
                XcodeProjectProcessor.ToXcodeProject.Execute(commonOptions, project, null, buildConfiguration, target);
            }
            else
            {
                XcodeProjectProcessor.ToXcodeProject.Execute(moduleToBuild.Options, project, null, buildConfiguration, target);
            }

            // TODO: these should really be options in their own rights
#if true
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
            else if (target.HasToolsetType(typeof(Clang.Toolset)))
            {
                buildConfiguration.Options["GCC_VERSION"].AddUnique("com.apple.compilers.llvm.clang.1_0");
            }
            else
            {
                throw new Opus.Core.Exception("Cannot identify toolchain {0}", target.ToolsetName('='));
            }
#endif

            success = true;
            return null;
        }
    }
}
