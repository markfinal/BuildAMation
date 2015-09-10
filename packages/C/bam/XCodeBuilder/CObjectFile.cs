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
namespace C
{
namespace V2
{
    public sealed class XcodeCompilation :
        ICompilationPolicy
    {
        void
        ICompilationPolicy.Compile(
            ObjectFile sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.TokenizedString objectFilePath,
            Bam.Core.V2.Module source)
        {
            var objectFile = new XcodeBuilder.V2.XcodeObjectFile(sender);
            objectFile.Source = objectFile.Project.FindOrCreateFileReference(
                source.GeneratedPaths[C.V2.SourceFile.Key],
                XcodeBuilder.V2.FileReference.EFileType.SourceCodeC,
                sourceTree:XcodeBuilder.V2.FileReference.ESourceTree.Absolute);
            objectFile.Output = objectFile.Project.FindOrCreateBuildFile(objectFilePath, objectFile.Source);
        }
    }
}
}
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
            }

            // add the source file to the configuration
            var config = project.BuildConfigurations.Get(baseTarget.ConfigurationName('='), moduleName);
            config.SourceFiles.AddUnique(data);

            success = true;
            return data;
        }
    }
}
