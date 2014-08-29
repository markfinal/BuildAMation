#region License
// Copyright 2010-2014 Mark Final
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
