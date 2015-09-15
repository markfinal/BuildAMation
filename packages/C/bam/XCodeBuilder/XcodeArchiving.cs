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
            XcodeBuilder.XcodeCommonProject library;
            // TODO: this is a hack, so that modules earlier in the graph can add pre/post build commands
            // to the project for this module
            if (null == sender.MetaData)
            {
                library = new XcodeBuilder.XcodeStaticLibrary(sender, libraryPath);
            }
            else
            {
                library = sender.MetaData as XcodeBuilder.XcodeCommonProject;
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
                    typeof(Clang.XcodeImplementation),
                    typeof(XcodeProjectProcessor.IConvertToProject),
                    xcodeConvertParameterTypes);
                library.SetCommonCompilationOptions(null, sharedSettings);

                foreach (var objFile in objectFiles)
                {
                    var deltaSettings = (objFile.Settings as C.SettingsBase).CreateDeltaSettings(sharedSettings, objFile);
                    var meta = objFile.MetaData as XcodeBuilder.XcodeObjectFile;
                    library.AddSource(objFile, meta.Source, meta.Output, deltaSettings);
                    meta.Project = library.Project;
                }
            }
            else
            {
                library.SetCommonCompilationOptions(null, objectFiles[0].Settings);
                foreach (var objFile in objectFiles)
                {
                    var meta = objFile.MetaData as XcodeBuilder.XcodeObjectFile;
                    library.AddSource(objFile, meta.Source, meta.Output, null);
                    meta.Project = library.Project;
                }
            }

            foreach (var header in headers)
            {
                var headerMod = header as HeaderFile;
                var headerFileRef = library.Project.FindOrCreateFileReference(
                    headerMod.InputPath,
                    XcodeBuilder.FileReference.EFileType.HeaderFile,
                    sourceTree:XcodeBuilder.FileReference.ESourceTree.Absolute);
                library.AddHeader(headerFileRef);
            }
        }
    }
}
