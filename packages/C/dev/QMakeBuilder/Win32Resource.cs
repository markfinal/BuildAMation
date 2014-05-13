// <copyright file="Win32Resource.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        public object Build(C.Win32Resource moduleToBuild, out bool success)
        {
            var sourceFilePath = moduleToBuild.ResourceFileLocation;
            var node = moduleToBuild.OwningNode;
            var options = moduleToBuild.Options as C.Win32ResourceCompilerOptionCollection;

            var data = new QMakeData(node);
            data.PriPaths.Add(this.EmptyConfigPriPath);
            data.WinRCFiles.Add(sourceFilePath.GetSinglePath());
            data.Output = QMakeData.OutputType.WinResource;
#if true
            data.DestDir = moduleToBuild.Locations[C.Win32Resource.OutputDirLKey];
#else
            data.DestDir = options.OutputDirectoryPath;
#endif

            success = true;
            return data;
        }
    }
}
