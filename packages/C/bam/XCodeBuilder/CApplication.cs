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
    public sealed class XcodeLinker :
        ILinkerPolicy
    {
        void
        ILinkerPolicy.Link(
            ConsoleApplication sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.TokenizedString executablePath,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> objectFiles,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> headers,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> libraries,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> frameworks)
        {
            var linker = sender.Settings as C.V2.ICommonLinkerOptions;
            // TODO: could the lib search paths be in the staticlibrary base class as a patch?
            var configName = sender.BuildEnvironment.Configuration.ToString();
            var macros = new Bam.Core.MacroList();
            macros.Add("moduleoutputdir", Bam.Core.TokenizedString.Create(configName, null));
            foreach (var library in libraries)
            {
                if (library is C.V2.StaticLibrary)
                {
                    var fullLibraryPath = library.GeneratedPaths[C.V2.StaticLibrary.Key].Parse(macros);
                    var dir = System.IO.Path.GetDirectoryName(fullLibraryPath);
                    linker.LibraryPaths.Add(Bam.Core.TokenizedString.Create(dir, null));
                }
                else if (library is C.V2.DynamicLibrary)
                {
                    var fullLibraryPath = library.GeneratedPaths[C.V2.DynamicLibrary.Key].Parse(macros);
                    var dir = System.IO.Path.GetDirectoryName(fullLibraryPath);
                    linker.LibraryPaths.Add(Bam.Core.TokenizedString.Create(dir, null));
                }
                else if (library is C.V2.CSDKModule)
                {
                    // SDK modules are collections of libraries, not one in particular
                    // thus do nothing as they are undefined at this point, and may yet be pulled in automatically
                }
                else if (library is C.V2.HeaderLibrary)
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

            XcodeBuilder.V2.XcodeCommonLinkable application;
            // TODO: this is a hack, so that modules earlier in the graph can add pre/post build commands
            // to the project for this module
            if (null == sender.MetaData)
            {
                if (sender is DynamicLibrary)
                {
                    application = new XcodeBuilder.V2.XcodeDynamicLibrary(sender, executablePath);
                }
                else
                {
                    application = new XcodeBuilder.V2.XcodeProgram(sender, executablePath);
                }
            }
            else
            {
                application = sender.MetaData as XcodeBuilder.V2.XcodeCommonLinkable;
            }

            var interfaceType = Bam.Core.State.ScriptAssembly.GetType("XcodeProjectProcessor.V2.IConvertToProject");
            if (interfaceType.IsAssignableFrom(sender.Settings.GetType()))
            {
                var map = sender.Settings.GetType().GetInterfaceMap(interfaceType);
                map.InterfaceMethods[0].Invoke(sender.Settings, new object[] { sender, application.Configuration });
            }

            if (objectFiles.Count > 1)
            {
                var xcodeConvertParameterTypes = new Bam.Core.TypeArray
                {
                    typeof(Bam.Core.Module),
                    typeof(XcodeBuilder.V2.Configuration)
                };

                var sharedSettings = C.V2.SettingsBase.SharedSettings(
                    objectFiles,
                    typeof(Clang.XcodeImplementation),
                    typeof(XcodeProjectProcessor.V2.IConvertToProject),
                    xcodeConvertParameterTypes);
                application.SetCommonCompilationOptions(null, sharedSettings);

                foreach (var objFile in objectFiles)
                {
                    var deltaSettings = (objFile.Settings as C.V2.SettingsBase).CreateDeltaSettings(sharedSettings, objFile);
                    var meta = objFile.MetaData as XcodeBuilder.V2.XcodeObjectFile;
                    application.AddSource(objFile, meta.Source, meta.Output, deltaSettings);
                    meta.Project = application.Project;
                }
            }
            else
            {
                application.SetCommonCompilationOptions(null, objectFiles[0].Settings);
                foreach (var objFile in objectFiles)
                {
                    var meta = objFile.MetaData as XcodeBuilder.V2.XcodeObjectFile;
                    application.AddSource(objFile, meta.Source, meta.Output, null);
                    meta.Project = application.Project;
                }
            }

            foreach (var header in headers)
            {
                var headerMod = header as HeaderFile;
                var headerFileRef = application.Project.FindOrCreateFileReference(
                    headerMod.InputPath,
                    XcodeBuilder.V2.FileReference.EFileType.HeaderFile,
                    sourceTree:XcodeBuilder.V2.FileReference.ESourceTree.Absolute);
                application.AddHeader(headerFileRef);
            }

            foreach (var library in libraries)
            {
                if (library is C.V2.StaticLibrary)
                {
                    application.AddStaticLibrary(library.MetaData as XcodeBuilder.V2.XcodeStaticLibrary);
                }
                else if (library is C.V2.DynamicLibrary)
                {
                    application.AddDynamicLibrary(library.MetaData as XcodeBuilder.V2.XcodeDynamicLibrary);
                }
                else if (library is C.V2.CSDKModule)
                {
                    // do nothing, just an area for external
                }
                else if (library is C.V2.HeaderLibrary)
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
}
