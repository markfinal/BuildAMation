#region License
// Copyright 2010-2015 Mark Final
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
                Bam.Core.V2.ExecutionContext context,
                Bam.Core.V2.TokenizedString executablePath,
                System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> objectFiles,
                System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> libraries,
                System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> frameworks)
            {
                var linker = sender.Settings as C.V2.ICommonLinkerOptions;
                // TODO: could the lib search paths be in the staticlibrary base class as a patch?
                var configName = sender.BuildEnvironment.Configuration.ToString();
                var macros = new Bam.Core.V2.MacroList();
                macros.Add("moduleoutputdir", Bam.Core.V2.TokenizedString.Create(configName, null));
                foreach (var library in libraries)
                {
                    if (library is C.V2.StaticLibrary)
                    {
                        var fullLibraryPath = library.GeneratedPaths[C.V2.StaticLibrary.Key].Parse(macros);
                        var dir = System.IO.Path.GetDirectoryName(fullLibraryPath);
                        linker.LibraryPaths.Add(Bam.Core.V2.TokenizedString.Create(dir, null));
                    }
                    else if (library is C.V2.DynamicLibrary)
                    {
                        var fullLibraryPath = library.GeneratedPaths[C.V2.DynamicLibrary.Key].Parse(macros);
                        var dir = System.IO.Path.GetDirectoryName(fullLibraryPath);
                        linker.LibraryPaths.Add(Bam.Core.V2.TokenizedString.Create(dir, null));
                    }
                    else if (library is C.V2.CSDKModule)
                    {
                        throw new Bam.Core.Exception("Don't know how to handle this module type: CSDKModule");
                    }
                    else
                    {
                        throw new Bam.Core.Exception("Don't know how to handle this module type");
                    }
                }

                XcodeBuilder.V2.XcodeCommonLinkable application;
                if (sender is DynamicLibrary)
                {
                    application = new XcodeBuilder.V2.XcodeDynamicLibrary(sender, executablePath);
                }
                else
                {
                    application = new XcodeBuilder.V2.XcodeProgram(sender, executablePath);
                }

                var interfaceType = Bam.Core.State.ScriptAssembly.GetType("XcodeProjectProcessor.V2.IConvertToProject");
                if (interfaceType.IsAssignableFrom(sender.Settings.GetType()))
                {
                    var map = sender.Settings.GetType().GetInterfaceMap(interfaceType);
                    map.InterfaceMethods[0].Invoke(sender.Settings, new object[] { sender, application.Configuration });
                }

                var commonObject = objectFiles[0];
                application.SetCommonCompilationOptions(commonObject, commonObject.Settings);

                foreach (var input in objectFiles)
                {
                    C.V2.SettingsBase deltaSettings = null;
                    if (input != commonObject)
                    {
                        deltaSettings = (input.Settings as C.V2.SettingsBase).Delta(commonObject.Settings, input);
                    }

                    if (input is Bam.Core.V2.IModuleGroup)
                    {
                        foreach (var child in input.Children)
                        {
                            Bam.Core.V2.Settings patchSettings = deltaSettings;
                            if (child.HasPatches)
                            {
                                if (null == patchSettings)
                                {
                                    patchSettings = System.Activator.CreateInstance(input.Settings.GetType(), child, false) as C.V2.SettingsBase;
                                }
                                else
                                {
                                    patchSettings = deltaSettings.Clone(child);
                                }
                                child.ApplySettingsPatches(patchSettings, honourParents: false);
                            }

                            var meta = child.MetaData as XcodeBuilder.V2.XcodeObjectFile;
                            application.AddSource(child, meta.Source, meta.Output, patchSettings);
                            meta.Project = application.Project;
                        }
                    }
                    else
                    {
                        var meta = input.MetaData as XcodeBuilder.V2.XcodeObjectFile;
                        application.AddSource(input, meta.Source, meta.Output, deltaSettings);
                        meta.Project = application.Project;
                    }
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
                        throw new Bam.Core.Exception("Don't know how to handle this module type: CSDKModule");
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
namespace XcodeBuilder
{
    public sealed partial class XcodeBuilder
    {
        public object
        Build(
            C.Application moduleToBuild,
            out bool success)
        {
            var node = moduleToBuild.OwningNode;
            var moduleName = node.ModuleName;
            var target = node.Target;
            var baseTarget = (Bam.Core.BaseTarget)target;

            var options = moduleToBuild.Options as C.LinkerOptionCollection;
            var executableLocation = moduleToBuild.Locations[C.Application.OutputFile];

            var project = this.Workspace.GetProject(node);

            var fileRef = project.FileReferences.Get(moduleName, PBXFileReference.EType.Executable, executableLocation, project.RootUri);
            project.ProductsGroup.Children.AddUnique(fileRef);

            var data = project.NativeTargets.Get(moduleName, PBXNativeTarget.EType.Executable, project);
            data.ProductReference = fileRef;

            // gather up all the source files for this target
            foreach (var childNode in node.Children)
            {
                if (childNode.Module is C.ObjectFileCollectionBase)
                {
                    foreach (var objectFile in childNode.Children)
                    {
                        var buildFile = objectFile.Data as PBXBuildFile;
                        data.SourceFilesToBuild.AddUnique(buildFile);
                    }
                }
                else
                {
                    var buildFile = childNode.Data as PBXBuildFile;
                    data.SourceFilesToBuild.AddUnique(buildFile);
                }
            }

            // build configuration target overrides to the project build configuration
            var buildConfiguration = project.BuildConfigurations.Get(baseTarget.ConfigurationName('='), moduleName);
            var nativeTargetConfigurationList = project.ConfigurationLists.Get(data);
            nativeTargetConfigurationList.AddUnique(buildConfiguration);
            if (null == data.BuildConfigurationList)
            {
                data.BuildConfigurationList = nativeTargetConfigurationList;
            }
            else
            {
                if (data.BuildConfigurationList != nativeTargetConfigurationList)
                {
                    throw new Bam.Core.Exception("Inconsistent build configuration lists");
                }
            }

            // fill out the build configuration
            XcodeProjectProcessor.ToXcodeProject.Execute(moduleToBuild.Options, project, data, buildConfiguration, target);

            buildConfiguration.Options["PRODUCT_NAME"].AddUnique(options.OutputName);

            // Xcode 4 complains this is missing for target configurations
            buildConfiguration.Options["COMBINE_HIDPI_IMAGES"].AddUnique("YES");

            var linkerTool = target.Toolset.Tool(typeof(C.ILinkerTool)) as C.ILinkerTool;
            var outputSuffix = linkerTool.ExecutableSuffix;
            buildConfiguration.Options["EXECUTABLE_SUFFIX"].AddUnique(outputSuffix);

            var basePath = Bam.Core.State.BuildRoot + System.IO.Path.DirectorySeparatorChar;
            var outputDirLoc = moduleToBuild.Locations[C.Application.OutputDir];
            var relPath = Bam.Core.RelativePathUtilities.GetPath(outputDirLoc, basePath);
            buildConfiguration.Options["CONFIGURATION_BUILD_DIR"].AddUnique("$SYMROOT/" + relPath);

            // adding the group for the target
            var group = project.Groups.Get(moduleName);
            group.SourceTree = "<group>";
            group.Path = moduleName;
            foreach (var source in node.Children)
            {
                if (source.Module is Bam.Core.IModuleCollection)
                {
                    foreach (var source2 in source.Children)
                    {
                        var sourceData = source2.Data as PBXBuildFile;
                        group.Children.AddUnique(sourceData.FileReference);
                    }
                }
                else
                {
                    var sourceData = source.Data as PBXBuildFile;
                    group.Children.AddUnique(sourceData.FileReference);
                }
            }
            data.Group = group;
            project.MainGroup.Children.AddUnique(group);

            var sourcesBuildPhase = project.SourceBuildPhases.Get("Sources", moduleName);
            data.BuildPhases.AddUnique(sourcesBuildPhase);

            var frameworksBuildPhase = project.FrameworksBuildPhases.Get("Frameworks", moduleName);
            data.BuildPhases.AddUnique(frameworksBuildPhase);

            if (null != node.ExternalDependents)
            {
                foreach (var dependency in node.ExternalDependents)
                {
                    var dependentData = dependency.Data as PBXNativeTarget;
                    if (null == dependentData)
                    {
                        continue;
                    }

                    // accumulate any scheme requirements from dependents
                    data.RequiredTargets.AddRangeUnique(dependentData.RequiredTargets);

                    if (dependentData.Project == project)
                    {
                        // first add a dependency so that they are built in the right order
                        // this is only required within the same project
                        var targetDependency = project.TargetDependencies.Get(moduleName, dependentData);

                        var containerItemProxy = project.ContainerItemProxies.Get(moduleName, dependentData, project);
                        targetDependency.TargetProxy = containerItemProxy;

                        data.Dependencies.Add(targetDependency);
                    }

                    // now add a link dependency
                    if (dependentData.Project == project)
                    {
                        var buildFile = project.BuildFiles.Get(dependency.UniqueModuleName, dependentData.ProductReference, frameworksBuildPhase);
                        if (null == buildFile)
                        {
                            throw new Bam.Core.Exception("Build file not available");
                        }

                        // now add linker search paths
                        if (dependency.Module is C.DynamicLibrary)
                        {
                            var outputDir = moduleToBuild.Locations[C.Application.OutputDir].GetSinglePath();
                            buildConfiguration.Options["LIBRARY_SEARCH_PATHS"].AddUnique(outputDir);
                        }
                        else if (dependency.Module is C.StaticLibrary)
                        {
                            var outputDir = moduleToBuild.Locations[C.StaticLibrary.OutputDirLocKey].GetSinglePath();
                            buildConfiguration.Options["LIBRARY_SEARCH_PATHS"].AddUnique(outputDir);
                        }
                    }
                    else
                    {
                        var type = dependentData.ProductReference.Type;
                        if (type == PBXFileReference.EType.StaticLibrary)
                        {
                            type = PBXFileReference.EType.ReferencedStaticLibrary;
                        }
                        if (type == PBXFileReference.EType.DynamicLibrary)
                        {
                            type = PBXFileReference.EType.ReferencedDynamicLibrary;
                        }

                        var relativePath = Bam.Core.RelativePathUtilities.GetPath(dependentData.ProductReference.FullPath, project.RootUri);
                        var dependentFileRef = project.FileReferences.Get(dependency.UniqueModuleName, type, relativePath, project.RootUri);
                        var buildFile = project.BuildFiles.Get(dependency.UniqueModuleName, dependentFileRef, frameworksBuildPhase);
                        if (null == buildFile)
                        {
                            throw new Bam.Core.Exception("Build file not available");
                        }

                        project.MainGroup.Children.AddUnique(dependentFileRef);

                        // now add linker search paths
                        buildConfiguration.Options["LIBRARY_SEARCH_PATHS"].AddUnique("$(inherited)");
                        if (dependency.Module is C.DynamicLibrary)
                        {
                            var outputDir = dependency.Module.Locations[C.Application.OutputDir].GetSinglePath();
                            buildConfiguration.Options["LIBRARY_SEARCH_PATHS"].AddUnique(outputDir);
                        }
                        else if (dependency.Module is C.StaticLibrary)
                        {
                            var outputDir = dependency.Module.Locations[C.StaticLibrary.OutputDirLocKey].GetSinglePath();
                            buildConfiguration.Options["LIBRARY_SEARCH_PATHS"].AddUnique(outputDir);
                        }
                    }
                }
            }

            // any required nodes must be registered as an ordering in the schema
            foreach (var req in node.EncapsulatingRequirements)
            {
                var reqData = req.Data as PBXNativeTarget;
                if (null != reqData)
                {
                    data.RequiredTargets.AddUnique(reqData);
                }
            }

            // find header files
            var fieldBindingFlags = System.Reflection.BindingFlags.Instance |
                                        System.Reflection.BindingFlags.Public |
                                            System.Reflection.BindingFlags.NonPublic;
            var fields = moduleToBuild.GetType().GetFields(fieldBindingFlags);
            foreach (var field in fields)
            {
                var headerFileAttributes = field.GetCustomAttributes(typeof(C.HeaderFilesAttribute), false);
                if (headerFileAttributes.Length > 0)
                {
                    var headerFileCollection = field.GetValue(moduleToBuild) as Bam.Core.FileCollection;
                    foreach (Bam.Core.Location location in headerFileCollection)
                    {
                        var headerPath = location.GetSinglePath();
                        var headerFileRef = project.FileReferences.Get(moduleName, PBXFileReference.EType.HeaderFile, headerPath, project.RootUri);
                        group.Children.AddUnique(headerFileRef);
                    }
                }
            }

            // TODO: this is the WRONG place to put this
            // add outstanding build phases made by nodes prior to this
            foreach (var scriptBuildPhase in project.ShellScriptBuildPhases)
            {
                data.BuildPhases.Insert(0, scriptBuildPhase as PBXShellScriptBuildPhase);
            }

            success = true;
            return data;
        }
    }
}
