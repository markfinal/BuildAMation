#region License
// Copyright (c) 2010-2018, Mark Final
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
using System.Linq;
namespace C
{
#if BAM_V2
    public static partial class XcodeSupport
    {
        public static void
        LinkOrArchive(
            out XcodeBuilder.Target outTarget,
            out XcodeBuilder.Configuration outConfiguration,
            CModule module,
            XcodeBuilder.FileReference.EFileType fileType,
            XcodeBuilder.Target.EProductType productType,
            Bam.Core.TokenizedString productName,
            System.Collections.Generic.IEnumerable<Bam.Core.Module> objectFiles,
            System.Collections.Generic.IEnumerable<Bam.Core.Module> headerFiles)
        {
            if (!objectFiles.Any())
            {
                outTarget = null;
                outConfiguration = null;
                return;
            }

            var workspace = Bam.Core.Graph.Instance.MetaData as XcodeBuilder.WorkspaceMeta;
            var target = workspace.EnsureTargetExists(module);
            var output_filename = module.CreateTokenizedString(
                "@filename($(0))",
                module.GeneratedPaths[(module.Settings as C.ICommonHasOutputPath).OutputPath]
            );
            output_filename.Parse();
            target.EnsureOutputFileReferenceExists(
                output_filename,
                fileType,
                productType
            );
            var configuration = target.GetConfiguration(module);
            lock (productName)
            {
                if (!productName.IsParsed)
                {
                    productName.Parse();
                }
            }
            configuration.SetProductName(productName);

            foreach (var header in headerFiles)
            {
                target.EnsureHeaderFileExists((header as HeaderFile).InputPath);
            }

            var excludedSource = new XcodeBuilder.MultiConfigurationValue();
            var realObjectFiles = objectFiles.Where(item => item is ObjectFile); // C,C++,ObjC,ObjC++
            if (realObjectFiles.Any())
            {
                var sharedSettings = C.SettingsBase.SharedSettings(
                    realObjectFiles
                );
                XcodeSharedSettings.Tweak(sharedSettings);
                XcodeProjectProcessor.XcodeConversion.Convert(
                    sharedSettings,
                    realObjectFiles.First().Settings.GetType(),
                    module,
                    configuration
                );

                foreach (var objFile in realObjectFiles)
                {
                    var asObjFileBase = objFile as C.ObjectFileBase;
                    if (!asObjFileBase.PerformCompilation)
                    {
                        var fullPath = asObjFileBase.InputPath.ToString();
                        var filename = System.IO.Path.GetFileName(fullPath);
                        excludedSource.Add(filename);
                    }

                    var buildFile = objFile.MetaData as XcodeBuilder.BuildFile;
                    var deltaSettings = (objFile.Settings as C.SettingsBase).CreateDeltaSettings(sharedSettings, objFile);
                    if (null != deltaSettings)
                    {
                        var commandLine = CommandLineProcessor.NativeConversion.Convert(deltaSettings, module);
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

                // now deal with other object file types
                var assembledObjectFiles = objectFiles.Where(item => item is AssembledObjectFile);
                foreach (var asmObj in assembledObjectFiles)
                {
                    var buildFile = asmObj.MetaData as XcodeBuilder.BuildFile;
                    configuration.BuildFiles.Add(buildFile);
                }
            }
            else
            {
                XcodeProjectProcessor.XcodeConversion.Convert(
                    objectFiles.First().Settings,
                    objectFiles.First().Settings.GetType(),
                    module,
                    configuration
                );
                foreach (var objFile in objectFiles)
                {
                    var asObjFileBase = objFile as C.ObjectFileBase;
                    if (!asObjFileBase.PerformCompilation)
                    {
                        var fullPath = asObjFileBase.InputPath.ToString();
                        var filename = System.IO.Path.GetFileName(fullPath);
                        excludedSource.Add(filename);
                    }

                    var buildFile = objFile.MetaData as XcodeBuilder.BuildFile;
                    configuration.BuildFiles.Add(buildFile);
                }
            }

            configuration["EXCLUDED_SOURCE_FILE_NAMES"] = excludedSource;

            outTarget = target;
            outConfiguration = configuration;
        }

        public static void
        ProcessLibraryDependencies(
            ConsoleApplication module,
            XcodeBuilder.Target target)
        {
            // add library search paths prior to converting linker settings
            var linker = module.Settings as C.ICommonLinkerSettings;
            foreach (var library in module.Libraries)
            {
                if (library is C.StaticLibrary)
                {
                    var libDir = library.CreateTokenizedString(
                        "@dir($(0))",
#if BAM_V2
                        library.GeneratedPaths[C.StaticLibrary.LibraryKey]
#else
                        library.GeneratedPaths[C.StaticLibrary.Key]
#endif
                    );
                    lock (libDir)
                    {
                        if (!libDir.IsParsed)
                        {
                            libDir.Parse();
                        }
                    }
                    linker.LibraryPaths.Add(libDir);
                }
                else if (library is C.IDynamicLibrary)
                {
                    var libDir = library.CreateTokenizedString(
                        "@dir($(0))",
#if BAM_V2
                        library.GeneratedPaths[C.DynamicLibrary.ExecutableKey]
#else
                        library.GeneratedPaths[C.DynamicLibrary.Key]
#endif
                    );
                    lock (libDir)
                    {
                        if (!libDir.IsParsed)
                        {
                            libDir.Parse();
                        }
                    }
                    linker.LibraryPaths.Add(libDir);
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
                else if (library is OSXFramework)
                {
                    // frameworks are dealt with elsewhere
                }
                else
                {
                    throw new Bam.Core.Exception("Don't know how to handle this module type, {0}", library.ToString());
                }
            }

            foreach (var library in module.Libraries)
            {
                var libAsCModule = library as C.CModule;
                if (null == libAsCModule)
                {
                    throw new Bam.Core.Exception("Don't know how to handle library module of type '{0}'", library.GetType().ToString());
                }
                if (libAsCModule.IsPrebuilt)
                {
                    if (library is OSXFramework)
                    {
                        // frameworks are dealt with elsewhere
                    }
                    else if (library is C.StaticLibrary)
                    {
                        (module.Tool as C.LinkerTool).ProcessLibraryDependency(module as CModule, libAsCModule);
                    }
                    else
                    {
                        throw new Bam.Core.Exception("Don't know how to handle this prebuilt module dependency, '{0}'", library.GetType().ToString());
                    }
                }
                else
                {
                    if (library is C.StaticLibrary)
                    {
                        target.DependsOn(library.MetaData as XcodeBuilder.Target);
                    }
                    else if (library is C.IDynamicLibrary)
                    {
                        target.DependsOn(library.MetaData as XcodeBuilder.Target);
                    }
                    else if (library is C.CSDKModule)
                    {
                        // do nothing, just an area for external
                    }
                    else if (library is C.HeaderLibrary)
                    {
                        // no library
                    }
                    else if (library is OSXFramework)
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

        public static void
        AddOrderOnlyDependentProjects(
            C.CModule module,
            XcodeBuilder.Target target)
        {
            // the target for a HeaderLibrary has no FileReference output, and thus cannot be an order only dependency
            var order_only_targets =
                module.OrderOnlyDependents().
                Distinct().
                Where(item => item.MetaData != null && item.MetaData is XcodeBuilder.Target && !(item is HeaderLibrary)).
                Select(item => item.MetaData as XcodeBuilder.Target);
            foreach (var required_target in order_only_targets)
            {
                target.Requires(required_target);
            }
        }
    }
#endif
                }
