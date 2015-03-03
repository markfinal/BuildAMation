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
    public sealed partial class NativeBuilder :
        Bam.Core.IBuilder
    {
        static
        NativeBuilder()
        {
            var level = Bam.Core.EVerboseLevel.Full;
            if (Bam.Core.State.HasCategory("NativeBuilder"))
            {
                if ((bool)Bam.Core.State.Get("NativeBuilder", "Explain"))
                {
                    level = Bam.Core.State.VerbosityLevel;
                }
            }
            Verbosity = level;
        }

        private static Bam.Core.EVerboseLevel Verbosity
        {
            get;
            set;
        }

        public static void
        MakeDirectory(
            string directory)
        {
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
                Bam.Core.Log.Message(Verbosity, "Created directory '{0}'", directory);
            }
        }

        public static bool
        RequiresBuilding(
            string outputPath,
            string inputPath)
        {
            if (Bam.Core.State.HasCategory("NativeBuilder"))
            {
                if ((bool)Bam.Core.State.Get("NativeBuilder", "ForceBuild"))
                {
                    return true;
                }
            }

            var inputFileDate = System.IO.File.GetLastWriteTime(inputPath);
            if (System.IO.File.Exists(outputPath))
            {
                var outputFileDate = System.IO.File.GetLastWriteTime(outputPath);
                if (inputFileDate.CompareTo(outputFileDate) > 0)
                {
                    Bam.Core.Log.Message(Verbosity, "Building '{1}' since input file '{0}' is newer.", inputPath, outputPath);
                    return true;
                }
            }
            else
            {
                Bam.Core.Log.Message(Verbosity, "Building '{0}' since it does not exist.", outputPath);
                return true;
            }

            return false;
        }

        public static bool
        DirectoryUpToDate(
            string destinationDir,
            string sourceDir)
        {
            if (Bam.Core.State.HasCategory("NativeBuilder"))
            {
                if ((bool)Bam.Core.State.Get("NativeBuilder", "ForceBuild"))
                {
                    return false;
                }
            }

            var inputDirDate = System.IO.Directory.GetLastWriteTime(sourceDir);
            if (System.IO.Directory.Exists(destinationDir))
            {
                var outputDirDate = System.IO.Directory.GetLastWriteTime(destinationDir);
                if (inputDirDate.CompareTo(outputDirDate) > 0)
                {
                    Bam.Core.Log.Message(Verbosity, "Building directory '{1}' since source directory '{0}' is newer.", sourceDir, destinationDir);
                    return false;
                }
            }
            else
            {
                Bam.Core.Log.Message(Verbosity, "Building directory '{0}' since it does not exist.", destinationDir);
                return false;
            }

            return true;
        }

        public static bool
        DirectoryUpToDate(
            Bam.Core.Location destinationDir,
            string sourceDir)
        {
            var destinationDirPath = destinationDir.GetSinglePath();
            return DirectoryUpToDate(destinationDirPath, sourceDir);
        }

        public enum FileRebuildStatus
        {
            AlwaysBuild,
            TimeStampOutOfDate,
            UpToDate
        }

        public static FileRebuildStatus
        IsSourceTimeStampNewer(
            Bam.Core.StringArray outputFiles,
            string inputFile)
        {
            if (Bam.Core.State.HasCategory("NativeBuilder"))
            {
                if ((bool)Bam.Core.State.Get("NativeBuilder", "ForceBuild"))
                {
                    return FileRebuildStatus.AlwaysBuild;
                }
            }

            if (0 == outputFiles.Count)
            {
                Bam.Core.Log.Message(Verbosity, "No output files - always build");
                return FileRebuildStatus.AlwaysBuild;
            }

            var newestInputFileDate = System.IO.File.GetLastWriteTime(inputFile);

            foreach (var outputFile in outputFiles)
            {
                if (System.IO.File.Exists(outputFile))
                {
                    var outputFileLastWriteTime = System.IO.File.GetLastWriteTime(outputFile);
                    if (newestInputFileDate.CompareTo(outputFileLastWriteTime) > 0)
                    {
                        Bam.Core.Log.Message(Verbosity, "Building '{1}' since '{0}' is newer.", inputFile, outputFile);
                        return FileRebuildStatus.TimeStampOutOfDate;
                    }
                }
                else
                {
                    Bam.Core.Log.Message(Verbosity, "Building '{0}' since it does not exist.", outputFile);
                    return FileRebuildStatus.AlwaysBuild;
                }
            }

            return FileRebuildStatus.UpToDate;
        }

        public static FileRebuildStatus
        IsSourceTimeStampNewer(
            Bam.Core.LocationArray outputFiles,
            Bam.Core.Location inputFile)
        {
            if (Bam.Core.State.HasCategory("NativeBuilder"))
            {
                if ((bool)Bam.Core.State.Get("NativeBuilder", "ForceBuild"))
                {
                    return FileRebuildStatus.AlwaysBuild;
                }
            }

            if (0 == outputFiles.Count)
            {
                Bam.Core.Log.Message(Verbosity, "No output files - always build");
                return FileRebuildStatus.AlwaysBuild;
            }

            var inputFilePath = inputFile.GetSinglePath();
            var newestInputFileDate = System.IO.File.GetLastWriteTime(inputFilePath);

            foreach (var outputFile in outputFiles)
            {
                var outputFilePath = outputFile.GetSinglePath();
                if (System.IO.File.Exists(outputFilePath))
                {
                    var outputFileLastWriteTime = System.IO.File.GetLastWriteTime(outputFilePath);
                    if (newestInputFileDate.CompareTo(outputFileLastWriteTime) > 0)
                    {
                        Bam.Core.Log.Message(Verbosity, "Building '{1}' since '{0}' is newer.", inputFilePath, outputFilePath);
                        return FileRebuildStatus.TimeStampOutOfDate;
                    }
                }
                else
                {
                    Bam.Core.Log.Message(Verbosity, "Building '{0}' since it does not exist.", outputFilePath);
                    return FileRebuildStatus.AlwaysBuild;
                }
            }

            return FileRebuildStatus.UpToDate;
        }

        // TODO: what if some of the paths passed in are directories? And what if they don't exist?
        public static bool
        RequiresBuilding(
            Bam.Core.StringArray outputFiles,
            Bam.Core.StringArray inputFiles)
        {
            if (Bam.Core.State.HasCategory("NativeBuilder"))
            {
                if ((bool)Bam.Core.State.Get("NativeBuilder", "ForceBuild"))
                {
                    return true;
                }
            }

            if (0 == outputFiles.Count)
            {
                Bam.Core.Log.Message(Verbosity, "No output files - always build");
                return true;
            }
            if (0 == inputFiles.Count)
            {
                Bam.Core.Log.Message(Verbosity, "No input files - always build");
                return true;
            }

            var newestInputFileDate = new System.DateTime(2000, 1, 1);
            string newestInputFile = null;
            foreach (var inputFile in inputFiles)
            {
                var inputFileLastWriteTime = System.IO.File.GetLastWriteTime(inputFile);
                if (inputFileLastWriteTime.CompareTo(newestInputFileDate) > 0)
                {
                    newestInputFileDate = inputFileLastWriteTime;
                    newestInputFile = inputFile;
                }
            }

            foreach (var outputFile in outputFiles)
            {
                if (System.IO.File.Exists(outputFile))
                {
                    var outputFileLastWriteTime = System.IO.File.GetLastWriteTime(outputFile);
                    if (newestInputFileDate.CompareTo(outputFileLastWriteTime) > 0)
                    {
                        Bam.Core.Log.Message(Verbosity, "Building '{1}' since '{0}' is newer", newestInputFile, outputFile);
                        return true;
                    }
                }
                else
                {
                    Bam.Core.Log.Message(Verbosity, "Building '{0}' since it does not exist.", outputFile);
                    return true;
                }
            }

            return false;
        }

        // TODO: what if some of the paths passed in are directories? And what if they don't exist?
        public static bool
        RequiresBuilding(
            Bam.Core.LocationArray outputFiles,
            Bam.Core.LocationArray inputFiles)
        {
            if (Bam.Core.State.HasCategory("NativeBuilder"))
            {
                if ((bool)Bam.Core.State.Get("NativeBuilder", "ForceBuild"))
                {
                    return true;
                }
            }

            if (0 == outputFiles.Count)
            {
                Bam.Core.Log.Message(Verbosity, "No output files - always build");
                return true;
            }
            if (0 == inputFiles.Count)
            {
                Bam.Core.Log.Message(Verbosity, "No input files - always build");
                return true;
            }

            var newestInputFileDate = new System.DateTime(2000, 1, 1);
            string newestInputFile = null;
            foreach (var inputFile in inputFiles)
            {
                var inputFilePath = inputFile.GetSingleRawPath();
                var inputFileLastWriteTime = System.IO.File.GetLastWriteTime(inputFilePath);
                if (inputFileLastWriteTime.CompareTo(newestInputFileDate) > 0)
                {
                    newestInputFileDate = inputFileLastWriteTime;
                    newestInputFile = inputFilePath;
                }
            }

            foreach (var outputFile in outputFiles)
            {
                var outputFilePath = outputFile.GetSingleRawPath();
                if (System.IO.File.Exists(outputFilePath))
                {
                    var outputFileLastWriteTime = System.IO.File.GetLastWriteTime(outputFilePath);
                    if (newestInputFileDate.CompareTo(outputFileLastWriteTime) > 0)
                    {
                        Bam.Core.Log.Message(Verbosity, "Building '{1}' since '{0}' is newer", newestInputFile, outputFilePath);
                        return true;
                    }
                }
                else
                {
                    Bam.Core.Log.Message(Verbosity, "Building '{0}' since it does not exist.", outputFilePath);
                    return true;
                }
            }

            return false;
        }
    }
}
