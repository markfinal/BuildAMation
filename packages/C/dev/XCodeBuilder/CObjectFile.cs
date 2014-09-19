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
            C.ObjectFile moduleToBuild,
            out bool success)
        {
            var node = moduleToBuild.OwningNode;
            var moduleName = node.ModuleName;
            var target = node.Target;
            var baseTarget = (Bam.Core.BaseTarget)target;

            var sourceFile = moduleToBuild.SourceFileLocation;

            var project = this.Workspace.GetProject(node);

            var fileType = PBXFileReference.EType.CSourceFile;
            if (moduleToBuild is C.ObjCxx.ObjectFile)
            {
                fileType = PBXFileReference.EType.ObjCxxSourceFile;
            }
            else if (moduleToBuild is C.ObjC.ObjectFile)
            {
                fileType = PBXFileReference.EType.ObjCSourceFile;
            }
            else if (moduleToBuild is C.Cxx.ObjectFile)
            {
                fileType = PBXFileReference.EType.CxxSourceFile;
            }
            var fileRef = project.FileReferences.Get(moduleName, fileType, sourceFile, project.RootUri);

            var sourcesBuildPhase = project.SourceBuildPhases.Get("Sources", moduleName);
            var data = project.BuildFiles.Get(moduleName, fileRef, sourcesBuildPhase);
            if (null == data)
            {
                throw new Bam.Core.Exception("Build file not available");
            }

            Bam.Core.BaseOptionCollection complementOptionCollection = null;
            if (node.EncapsulatingNode.Module is Bam.Core.ICommonOptionCollection)
            {
                var commonOptions = (node.EncapsulatingNode.Module as Bam.Core.ICommonOptionCollection).CommonOptionCollection;
                if (commonOptions is C.ICCompilerOptions)
                {
                    complementOptionCollection = moduleToBuild.Options.Complement(commonOptions);
                }
            }

            if ((complementOptionCollection != null) && !complementOptionCollection.Empty)
            {
                // there is an option delta to write for this file
                var commandLineBuilder = new Bam.Core.StringArray();
                CommandLineProcessor.ToCommandLine.Execute(complementOptionCollection, commandLineBuilder, target, null);

                if (commandLineBuilder.Count > 0)
                {
                    var compilerFlags = data.Settings["COMPILER_FLAGS"];

                    // need to escape any quotes again, otherwise the quotes are lost in the command lines
                    for (int index = 0; index < commandLineBuilder.Count; ++index)
                    {
                        var arg = commandLineBuilder[index];
                        if (arg.Contains("\""))
                        {
                            arg = arg.Replace("\"", "\\\\\"");
                            compilerFlags.AddUnique(arg);
                        }
                        else
                        {
                            compilerFlags.AddUnique(arg);
                        }
                    }
                }
            }
            else
            {
                // fill out the build configuration for the singular file
                var buildConfiguration = project.BuildConfigurations.Get(baseTarget.ConfigurationName('='), moduleName);
                XcodeProjectProcessor.ToXcodeProject.Execute(moduleToBuild.Options, project, data, buildConfiguration, target);

                var basePath = Bam.Core.State.BuildRoot + System.IO.Path.DirectorySeparatorChar;
                var outputDirLoc = moduleToBuild.Locations[C.ObjectFile.OutputDir];
                var relPath = Bam.Core.RelativePathUtilities.GetPath(outputDirLoc, basePath);
                buildConfiguration.Options["CONFIGURATION_TEMP_DIR"].AddUnique("$SYMROOT/" + relPath);
                buildConfiguration.Options["TARGET_TEMP_DIR"].AddUnique("$CONFIGURATION_TEMP_DIR");

                // TODO: these should really be options in their own rights
#if true
                buildConfiguration.Options["MACOSX_DEPLOYMENT_TARGET"].AddUnique("10.8");
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
            }

            // add the source file to the configuration
            var config = project.BuildConfigurations.Get(baseTarget.ConfigurationName('='), moduleName);
            config.SourceFiles.AddUnique(data);

            success = true;
            return data;
        }
    }
}
