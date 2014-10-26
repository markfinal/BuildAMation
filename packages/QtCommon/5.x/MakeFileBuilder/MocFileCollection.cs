#region License
// <copyright>
//  Mark Final
// </copyright>
// <author>Mark Final</author>
#endregion // License
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object
        Build(
            QtCommon.MocFileCollection moduleToBuild,
            out bool success)
        {
            var mocFileCollectionModule = moduleToBuild as Bam.Core.BaseModule;
            var node = mocFileCollectionModule.OwningNode;

            var dependents = new MakeFileVariableDictionary();
            foreach (var childNode in node.Children)
            {
                var data = childNode.Data as MakeFileData;
                // TODO: handle this better for more dependents
                dependents.Append(data.VariableDictionary);
            }

            var makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));
            Bam.Core.Log.DebugMessage("Makefile : '{0}'", makeFilePath);

            var makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            var rule = new MakeFileRule(
                moduleToBuild,
                QtCommon.MocFile.OutputFile,
                node.UniqueModuleName,
                null,
                dependents,
                null,
                null);
            if (null == node.Parent)
            {
                // phony target
                rule.TargetIsPhony = true;
            }
            makeFile.RuleArray.Add(rule);

            using (System.IO.TextWriter makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            success = true;

            var returnData = new MakeFileData(makeFilePath, makeFile.ExportedTargets, makeFile.ExportedVariables, null);
            return returnData;
        }
    }
}
