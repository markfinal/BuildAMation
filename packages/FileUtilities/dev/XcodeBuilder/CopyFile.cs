// <copyright file="CopyFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed partial class XcodeBuilder
    {
        public object Build(FileUtilities.CopyFile moduleToBuild, out bool success)
        {
            var copyOptions = moduleToBuild.Options as FileUtilities.ICopyFileOptions;
            if (null == copyOptions.SourceModuleType)
            {
                throw new Opus.Core.Exception("Xcode support for copying from arbitrary locations is unavailable");
            }

            var node = moduleToBuild.OwningNode;
            var target = node.Target;

            var copySourceNode = Opus.Core.ModuleUtilities.GetNode(copyOptions.SourceModuleType, (Opus.Core.BaseTarget)target);
            var copySourceData = copySourceNode.Data as PBXNativeTarget;

            var besideModuleType = moduleToBuild.BesideModuleType;
            if (null == besideModuleType)
            {
                var project = copySourceData.Project;
                var moduleName = copySourceNode.ModuleName;

                var copyFilesBuildPhase = project.CopyFilesBuildPhases.Get("CopyFiles", moduleName);
                copyFilesBuildPhase.DestinationPath = copyOptions.DestinationDirectory;
                copyFilesBuildPhase.SubFolder = PBXCopyFilesBuildPhase.ESubFolder.AbsolutePath;
                copySourceData.BuildPhases.AddUnique(copyFilesBuildPhase);

                // need a different copy of the build file, to live in the CopyFiles build phase
                // but still referencing the same PBXFileReference
                var type = copySourceData.ProductReference.Type;
                if (type == PBXFileReference.EType.DynamicLibrary)
                {
                    type = PBXFileReference.EType.ReferencedDynamicLibrary;
                }
                var relativePath = Opus.Core.RelativePathUtilities.GetPath(copySourceData.ProductReference.FullPath, project.RootUri);
                var dependentFileRef = project.FileReferences.Get(copySourceNode.UniqueModuleName, type, relativePath, project.RootUri);
                var buildFile = project.BuildFiles.Get(copySourceNode.UniqueModuleName, dependentFileRef, copyFilesBuildPhase);
                if (null == buildFile)
                {
                    throw new Opus.Core.Exception("Build file not available");
                }

                project.MainGroup.Children.AddUnique(dependentFileRef);
            }
            else
            {
                var besideModuleNode = Opus.Core.ModuleUtilities.GetNode(besideModuleType, (Opus.Core.BaseTarget)target);
                var besideModuleData = besideModuleNode.Data as PBXNativeTarget;

                var project = besideModuleData.Project;
                var moduleName = besideModuleNode.ModuleName;

                var copyFilesBuildPhase = project.CopyFilesBuildPhases.Get("CopyFiles", moduleName);
                besideModuleData.BuildPhases.AddUnique(copyFilesBuildPhase);

                // need a different copy of the build file, to live in the CopyFiles build phase
                // but still referencing the same PBXFileReference
                var type = copySourceData.ProductReference.Type;
                if (type == PBXFileReference.EType.DynamicLibrary)
                {
                    type = PBXFileReference.EType.ReferencedDynamicLibrary;
                }
                var relativePath = Opus.Core.RelativePathUtilities.GetPath(copySourceData.ProductReference.FullPath, project.RootUri);
                var dependentFileRef = project.FileReferences.Get(copySourceNode.UniqueModuleName, type, relativePath, project.RootUri);
                var buildFile = project.BuildFiles.Get(copySourceNode.UniqueModuleName, dependentFileRef, copyFilesBuildPhase);
                if (null == buildFile)
                {
                    throw new Opus.Core.Exception("Build file not available");
                }

                project.MainGroup.Children.AddUnique(dependentFileRef);
            }

            success = true;
            return null;
        }
    }
}
