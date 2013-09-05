// <copyright file="CApplication.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public sealed partial class XCodeBuilder
    {
        public object Build(C.Application moduleToBuild, out bool success)
        {
            Opus.Core.Log.MessageAll("Application {0}", moduleToBuild.OwningNode.ModuleName);
            var options = moduleToBuild.Options as C.LinkerOptionCollection;
            var outputPath = options.OutputPaths[C.OutputFileFlags.Executable];

            var fileRef = new PBXFileReference(moduleToBuild.OwningNode.ModuleName, outputPath);
            fileRef.IsExecutable = true;
            //this.Project.FileReferences.Add(fileRef);
            // TODO: intentionally add this to the front for consistency with XCode generated projects
            this.Project.FileReferences.Insert(fileRef, 0);
            this.Project.ProductsGroup.Children.Add(fileRef);

            var data = new PBXNativeTarget(moduleToBuild.OwningNode.ModuleName);
            data.ProductReference = fileRef;
            this.Project.NativeTargets.Add(data);

            var target = moduleToBuild.OwningNode.Target;
            var baseTarget = (Opus.Core.BaseTarget)target;
            var buildConfiguration = this.Project.BuildConfigurations.Get(baseTarget.ConfigurationName('='), moduleToBuild.OwningNode.ModuleName);

            // adding the configuration list for the PBXNativeTarget
            var nativeTargetConfigurationList = this.Project.ConfigurationLists.Get(baseTarget.ConfigurationName('='), data);
            nativeTargetConfigurationList.AddUnique(buildConfiguration);
            data.BuildConfigurationList = nativeTargetConfigurationList;

            // adding the group for the target
            var group = new PBXGroup(moduleToBuild.OwningNode.ModuleName);
            group.SourceTree = "<group>";
            group.Path = moduleToBuild.OwningNode.ModuleName;
            foreach (var source in moduleToBuild.OwningNode.Children)
            {
                if (source.Module is Opus.Core.IModuleCollection)
                {
                    Opus.Core.Log.MessageAll("Collective source; {0}", source.UniqueModuleName);
                    foreach (var source2 in source.Children)
                    {
                        Opus.Core.Log.MessageAll("\tsource; {0}", source2.UniqueModuleName);
                        var sourceData = source2.Data as XCodeNodeData;
                        Opus.Core.Log.MessageAll("\t{0}", sourceData.Name);
                        group.Children.Add(sourceData);
                    }
                }
                else
                {
                    Opus.Core.Log.MessageAll("source; {0}", source.UniqueModuleName);
                    var sourceData = source.Data as XCodeNodeData;
                    group.Children.Add(sourceData);
                }
            }
            this.Project.Groups.Add(group);
            this.Project.MainGroup.Children.Add(group);

            var sourcesBuildPhase = this.Project.SourceBuildPhases.Get("Sources", moduleToBuild.OwningNode.ModuleName);
            data.BuildPhases.Add(sourcesBuildPhase);

            var copyFilesBuildPhase = this.Project.CopyFilesBuildPhases.Get("CopyFiles", moduleToBuild.OwningNode.ModuleName);
            data.BuildPhases.Add(copyFilesBuildPhase);

            var frameworksBuildPhase = this.Project.FrameworksBuildPhases.Get("Frameworks", moduleToBuild.OwningNode.ModuleName);
            data.BuildPhases.Add(frameworksBuildPhase);

            success = true;
            return data;
        }
    }
}
