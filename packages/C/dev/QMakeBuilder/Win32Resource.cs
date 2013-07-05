// <copyright file="Win32Resource.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder2
    {
        public object Build(C.Win32Resource moduleToBuild, out bool success)
        {
            var sourceFilePath = moduleToBuild.ResourceFile.AbsolutePath;
            var node = moduleToBuild.OwningNode;
            var options = moduleToBuild.Options as C.Win32ResourceCompilerOptionCollection;

            var data = new QMakeData(node);
            data.PriPaths.Add(this.EmptyConfigPriPath);
            data.WinRCFiles.Add(sourceFilePath);
            data.Output = QMakeData.OutputType.WinResource;
            data.DestDir = options.OutputDirectoryPath;

            success = true;
            return data;
        }
    }

    public sealed partial class QMakeBuilder
    {
        public object Build(C.Win32Resource resourceFile, out bool success)
        {
            var resourceFilePath = resourceFile.ResourceFile.AbsolutePath;
            if (!System.IO.File.Exists(resourceFilePath))
            {
                throw new Opus.Core.Exception("Resource file '{0}' does not exist", resourceFilePath);
            }

            Opus.Core.Log.MessageAll("TODO: Stub function for QMake support for {0}", resourceFilePath);
            // TODO: add to RC_FILE
            // what about RC options?

            success = true;
            return null;
        }
    }
}