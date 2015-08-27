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
        public sealed class XcodeLibrarian :
            ILibrarianPolicy
        {
            void
            ILibrarianPolicy.Archive(
                StaticLibrary sender,
                Bam.Core.V2.ExecutionContext context,
                Bam.Core.V2.TokenizedString libraryPath,
                System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> objectFiles,
                System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> headers)
            {
                var library = new XcodeBuilder.V2.XcodeStaticLibrary(sender, libraryPath);

#if true
                if (objectFiles.Count > 1)
                {
                    var xcodeConvertParameterTypes = new Bam.Core.TypeArray
                    {
                        typeof(Bam.Core.V2.Module),
                        typeof(XcodeBuilder.V2.Configuration)
                    };

                    var sharedSettings = C.V2.SettingsBase.SharedSettings(
                        objectFiles,
                        typeof(Clang.XcodeImplementation),
                        typeof(XcodeProjectProcessor.V2.IConvertToProject),
                        xcodeConvertParameterTypes);
                    library.SetCommonCompilationOptions(null, sharedSettings);

                    foreach (var objFile in objectFiles)
                    {
                        var deltaSettings = (objFile.Settings as C.V2.SettingsBase).CreateDeltaSettings(sharedSettings, objFile);
                        var meta = objFile.MetaData as XcodeBuilder.V2.XcodeObjectFile;
                        library.AddSource(objFile, meta.Source, meta.Output, deltaSettings);
                        meta.Project = library.Project;
                    }
                }
                else
                {
                    library.SetCommonCompilationOptions(null, objectFiles[0].Settings);
                    foreach (var objFile in objectFiles)
                    {
                        var meta = objFile.MetaData as XcodeBuilder.V2.XcodeObjectFile;
                        library.AddSource(objFile, meta.Source, meta.Output, null);
                        meta.Project = library.Project;
                    }
                }
#else
                var commonObject = inputs[0];
                library.SetCommonCompilationOptions(commonObject, commonObject.Settings);

                foreach (var input in inputs)
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
                            library.AddSource(child, meta.Source, meta.Output, patchSettings);
                            meta.Project = library.Project;
                        }
                    }
                    else
                    {
                        var meta = input.MetaData as XcodeBuilder.V2.XcodeObjectFile;
                        library.AddSource(input, meta.Source, meta.Output, deltaSettings);
                        meta.Project = library.Project;
                    }
                }
#endif

                foreach (var header in headers)
                {
                    var headerMod = header as HeaderFile;
                    var headerFileRef = library.Project.FindOrCreateFileReference(
                        headerMod.InputPath,
                        XcodeBuilder.V2.FileReference.EFileType.HeaderFile,
                        sourceTree:XcodeBuilder.V2.FileReference.ESourceTree.Absolute);
                    library.AddHeader(headerFileRef);
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
            C.StaticLibrary moduleToBuild,
            out bool success)
        {
            var node = moduleToBuild.OwningNode;
            var moduleName = node.ModuleName;
            var target = node.Target;
            var baseTarget = (Bam.Core.BaseTarget)target;

            var options = moduleToBuild.Options as C.ArchiverOptionCollection;
            var libraryLocation = moduleToBuild.Locations[C.StaticLibrary.OutputFileLocKey];

            var project = this.Workspace.GetProject(node);

            var fileRef = project.FileReferences.Get(moduleName, PBXFileReference.EType.StaticLibrary, libraryLocation, project.RootUri);
            project.ProductsGroup.Children.AddUnique(fileRef);

            var data = project.NativeTargets.Get(moduleName, PBXNativeTarget.EType.StaticLibrary, project);
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

                        // since static libraries have no in-built dependency, add an artificial one into the scheme
                        if (null != objectFile.ExternalDependents)
                        {
                            foreach (var dep in objectFile.ExternalDependents)
                            {
                                if (dep.Data is PBXNativeTarget)
                                {
                                    data.RequiredTargets.AddUnique(dep.Data as PBXNativeTarget);
                                }
                            }
                        }
                    }
                }
                else
                {
                    var buildFile = childNode.Data as PBXBuildFile;
                    data.SourceFilesToBuild.AddUnique(buildFile);

                    // since static libraries have no in-built dependency, add an artificial one into the scheme
                    if (null != childNode.ExternalDependents)
                    {
                        foreach (var dep in childNode.ExternalDependents)
                        {
                            if (dep.Data is PBXNativeTarget)
                            {
                                data.RequiredTargets.AddUnique(dep.Data as PBXNativeTarget);
                            }
                        }
                    }
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

            var archiverTool = target.Toolset.Tool(typeof(C.IArchiverTool)) as C.IArchiverTool;
            var outputPrefix = archiverTool.StaticLibraryPrefix;
            var outputSuffix = archiverTool.StaticLibrarySuffix;
            buildConfiguration.Options["EXECUTABLE_PREFIX"].AddUnique(outputPrefix);
            buildConfiguration.Options["EXECUTABLE_SUFFIX"].AddUnique(outputSuffix);

            var basePath = Bam.Core.State.BuildRoot + System.IO.Path.DirectorySeparatorChar;
            var relPath = Bam.Core.RelativePathUtilities.GetPath(moduleToBuild.Locations[C.StaticLibrary.OutputDirLocKey], basePath);
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

            // any external dependents get turned into scheme requirements
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
                }
            }

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
