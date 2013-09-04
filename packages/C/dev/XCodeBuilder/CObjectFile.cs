// <copyright file="CObjectFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public sealed partial class XCodeBuilder
    {
        public object Build(C.ObjectFile moduleToBuild, out bool success)
        {
            Opus.Core.Log.MessageAll("ObjectFile");
            var sourceFile = moduleToBuild.SourceFile.AbsolutePath;
            var fileRef = new PBXFileReference(moduleToBuild.OwningNode.ModuleName, sourceFile);
            fileRef.IsSourceCode = true;
            this.Project.FileReferences.Add(fileRef);

            var target = moduleToBuild.OwningNode.Target;
            var baseTarget = (Opus.Core.BaseTarget)target;
            var buildConfiguration = this.Project.BuildConfigurations.Get(baseTarget.ConfigurationName('='));

            var configurationList = this.Project.ConfigurationLists.Get(baseTarget.ConfigurationName('='));
            configurationList.AddUnique(buildConfiguration);

            var data = new PBXBuildFile(moduleToBuild.OwningNode.ModuleName);
            data.FileReference = fileRef;
            this.Project.BuildFiles.Add(data);
            success = true;
            return data;
        }
    }
}
