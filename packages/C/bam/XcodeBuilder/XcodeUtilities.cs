#region License
// Copyright (c) 2010-2019, Mark Final
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
    /// <summary>
    /// Utility class offering support for Xcode project generation
    /// </summary>
    static partial class XcodeSupport
    {
        /// <summary>
        /// Generate an Xcode Target that has been the result of a link or an archive
        /// </summary>
        /// <param name="outTarget">Generated Target</param>
        /// <param name="outConfiguration">Generated Configuration</param>
        /// <param name="module">Module represented</param>
        /// <param name="fileType">Filetype of the output</param>
        /// <param name="productType">Product type of the output</param>
        /// <param name="productName">Product name of the output</param>
        /// <param name="outputPath">Path to the output</param>
        /// <param name="headerFiles">Any header files to be included</param>
        public static void
        LinkOrArchive(
            out XcodeBuilder.Target outTarget,
            out XcodeBuilder.Configuration outConfiguration,
            CModule module,
            XcodeBuilder.FileReference.EFileType fileType,
            XcodeBuilder.Target.EProductType productType,
            Bam.Core.TokenizedString productName,
            Bam.Core.TokenizedString outputPath,
            System.Collections.Generic.IEnumerable<Bam.Core.Module> headerFiles)
        {
            if (module.IsPrebuilt || !module.InputModulePaths.Any())
            {
                outTarget = null;
                outConfiguration = null;
                return;
            }

            var workspace = Bam.Core.Graph.Instance.MetaData as XcodeBuilder.WorkspaceMeta;
            var project = workspace.EnsureProjectExists(module, module.PackageDefinition.FullName);
            var target = workspace.EnsureTargetExists(module, project);
            var output_filename = module.CreateTokenizedString(
                "@filename($(0))",
                outputPath
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

            // there are settings explicitly for prefix and suffix
            if (module is C.Plugin || module is C.Cxx.Plugin)
            {
                var prefix = module.CreateTokenizedString("$(pluginprefix)");
                if (!prefix.IsParsed)
                {
                    prefix.Parse();
                }
                configuration["EXECUTABLE_PREFIX"] = new XcodeBuilder.UniqueConfigurationValue(prefix.ToString());
                var suffix = module.CreateTokenizedString("$(pluginext)");
                if (!suffix.IsParsed)
                {
                    suffix.Parse();
                }
                configuration["EXECUTABLE_EXTENSION"] = new XcodeBuilder.UniqueConfigurationValue(suffix.ToString().TrimStart(new[] { '.' }));
            }
            else if (module is C.DynamicLibrary || module is C.Cxx.DynamicLibrary)
            {
                var prefix = module.CreateTokenizedString("$(dynamicprefix)");
                if (!prefix.IsParsed)
                {
                    prefix.Parse();
                }
                configuration["EXECUTABLE_PREFIX"] = new XcodeBuilder.UniqueConfigurationValue(prefix.ToString());
                var suffix = module.CreateTokenizedString("$(dynamicextonly)");
                if (!suffix.IsParsed)
                {
                    suffix.Parse();
                }
                configuration["EXECUTABLE_EXTENSION"] = new XcodeBuilder.UniqueConfigurationValue(suffix.ToString().TrimStart(new[] { '.' }));
            }
            else if (module is C.ConsoleApplication || module is C.Cxx.ConsoleApplication)
            {
                var prefix = string.Empty;
                configuration["EXECUTABLE_PREFIX"] = new XcodeBuilder.UniqueConfigurationValue(prefix);
                var suffix = module.CreateTokenizedString("$(exeext)");
                if (!suffix.IsParsed)
                {
                    suffix.Parse();
                }
                configuration["EXECUTABLE_EXTENSION"] = new XcodeBuilder.UniqueConfigurationValue(suffix.ToString().TrimStart(new[] { '.' }));
            }
            else if (module is C.StaticLibrary)
            {
                // nothing to set
            }
            else
            {
                throw new Bam.Core.Exception(
                    $"Unknown type of executable is being processed: {module.ToString()}"
                );
            }

            foreach (var header in headerFiles)
            {
                target.EnsureHeaderFileExists((header as HeaderFile).InputPath);
            }

            var excludedSource = new XcodeBuilder.MultiConfigurationValue();
            var realObjectFiles = module.InputModulePaths.Select(item => item.module).Where(item => item is ObjectFile); // C,C++,ObjC,ObjC++
            if (realObjectFiles.Any())
            {
                var sharedSettings = C.SettingsBase.SharedSettings(
                    realObjectFiles
                );
                XcodeSharedSettings.Tweak(sharedSettings, realObjectFiles.Count() != module.InputModulePaths.Count());
                XcodeProjectProcessor.XcodeConversion.Convert(
                    sharedSettings,
                    module,
                    configuration,
                    settingsTypeOverride: realObjectFiles.First().Settings.GetType()
                );

                foreach (var objFile in realObjectFiles)
                {
                    var asObjFileBase = objFile as C.ObjectFileBase;
                    if (!asObjFileBase.PerformCompilation)
                    {
                        excludedSource.Add((asObjFileBase as C.IRequiresSourceModule).Source.InputPath.ToString());
                    }

                    var buildFile = objFile.MetaData as XcodeBuilder.BuildFile;
                    var deltaSettings = (objFile.Settings as C.SettingsBase).CreateDeltaSettings(sharedSettings, objFile);
                    if (null != deltaSettings)
                    {
                        if (deltaSettings is C.ICommonPreprocessorSettings preprocessor)
                        {
                            // this happens for mixed C language source files, e.g. C++ and ObjC++,
                            // 1) the target language is already encoded in the file type
                            // 2) Xcode 10's build system seems to ignore some C++ language settings if -x c++ appears on C++ source files
                            preprocessor.TargetLanguage = null;
                        }

                        var commandLine = CommandLineProcessor.NativeConversion.Convert(
                            deltaSettings,
                            objFile,
                            createDelta: true
                        );
                        if (commandLine.Any())
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
                var assembledObjectFiles = module.InputModulePaths.Select(item => item.module).Where(item => item is AssembledObjectFile);
                foreach (var asmObj in assembledObjectFiles)
                {
                    var buildFile = asmObj.MetaData as XcodeBuilder.BuildFile;
                    configuration.BuildFiles.Add(buildFile);
                }
            }
            else
            {
                var firstInputModuleSettings = module.InputModulePaths.First().module.Settings;
                XcodeProjectProcessor.XcodeConversion.Convert(
                    firstInputModuleSettings,
                    module,
                    configuration
                );
                foreach (var objFile in module.InputModulePaths.Select(item => item.module))
                {
                    var asObjFileBase = objFile as C.ObjectFileBase;
                    if (!asObjFileBase.PerformCompilation)
                    {
                        excludedSource.Add((asObjFileBase as C.IRequiresSourceModule).Source.InputPath.ToString());
                    }

                    var buildFile = objFile.MetaData as XcodeBuilder.BuildFile;
                    configuration.BuildFiles.Add(buildFile);
                }
            }

            configuration["EXCLUDED_SOURCE_FILE_NAMES"] = excludedSource;

            outTarget = target;
            outConfiguration = configuration;
        }

        /// <summary>
        /// Process all library dependencies on a module
        /// </summary>
        /// <param name="module">Module with dependencies</param>
        /// <param name="target">Target to add dependencies on.</param>
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
                    foreach (var dir in library.OutputDirectories)
                    {
                        linker.LibraryPaths.AddUnique(dir);
                    }
                }
                else if (library is C.IDynamicLibrary)
                {
                    foreach (var dir in library.OutputDirectories)
                    {
                        linker.LibraryPaths.AddUnique(dir);
                    }
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
                else if (library is C.SDKTemplate)
                {
                    foreach (var dir in library.OutputDirectories)
                    {
                        linker.LibraryPaths.AddUnique(dir);
                    }
                }
                else
                {
                    throw new Bam.Core.Exception(
                        $"Don't know how to handle this module type, {library.ToString()}"
                    );
                }
            }

            foreach (var library in module.Libraries)
            {
                var libAsCModule = library as C.CModule;
                if (null == libAsCModule)
                {
                    if (library is C.SDKTemplate)
                    {
                        if (library.MetaData is XcodeBuilder.Target libraryTarget)
                        {
                            target.Requires(libraryTarget);
                            foreach (var forwarded in (library as IForwardedLibraries).ForwardedLibraries)
                            {
                                if (forwarded is C.IDynamicLibrary)
                                {
                                    target.EnsureFrameworksBuildFileExists(
                                        (forwarded as Bam.Core.Module).GeneratedPaths[C.DynamicLibrary.ExecutableKey],
                                        XcodeBuilder.FileReference.EFileType.DynamicLibrary,
                                        XcodeBuilder.FileReference.ESourceTree.Absolute
                                    );
                                }
                                else if (forwarded is C.StaticLibrary)
                                {
                                    target.EnsureFrameworksBuildFileExists(
                                        (forwarded as Bam.Core.Module).GeneratedPaths[C.StaticLibrary.LibraryKey],
                                        XcodeBuilder.FileReference.EFileType.Archive,
                                        XcodeBuilder.FileReference.ESourceTree.Absolute
                                    );
                                }
                            }
                        }
                        else
                        {
                            foreach (var forwarded in (library as IForwardedLibraries).ForwardedLibraries)
                            {
                                (module.Tool as C.LinkerTool).ProcessLibraryDependency(module as CModule, forwarded as CModule);
                            }
                        }
                        continue;
                    }
                    else
                    {
                        throw new Bam.Core.Exception(
                            $"Don't know how to handle library module of type '{library.GetType().ToString()}'"
                        );
                    }
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
                        throw new Bam.Core.Exception(
                            $"Don't know how to handle this prebuilt module dependency, '{library.GetType().ToString()}'"
                        );
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

        /// <summary>
        /// Add order only dependencies from a module
        /// </summary>
        /// <param name="module">Module containing order only dependencies</param>
        /// <param name="target">Target to add dependencies on</param>
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
}
