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
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> libraries)
        {
            if (0 == objectFiles.Count)
            {
                return;
            }

            var workspace = Bam.Core.Graph.Instance.MetaData as XcodeBuilder.WorkspaceMeta;
            var target = workspace.EnsureTargetExists(sender);
            var exeFilename = sender.CreateTokenizedString("@filename($(0))", executablePath);
            exeFilename.Parse();
            target.EnsureOutputFileReferenceExists(
                exeFilename,
                (sender is IDynamicLibrary) ? XcodeBuilder.FileReference.EFileType.DynamicLibrary : XcodeBuilder.FileReference.EFileType.Executable,
                (sender is IDynamicLibrary) ? XcodeBuilder.Target.EProductType.DynamicLibrary : XcodeBuilder.Target.EProductType.Executable);
            var configuration = target.GetConfiguration(sender);
            if (sender is IDynamicLibrary && !((sender is Plugin) || (sender is C.Cxx.Plugin)))
            {
                var productName = sender.Macros["OutputName"].ToString().Equals(sender.Macros["modulename"].ToString()) ?
                                        sender.CreateTokenizedString("${TARGET_NAME}.$(MajorVersion)") :
                                        sender.CreateTokenizedString("$(OutputName).$(MajorVersion)");
                lock (productName)
                {
                    if (!productName.IsParsed)
                    {
                        productName.Parse();
                    }
                }
                configuration.SetProductName(productName);
            }
            else
            {
                if (sender.Macros["OutputName"].ToString().Equals(sender.Macros["modulename"].ToString()))
                {
                    configuration.SetProductName(Bam.Core.TokenizedString.CreateVerbatim("${TARGET_NAME}"));
                }
                else
                {
                    configuration.SetProductName(sender.Macros["OutputName"]);
                }
            }

            foreach (var header in headers)
            {
                target.EnsureHeaderFileExists((header as HeaderFile).InputPath);
            }

            var excludedSource = new XcodeBuilder.MultiConfigurationValue();
            var realObjectFiles = objectFiles.Where(item => !((item is WinResource) || (item is AssembledObjectFile)));
            if (realObjectFiles.Any())
            {
                var xcodeConvertParameterTypes = new Bam.Core.TypeArray
                {
                    typeof(Bam.Core.Module),
                    typeof(XcodeBuilder.Configuration)
                };

                var sharedSettings = C.SettingsBase.SharedSettings(
                    realObjectFiles,
                    typeof(ClangCommon.XcodeCompilerImplementation),
                    typeof(XcodeProjectProcessor.IConvertToProject),
                    xcodeConvertParameterTypes);
                XcodeSharedSettings.Tweak(sharedSettings);
                (sharedSettings as XcodeProjectProcessor.IConvertToProject).Convert(sender, configuration);

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
                        var commandLine = new Bam.Core.StringArray();
                        (deltaSettings as CommandLineProcessor.IConvertToCommandLine).Convert(commandLine);
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
                (objectFiles[0].Settings as XcodeProjectProcessor.IConvertToProject).Convert(sender, configuration);
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

            // add library search paths prior to converting linker settings
            var linker = sender.Settings as C.ICommonLinkerSettings;
            foreach (var library in libraries)
            {
                if (library is C.StaticLibrary)
                {
                    var libDir = library.CreateTokenizedString("@dir($(0))", library.GeneratedPaths[C.StaticLibrary.Key]);
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
                    var libDir = library.CreateTokenizedString("@dir($(0))", library.GeneratedPaths[C.DynamicLibrary.Key]);
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

            foreach (var library in libraries)
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
                        (sender.Tool as C.LinkerTool).ProcessLibraryDependency(sender as CModule, libAsCModule);
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

            // convert link settings to the Xcode project
            (sender.Settings as XcodeProjectProcessor.IConvertToProject).Convert(sender, configuration);

            var required_targets = new System.Collections.Generic.HashSet<XcodeBuilder.Target>();
            // order only dependencies - recurse into each, so that all layers
            // of order only dependencies are included
            var queue = new System.Collections.Generic.Queue<Bam.Core.Module>(sender.Requirements);
            while (queue.Count > 0)
            {
                var required = queue.Dequeue();
                foreach (var additional in required.Requirements)
                {
                    queue.Enqueue(additional);
                }

                if (null == required.MetaData)
                {
                    continue;
                }
                if (required is HeaderLibrary)
                {
                    // the target for a HeaderLibrary has no FileReference output, and thus cannot be an order only dependency
                    continue;
                }

                var requiredTarget = required.MetaData as XcodeBuilder.Target;
                if (null != requiredTarget)
                {
                    required_targets.Add(requiredTarget);
                }
            }
            // any non-C module projects should be order-only dependencies
            foreach (var dependent in sender.Dependents)
            {
                if (null == dependent.MetaData)
                {
                    continue;
                }
                if (dependent is C.CModule)
                {
                    continue;
                }
                var dependentTarget = dependent.MetaData as XcodeBuilder.Target;
                if (null != dependentTarget)
                {
                    required_targets.Add(dependentTarget);
                }
            }
            foreach (var reqTarget in required_targets)
            {
                target.Requires(reqTarget);
            }
        }
    }
}
