// <copyright file="TextFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
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
