// <copyright file="ProductModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed partial class XcodeBuilder
    {
        private static void
        CopyNodes(
            Publisher.ProductModule moduleToBuild,
            PBXProject project,
            Opus.Core.DependencyNode toCopy,
            PBXNativeTarget nativeTarget,
            string subdirectory)
        {
            var copyFilesBuildPhase = project.CopyFilesBuildPhases.Get("CopyFiles", moduleToBuild.OwningNode.ModuleName);
            copyFilesBuildPhase.SubFolder = PBXCopyFilesBuildPhase.ESubFolder.Executables;
            if (!string.IsNullOrEmpty(subdirectory))
            {
                copyFilesBuildPhase.DestinationPath = subdirectory;
            }
            nativeTarget.BuildPhases.AddUnique(copyFilesBuildPhase);

            var copySourceNativeTarget = toCopy.Data as PBXNativeTarget;

            // need a different copy of the build file, to live in the CopyFiles build phase
            // but still referencing the same PBXFileReference
            var type = copySourceNativeTarget.ProductReference.Type;
            if (type == PBXFileReference.EType.DynamicLibrary)
            {
                type = PBXFileReference.EType.ReferencedDynamicLibrary;
            }
            var relativePath = Opus.Core.RelativePathUtilities.GetPath(copySourceNativeTarget.ProductReference.FullPath, project.RootUri);
            var dependentFileRef = project.FileReferences.Get(toCopy.UniqueModuleName, type, relativePath, project.RootUri);
            var buildFile = project.BuildFiles.Get(toCopy.UniqueModuleName, dependentFileRef, copyFilesBuildPhase);
            if (null == buildFile)
            {
                throw new Opus.Core.Exception("Build file not available");
            }

            project.MainGroup.Children.AddUnique(dependentFileRef);
        }

        public object
        Build(
            Publisher.ProductModule moduleToBuild,
            out bool success)
        {
            var options = moduleToBuild.Options as Publisher.IPublishOptions;

            var primaryNodeData = Publisher.ProductModuleUtilities.GetPrimaryNodeData(moduleToBuild);
            if (null == primaryNodeData)
            {
                success = true;
                return null;
            }

            var primaryNode = primaryNodeData.Node;
            var project = this.Workspace.GetProject(primaryNode);
            var primaryPBXNativeTarget = primaryNode.Data as PBXNativeTarget;

            if (options.OSXApplicationBundle)
            {
                primaryPBXNativeTarget.Type = PBXNativeTarget.EType.ApplicationBundle;
                primaryPBXNativeTarget.ProductReference.SetType(PBXFileReference.EType.ApplicationBundle);
            }

            var dependents = new Opus.Core.DependencyNodeCollection();
            if (null != primaryNode.ExternalDependents)
            {
                dependents.AddRange(primaryNode.ExternalDependents);
            }
            if (null != primaryNode.RequiredDependents)
            {
                dependents.AddRange(primaryNode.RequiredDependents);
            }

            foreach (var dependency in dependents)
            {
                var module = dependency.Module;
                var moduleType = module.GetType();
                var flags = System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic;
                var fields = moduleType.GetFields(flags);
                foreach (var field in fields)
                {
                    var candidates = field.GetCustomAttributes(typeof(Publisher.PublishModuleDependencyAttribute), false);
                    if (0 == candidates.Length)
                    {
                        continue;
                    }
                    if (candidates.Length > 1)
                    {
                        throw new Opus.Core.Exception("More than one publish module dependency found");
                    }
                    var attribute = candidates[0] as Publisher.PublishModuleDependencyAttribute;
                    var matchesTarget = Opus.Core.TargetUtilities.MatchFilters(moduleToBuild.OwningNode.Target, attribute);
                    if (!matchesTarget)
                    {
                        continue;
                    }
                    var candidateData = field.GetValue(module) as Opus.Core.Array<Opus.Core.LocationKey>;
                    if (null != candidateData)
                    {
                        foreach (var key in candidateData)
                        {
                            if (!module.Locations.Contains(key))
                            {
                                continue;
                            }

                            CopyNodes(moduleToBuild, project, module.OwningNode, primaryPBXNativeTarget, string.Empty);
                        }
                    }
                    else
                    {
                        var candidateData2 = field.GetValue(module) as Opus.Core.Array<Publisher.PublishDependency>;
                        if (null == candidateData2)
                        {
                            throw new Opus.Core.Exception("Unrecognized type for dependency data");
                        }

                        foreach (var dep in candidateData2)
                        {
                            var key = dep.Key;
                            if (!module.Locations.Contains(key))
                            {
                                continue;
                            }

                            // take the common subdirectory by default, otherwise override on a per Location basis
                            var subDirectory = attribute.CommonSubDirectory;
                            if (!string.IsNullOrEmpty(dep.SubDirectory))
                            {
                                subDirectory = dep.SubDirectory;
                            }

                            CopyNodes(moduleToBuild, project, module.OwningNode, primaryPBXNativeTarget, subDirectory);
                        }
                    }
                }
            }

            success = true;
            return null;
        }
    }
}
