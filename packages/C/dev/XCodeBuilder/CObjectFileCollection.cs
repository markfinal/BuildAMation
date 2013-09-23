// <copyright file="ObjectFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed partial class XcodeBuilder
    {
        public object Build(C.ObjectFileCollectionBase moduleToBuild, out bool success)
        {
            var node = moduleToBuild.OwningNode;
            var moduleName = node.ModuleName;
            var target = node.Target;
            var baseTarget = (Opus.Core.BaseTarget)target;

            // fill out the build configuration on behalf of all of it's children
            var buildConfiguration = this.Project.BuildConfigurations.Get(baseTarget.ConfigurationName('='), moduleName);

            if (node.EncapsulatingNode.Module is Opus.Core.ICommonOptionCollection)
            {
                var commonOptions = (node.EncapsulatingNode.Module as Opus.Core.ICommonOptionCollection).CommonOptionCollection;
                XcodeProjectProcessor.ToXcodeProject.Execute(commonOptions, buildConfiguration, target);
            }
            else
            {
                XcodeProjectProcessor.ToXcodeProject.Execute(moduleToBuild.Options, buildConfiguration, target);
            }
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

            Opus.Core.Log.MessageAll("Common options for {0}", moduleName);
            foreach (var o in buildConfiguration.Options)
            {
                Opus.Core.Log.MessageAll("  {0} {1}", o.Key, o.Value);
            }

            success = true;
            return null;
        }
    }
}
