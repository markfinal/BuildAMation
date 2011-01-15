// <copyright file="NativeBuilder.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>NativeBuilder package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.DeclareBuilder("Native", typeof(NativeBuilder.NativeBuilder))]

namespace NativeBuilder
{
    public sealed partial class NativeBuilder : Opus.Core.IBuilder
    {
        public static void MakeDirectory(string directory)
        {
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
                Opus.Core.Log.Detail("Created directory '{0}'", directory);
            }
        }

        // TODO: what if some of the paths passed in are directories? And what if they don't exist?
        public static bool RequiresBuilding(Opus.Core.StringArray outputFiles, Opus.Core.StringArray inputFiles)
        {
            if (Opus.Core.State.HasCategory("NativeBuilder"))
            {
                if ((bool)Opus.Core.State.Get("NativeBuilder", "ForceBuild"))
                {
                    return true;
                }
            }

            if (0 == outputFiles.Count)
            {
                Opus.Core.Log.DebugMessage("No output files - always build");
                return true;
            }
            if (0 == inputFiles.Count)
            {
                Opus.Core.Log.DebugMessage("No input files - always build");
                return true;
            }

            System.DateTime newestInputFileDate = new System.DateTime(2000, 1, 1);
            string newestInputFile = null;
            foreach (string inputFile in inputFiles)
            {
                System.DateTime inputFileLastWriteTime = System.IO.File.GetLastWriteTime(inputFile);
                if (inputFileLastWriteTime.CompareTo(newestInputFileDate) > 0)
                {
                    newestInputFileDate = inputFileLastWriteTime;
                    newestInputFile = inputFile;
                }
            }

            foreach (string outputFile in outputFiles)
            {
                if (System.IO.File.Exists(outputFile))
                {
                    System.DateTime outputFileLastWriteTime = System.IO.File.GetLastWriteTime(outputFile);
                    if (newestInputFileDate.CompareTo(outputFileLastWriteTime) > 0)
                    {
                        Opus.Core.Log.DebugMessage("Input file '{0}' is newer than output file '{1}'. Requires build.", newestInputFile, outputFile);
                        return true;
                    }
                }
                else
                {
                    Opus.Core.Log.DebugMessage("Output file '{0}' does not exist. Requires build.", outputFile);
                    return true;
                }
            }

            return false;
        }
    }
}
