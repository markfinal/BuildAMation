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
    public sealed class XcodeLibrarian :
        IArchivingPolicy
    {
        void
        IArchivingPolicy.Archive(
            StaticLibrary sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.TokenizedString libraryPath,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> objectFiles,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> headers)
        {
            if (0 == objectFiles.Count)
            {
                return;
            }

            var workspace = Bam.Core.Graph.Instance.MetaData as XcodeBuilder.WorkspaceMeta;
            var target = workspace.EnsureTargetExists(sender);
            target.EnsureOutputFileReferenceExists(
                sender.CreateTokenizedString("@filename($(0))", libraryPath),
                XcodeBuilder.FileReference.EFileType.Archive,
                XcodeBuilder.Target.EProductType.StaticLibrary);
            var configuration = target.GetConfiguration(sender);
            configuration.SetProductName(Bam.Core.TokenizedString.CreateVerbatim("${TARGET_NAME}"));

            foreach (var header in headers)
            {
                target.EnsureHeaderFileExists((header as HeaderFile).InputPath);
            }

            if (objectFiles.Count > 1)
            {
                var xcodeConvertParameterTypes = new Bam.Core.TypeArray
                {
                    typeof(Bam.Core.Module),
                    typeof(XcodeBuilder.Configuration)
                };

                var sharedSettings = C.SettingsBase.SharedSettings(
                    objectFiles,
                    typeof(ClangCommon.XcodeCompilerImplementation),
                    typeof(XcodeProjectProcessor.IConvertToProject),
                    xcodeConvertParameterTypes);
                (sharedSettings as XcodeProjectProcessor.IConvertToProject).Convert(sender, configuration);

                foreach (var objFile in objectFiles)
                {
                    var buildFile = objFile.MetaData as XcodeBuilder.BuildFile;
                    var deltaSettings = (objFile.Settings as C.SettingsBase).CreateDeltaSettings(sharedSettings, objFile);
                    if (null != deltaSettings)
                    {
                        var commandLine = new Bam.Core.StringArray();
                        (deltaSettings as CommandLineProcessor.IConvertToCommandLine).Convert(sender, commandLine);
                        if (commandLine.Count > 0)
                        {
                            // Cannot set per-file-per-configuration settings, so blend them together
                            if (null == buildFile.Settings)
                            {
                                buildFile.Settings = commandLine;
                            }
                            else
                            {
                                buildFile.Settings.AddRangeUnique(commandLine);
                            }
                        }
                    }
                    configuration.BuildFiles.Add(buildFile);
                }
            }
            else
            {
                (objectFiles[0].Settings as XcodeProjectProcessor.IConvertToProject).Convert(sender, configuration);
                foreach (var objFile in objectFiles)
                {
                    var buildFile = objFile.MetaData as XcodeBuilder.BuildFile;
                    configuration.BuildFiles.Add(buildFile);
                }
            }

            // convert librarian settings to the Xcode project
            if (sender.Settings is XcodeProjectProcessor.IConvertToProject)
            {
                (sender.Settings as XcodeProjectProcessor.IConvertToProject).Convert(sender, configuration);
            }

            // order only dependents
            foreach (var required in sender.Requirements)
            {
                if (null == required.MetaData)
                {
                    continue;
                }

                var requiredTarget = required.MetaData as XcodeBuilder.Target;
                if (null != requiredTarget)
                {
                    target.Requires(requiredTarget);
                }
            }
        }
    }
}
