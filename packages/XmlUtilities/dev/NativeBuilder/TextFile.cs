#region License
// Copyright 2010-2015 Mark Final
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
#endregion // License
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object
        Build(
            XmlUtilities.TextFileModule moduleToBuild,
            out bool success)
        {
            var node = moduleToBuild.OwningNode;

            var outputLoc = moduleToBuild.Locations[XmlUtilities.TextFileModule.OutputFile];
            var outputPath = outputLoc.GetSinglePath();
            if (null == outputPath)
            {
                throw new Bam.Core.Exception("Text output path was not set");
            }

            // dependency checking
            {
                var outputFiles = new Bam.Core.StringArray();
                outputFiles.Add(outputPath);
                if (!RequiresBuilding(outputFiles, new Bam.Core.StringArray()))
                {
                    Bam.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                    success = true;
                    return null;
                }
            }

            Bam.Core.Log.Info("Writing text file '{0}'", outputPath);

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Bam.Core.ScaffoldLocation.ETypeHint.Directory, Bam.Core.Location.EExists.WillExist);
            foreach (var dir in dirsToCreate)
            {
                var dirPath = dir.GetSinglePath();
                NativeBuilder.MakeDirectory(dirPath);
            }

            using (var writer = new System.IO.StreamWriter(outputPath, false, System.Text.Encoding.ASCII))
            {
                var content = moduleToBuild.Content.ToString();
                writer.Write(content);
            }

            success = true;
            return null;
        }
    }
}
