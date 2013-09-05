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
            Opus.Core.Log.MessageAll("ObjectFile {0}", moduleToBuild.OwningNode.ModuleName);
            var sourceFile = moduleToBuild.SourceFile.AbsolutePath;
            var fileRef = new PBXFileReference(moduleToBuild.OwningNode.ModuleName, sourceFile, this.RootUri);
            fileRef.IsSourceCode = true;
            this.Project.FileReferences.Add(fileRef);

            var target = moduleToBuild.OwningNode.Target;
            var baseTarget = (Opus.Core.BaseTarget)target;

            // TODO: this has to be separate from the project configuration
            /*var buildConfiguration = */this.Project.BuildConfigurations.Get(baseTarget.ConfigurationName('='), moduleToBuild.OwningNode.ModuleName);
            // TODO: more will happen with configurations once we start to add options to it

            var data = new PBXBuildFile(moduleToBuild.OwningNode.ModuleName);
            data.FileReference = fileRef;
            this.Project.BuildFiles.Add(data);

            var sourcesBuildPhase = this.Project.SourceBuildPhases.Get("Sources", moduleToBuild.OwningNode.ModuleName);
            sourcesBuildPhase.Files.Add(data);
            data.BuildPhase = sourcesBuildPhase;

            success = true;
            return data;
        }
    }
}
