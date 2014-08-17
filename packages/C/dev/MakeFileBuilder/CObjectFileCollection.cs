#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object
        Build(
            C.ObjectFileCollectionBase moduleToBuild,
            out bool success)
        {
            var objectFileCollectionModule = moduleToBuild as Bam.Core.BaseModule;
            var node = objectFileCollectionModule.OwningNode;

            var dependents = new MakeFileVariableDictionary();
            var childDataArray = new Bam.Core.Array<MakeFileData>();
            foreach (var childNode in node.Children)
            {
                var data = childNode.Data as MakeFileData;
                if (!data.VariableDictionary.ContainsKey(C.ObjectFile.OutputFile))
                {
                    throw new Bam.Core.Exception("MakeFile Variable for '{0}' is missing", childNode.UniqueModuleName);
                }

                childDataArray.Add(data);
                dependents.Add(C.ObjectFile.OutputFile, data.VariableDictionary[C.ObjectFile.OutputFile]);
            }
            if (null != node.ExternalDependents)
            {
                var keysToFilter = new Bam.Core.Array<Bam.Core.LocationKey>(
                    C.ObjectFile.OutputFile
                );

                foreach (var dependentNode in node.ExternalDependents)
                {
                    if (null == dependentNode.Data)
                    {
                        continue;
                    }
                    var data = dependentNode.Data as MakeFileData;
                    foreach (var makeVariable in data.VariableDictionary.Filter(keysToFilter))
                    {
                        dependents.Add(makeVariable.Key, makeVariable.Value);
                    }
                }
            }

            var makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));

            var makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            // no output paths because this rule has no recipe
            var rule = new MakeFileRule(
                moduleToBuild,
                C.ObjectFile.OutputFile,
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

            using (var makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            var returnData = new MakeFileData(makeFilePath, makeFile.ExportedTargets, makeFile.ExportedVariables, null);
            success = true;
            return returnData;
        }
    }
}
