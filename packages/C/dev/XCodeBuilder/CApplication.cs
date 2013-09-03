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
            var fileRef = new PBXFileReference(moduleToBuild.OwningNode.ModuleName);
            this.Project.FileReferences.AddFileRef(fileRef);

            var data = new PBXNativeTarget(moduleToBuild.OwningNode.ModuleName);
            data.ProductReference = fileRef;
            this.Project.AddNativeTarget(data);

            success = true;
            return data;
        }
    }
}
