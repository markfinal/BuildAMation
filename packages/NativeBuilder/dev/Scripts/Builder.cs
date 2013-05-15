// <copyright file="Builder.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>NativeBuilder package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder : Opus.Core.IBuilder
    {
        static NativeBuilder()
        {
            Opus.Core.EVerboseLevel level = Opus.Core.EVerboseLevel.Full;
            if (Opus.Core.State.HasCategory("NativeBuilder"))
            {
                if ((bool)Opus.Core.State.Get("NativeBuilder", "Explain"))
                {
                    level = Opus.Core.State.VerbosityLevel;
                }
            }
            Verbosity = level;
        }

        private static Opus.Core.EVerboseLevel Verbosity
        {
            get;
            set;
        }

        public static void MakeDirectory(string directory)
        {
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
                Opus.Core.Log.Message(Verbosity, "Created directory '{0}'", directory);
            }
        }

        public static bool RequiresBuilding(string outputPath, string inputPath)
        {
            if (Opus.Core.State.HasCategory("NativeBuilder"))
            {
                if ((bool)Opus.Core.State.Get("NativeBuilder", "ForceBuild"))
                {
                    return true;
                }
            }

            System.DateTime inputFileDate = System.IO.File.GetLastWriteTime(inputPath);
            if (System.IO.File.Exists(outputPath))
            {
                System.DateTime outputFileDate = System.IO.File.GetLastWriteTime(outputPath);
                if (inputFileDate.CompareTo(outputFileDate) > 0)
                {
                    Opus.Core.Log.Message(Verbosity, "Building '{1}' since input file '{0}' is newer.", inputPath, outputPath);
                    return true;
                }
            }
            else
            {
                Opus.Core.Log.Message(Verbosity, "Building '{0}' since it does not exist.", outputPath);
                return true;
            }

            return false;
        }

        public static bool DirectoryUpToDate(string destinationDir, string sourceDir)
        {
            if (Opus.Core.State.HasCategory("NativeBuilder"))
            {
                if ((bool)Opus.Core.State.Get("NativeBuilder", "ForceBuild"))
                {
                    return false;
                }
            }

            System.DateTime inputDirDate = System.IO.Directory.GetLastWriteTime(sourceDir);
            if (System.IO.Directory.Exists(destinationDir))
            {
                System.DateTime outputDirDate = System.IO.Directory.GetLastWriteTime(destinationDir);
                if (inputDirDate.CompareTo(outputDirDate) > 0)
                {
                    Opus.Core.Log.Message(Verbosity, "Building directory '{1}' since source directory '{0}' is newer.", sourceDir, destinationDir);
                    return false;
                }
            }
            else
            {
                Opus.Core.Log.Message(Verbosity, "Building directory '{0}' since it does not exist.", destinationDir);
                return false;
            }

            return true;
        }

        public enum FileRebuildStatus
        {
            AlwaysBuild,
            TimeStampOutOfDate,
            UpToDate
        }

        public static FileRebuildStatus IsSourceTimeStampNewer(Opus.Core.StringArray outputFiles, string inputFile)
        {
            if (Opus.Core.State.HasCategory("NativeBuilder"))
            {
                if ((bool)Opus.Core.State.Get("NativeBuilder", "ForceBuild"))
                {
                    return FileRebuildStatus.AlwaysBuild;
                }
            }

            if (0 == outputFiles.Count)
            {
                Opus.Core.Log.Message(Verbosity, "No output files - always build");
                return FileRebuildStatus.AlwaysBuild;
            }

            System.DateTime newestInputFileDate = System.IO.File.GetLastWriteTime(inputFile);

            foreach (string outputFile in outputFiles)
            {
                if (System.IO.File.Exists(outputFile))
                {
                    System.DateTime outputFileLastWriteTime = System.IO.File.GetLastWriteTime(outputFile);
                    if (newestInputFileDate.CompareTo(outputFileLastWriteTime) > 0)
                    {
                        Opus.Core.Log.Message(Verbosity, "Building '{1}' since '{0}' is newer.", inputFile, outputFile);
                        return FileRebuildStatus.TimeStampOutOfDate;
                    }
                }
                else
                {
                    Opus.Core.Log.Message(Verbosity, "Building '{0}' since it does not exist.", outputFile);
                    return FileRebuildStatus.AlwaysBuild;
                }
            }

            return FileRebuildStatus.UpToDate;
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
                Opus.Core.Log.Message(Verbosity, "No output files - always build");
                return true;
            }
            if (0 == inputFiles.Count)
            {
                Opus.Core.Log.Message(Verbosity, "No input files - always build");
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
                        Opus.Core.Log.Message(Verbosity, "Building '{1}' since '{0}' is newer", newestInputFile, outputFile);
                        return true;
                    }
                }
                else
                {
                    Opus.Core.Log.Message(Verbosity, "Building '{0}' since it does not exist.", outputFile);
                    return true;
                }
            }

            return false;
        }
    }
}
