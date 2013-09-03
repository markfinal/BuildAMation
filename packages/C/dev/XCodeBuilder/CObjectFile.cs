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
            var sourceFile = moduleToBuild.SourceFile.AbsolutePath;
            var fileRef = new PBXFileReference(moduleToBuild.OwningNode.ModuleName, sourceFile);
            fileRef.IsSourceCode = true;
            this.Project.FileReferences.Add(fileRef);

            var data = new PBXBuildFile(moduleToBuild.OwningNode.ModuleName);
            data.FileReference = fileRef;
            this.Project.BuildFiles.Add(data);
            success = true;
            return data;
        }
    }
}
