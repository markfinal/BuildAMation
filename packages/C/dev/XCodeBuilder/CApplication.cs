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

            success = true;
            return data;
        }
    }
}
