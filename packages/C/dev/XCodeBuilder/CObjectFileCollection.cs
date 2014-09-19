#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
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
            var baseTarget = (Bam.Core.BaseTarget)target;

            var project = this.Workspace.GetProject(node);

            Bam.Core.BaseOptionCollection commonOptions = null;
            if (node.EncapsulatingNode.Module is Bam.Core.ICommonOptionCollection)
            {
                commonOptions = (node.EncapsulatingNode.Module as Bam.Core.ICommonOptionCollection).CommonOptionCollection;
                if (null == commonOptions)
                {
                    success = true;
                    return null;
                }
            }

            // fill out the build configuration on behalf of all of it's children
            var buildConfiguration = project.BuildConfigurations.Get(baseTarget.ConfigurationName('='), moduleName);

            var basePath = Bam.Core.State.BuildRoot + System.IO.Path.DirectorySeparatorChar;
            var outputDirLoc = moduleToBuild.Locations[C.ObjectFile.OutputDir];
            var relPath = Bam.Core.RelativePathUtilities.GetPath(outputDirLoc, basePath);
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
            buildConfiguration.Options["MACOSX_DEPLOYMENT_TARGET"].AddUnique("10.8");
            buildConfiguration.Options["SDKROOT"].AddUnique("macosx10.8");
            buildConfiguration.Options["SUPPORTED_PLATFORMS"].AddUnique("macosx");

            if (target.HasToolsetType(typeof(LLVMGcc.Toolset)))
            {
                if (target.Toolset.Version(baseTarget).StartsWith("4.2"))
                {
                    buildConfiguration.Options["GCC_VERSION"].AddUnique("com.apple.compilers.llvmgcc42");
                }
                else
                {
                    throw new Bam.Core.Exception("Not supporting LLVM Gcc version {0}", target.Toolset.Version(baseTarget));
                }
            }
            else if (target.HasToolsetType(typeof(Clang.Toolset)))
            {
                buildConfiguration.Options["GCC_VERSION"].AddUnique("com.apple.compilers.llvm.clang.1_0");
            }
            else
            {
                throw new Bam.Core.Exception("Cannot identify toolchain {0}", target.ToolsetName('='));
            }
#endif

            success = true;
            return null;
        }
    }
}
