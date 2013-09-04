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
            Opus.Core.Log.MessageAll("Application");
            var options = moduleToBuild.Options as C.LinkerOptionCollection;
            var outputPath = options.OutputPaths[C.OutputFileFlags.Executable];

            var fileRef = new PBXFileReference(moduleToBuild.OwningNode.ModuleName, outputPath);
            fileRef.IsExecutable = true;
            this.Project.FileReferences.Add(fileRef);

            var data = new PBXNativeTarget(moduleToBuild.OwningNode.ModuleName);
            data.ProductReference = fileRef;
            this.Project.NativeTargets.Add(data);

            var target = moduleToBuild.OwningNode.Target;
            var baseTarget = (Opus.Core.BaseTarget)target;
            var buildConfiguration = this.Project.BuildConfigurations.Get(baseTarget.ConfigurationName('='));

            // adding the configuration list for the PBXNativeTarget
            var nativeTargetConfigurationList = this.Project.ConfigurationLists.Get(baseTarget.ConfigurationName('='), data);
            nativeTargetConfigurationList.AddUnique(buildConfiguration);
            data.BuildConfigurationList = nativeTargetConfigurationList;

            success = true;
            return data;
        }
    }
}
