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
namespace Publisher
{
    public sealed class XcodeCollatedObject :
        ICollatedObjectPolicy
    {
        void
        ICollatedObjectPolicy.Collate(
            CollatedObject sender,
            Bam.Core.ExecutionContext context)
        {
            var sourcePath = sender.SourcePath;
            if (null == sender.Reference)
            {
                // no copy is needed, but as we're copying other files relative to this, record where they have to go
                // therefore ignore any subdirectory on this module

                // make an app bundle if required
                if ((sender.SubDirectory != null) && sender.SubDirectory.Parse().Contains(".app/"))
                {
                    var meta = sender.SourceModule.MetaData as XcodeBuilder.XcodeMeta;
                    meta.Target.MakeApplicationBundle();

                    // this has to be the path that Xcode writes to
                    sender.GeneratedPaths[CollatedObject.CopiedObjectKey].Aliased(sender.CreateTokenizedString("$(packagebuilddir)/$(config)/$(0)/@filename($(1))", sender.SubDirectory, sourcePath));
                }
                else
                {
                    // this has to be the path that Xcode writes to
                    sender.GeneratedPaths[CollatedObject.CopiedObjectKey].Aliased(sender.CreateTokenizedString("$(packagebuilddir)/$(config)/@filename($(0))", sourcePath));
                }

                return;
            }

            if ((null != sender.Reference) && (null != sender.SourceModule) &&
                (sender.SourceModule.PackageDefinition == sender.Reference.PackageDefinition))
            {
                // same package has the same output folder, so don't bother copying
                // TODO: does the destination directory need to be set?
                return;
            }

            var destinationPath = sender.Macros["CopyDir"].Parse();

            var commandLine = new Bam.Core.StringArray();
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(sender, commandLine);

            if (sender.SourceModule != null && sender.SourceModule.MetaData != null)
            {
                var commands = new Bam.Core.StringArray();
                commands.Add(System.String.Format("[[ ! -d {0} ]] && mkdir -p {0}", destinationPath));
                commands.Add(System.String.Format("{0} {1} $CONFIGURATION_BUILD_DIR/$EXECUTABLE_NAME {2}",
                    (sender.Tool as Bam.Core.ICommandLineTool).Executable,
                    commandLine.ToString(' '),
                    destinationPath));
                (sender.SourceModule.MetaData as XcodeBuilder.XcodeCommonProject).AddPostBuildCommands(commands);
            }
            else
            {
                var isSymlink = (sender is CollatedSymbolicLink);
                var commands = new Bam.Core.StringArray();

                var destinationFolder = "$CONFIGURATION_BUILD_DIR";
                if (sender.Reference != null)
                {
                    destinationFolder = "$CONFIGURATION_BUILD_DIR/$EXECUTABLE_FOLDER_PATH";
                    if (isSymlink)
                    {
                        commands.Add(System.String.Format("[[ ! -d {0} ]] && mkdir -p {0}",
                            sender.CreateTokenizedString("@dir($CONFIGURATION_BUILD_DIR/$EXECUTABLE_FOLDER_PATH/$(0))",
                                sender.CreateTokenizedString("$(0)/@filename($(1))", sender.SubDirectory, sender.SourcePath)).Parse()));
                    }
                    else
                    {
                        commands.Add(System.String.Format("[[ ! -d {0} ]] && mkdir -p {0}",
                            sender.CreateTokenizedString("$CONFIGURATION_BUILD_DIR/$EXECUTABLE_FOLDER_PATH/$(0)",
                                sender.SubDirectory).Parse()));
                    }
                }

                commands.Add(System.String.Format("{0} {1} {2} {3}/{4}{5}",
                    (sender.Tool as Bam.Core.ICommandLineTool).Executable,
                    commandLine.ToString(' '),
                    isSymlink ? sender.Macros["LinkTarget"].Parse() : sourcePath.Parse(),
                    destinationFolder,
                    isSymlink ? sender.CreateTokenizedString("$(0)/@filename($(1))", sender.SubDirectory, sender.SourcePath).Parse() : sender.SubDirectory.Parse(),
                    isSymlink ? string.Empty : "/"));
                (sender.Reference.SourceModule.MetaData as XcodeBuilder.XcodeCommonProject).AddPostBuildCommands(commands);
            }
        }
    }
}
