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
    public sealed class XcodeLinker :
        ILinkingPolicy
    {
        void
        ILinkingPolicy.Link(
            ConsoleApplication sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.TokenizedString executablePath,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> objectFiles,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> headers,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> libraries,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> frameworks)
        {
            var linker = sender.Settings as C.ICommonLinkerSettings;
            // TODO: could the lib search paths be in the staticlibrary base class as a patch?
            var configName = sender.BuildEnvironment.Configuration.ToString();
            var macros = new Bam.Core.MacroList();
            macros.Add("moduleoutputdir", Bam.Core.TokenizedString.Create(configName, null));
            foreach (var library in libraries)
            {
                if (library is C.StaticLibrary)
                {
                    var fullLibraryPath = library.GeneratedPaths[C.StaticLibrary.Key].Parse(macros);
                    var dir = System.IO.Path.GetDirectoryName(fullLibraryPath);
                    linker.LibraryPaths.Add(Bam.Core.TokenizedString.Create(dir, null));
                }
                else if (library is C.IDynamicLibrary)
                {
                    var fullLibraryPath = library.GeneratedPaths[C.DynamicLibrary.Key].Parse(macros);
                    var dir = System.IO.Path.GetDirectoryName(fullLibraryPath);
                    linker.LibraryPaths.Add(Bam.Core.TokenizedString.Create(dir, null));
                }
                else if (library is C.CSDKModule)
                {
                    // SDK modules are collections of libraries, not one in particular
                    // thus do nothing as they are undefined at this point, and may yet be pulled in automatically
                }
                else if (library is C.HeaderLibrary)
                {
                    // no library
                }
                else if (library is ExternalFramework)
                {
                    // frameworks are dealt with elsewhere
                }
                else
                {
                    throw new Bam.Core.Exception("Don't know how to handle this module type, {0}", library.ToString());
                }
            }

            XcodeBuilder.XcodeCommonLinkable application;
            // TODO: this is a hack, so that modules earlier in the graph can add pre/post build commands
            // to the project for this module
            if (null == sender.MetaData)
            {
                if (sender is IDynamicLibrary)
                {
                    application = new XcodeBuilder.XcodeDynamicLibrary(sender, executablePath);
                }
                else
                {
                    application = new XcodeBuilder.XcodeProgram(sender, executablePath);
                }
            }
            else
            {
                application = sender.MetaData as XcodeBuilder.XcodeCommonLinkable;
            }

            // convert link settings to the Xcode project
            (sender.Settings as XcodeProjectProcessor.IConvertToProject).Convert(sender, application.Configuration);

            if (objectFiles.Count > 1)
            {
                var xcodeConvertParameterTypes = new Bam.Core.TypeArray
                {
                    typeof(Bam.Core.Module),
                    typeof(XcodeBuilder.Configuration)
                };

                var sharedSettings = C.SettingsBase.SharedSettings(
                    objectFiles,
                    typeof(ClangCommon.XcodeImplementation),
                    typeof(XcodeProjectProcessor.IConvertToProject),
                    xcodeConvertParameterTypes);
                application.SetCommonCompilationOptions(null, sharedSettings);

                foreach (var objFile in objectFiles)
                {
                    var deltaSettings = (objFile.Settings as C.SettingsBase).CreateDeltaSettings(sharedSettings, objFile);
                    var meta = objFile.MetaData as XcodeBuilder.XcodeObjectFile;
                    application.AddSource(objFile, meta.Source, meta.Output, deltaSettings);
                    meta.Project = application.Project;
                }
            }
            else
            {
                application.SetCommonCompilationOptions(null, objectFiles[0].Settings);
                foreach (var objFile in objectFiles)
                {
                    var meta = objFile.MetaData as XcodeBuilder.XcodeObjectFile;
                    application.AddSource(objFile, meta.Source, meta.Output, null);
                    meta.Project = application.Project;
                }
            }

            foreach (var header in headers)
            {
                var headerMod = header as HeaderFile;
                var headerFileRef = application.Project.FindOrCreateFileReference(
                    headerMod.InputPath,
                    XcodeBuilder.FileReference.EFileType.HeaderFile,
                    sourceTree:XcodeBuilder.FileReference.ESourceTree.Absolute);
                application.AddHeader(headerFileRef);
            }

            foreach (var library in libraries)
            {
                if (library is C.StaticLibrary)
                {
                    application.AddStaticLibrary(library.MetaData as XcodeBuilder.XcodeStaticLibrary);
                }
                else if (library is C.IDynamicLibrary)
                {
                    application.AddDynamicLibrary(library.MetaData as XcodeBuilder.XcodeDynamicLibrary);
                }
                else if (library is C.CSDKModule)
                {
                    // do nothing, just an area for external
                }
                else if (library is C.HeaderLibrary)
                {
                    // no library
                }
                else if (library is ExternalFramework)
                {
                    // frameworks are dealt with elsewhere
                }
                else
                {
                    throw new Bam.Core.Exception("Don't know how to handle this module type");
                }
            }
        }
    }
}
