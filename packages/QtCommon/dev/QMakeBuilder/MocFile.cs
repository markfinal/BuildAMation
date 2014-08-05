// <copyright file="MocFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public partial class QMakeBuilder
    {
        public object
        Build(
            QtCommon.MocFile moduleToBuild,
            out System.Boolean success)
        {
            var sourceFilePath = moduleToBuild.SourceFileLocation.GetSinglePath();
            var node = moduleToBuild.OwningNode;

            var data = new QMakeData(node);
            data.PriPaths.Add(this.EmptyConfigPriPath);
            data.Headers.Add(sourceFilePath);
            data.Output = QMakeData.OutputType.MocFile;
            data.MocDir = moduleToBuild.Locations[QtCommon.MocFile.OutputDir];

            success = true;
            return data;
        }
    }
}
