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
#endregion // License
namespace VisualCCommon
{
    public sealed class Linker :
        C.ILinkerTool,
        C.IWinImportLibrary,
        Bam.Core.IToolSupportsResponseFile,
        Bam.Core.IToolForwardedEnvironmentVariables,
        Bam.Core.IToolEnvironmentVariables
    {
        public static readonly Bam.Core.LocationKey PDBFile = new Bam.Core.LocationKey("LinkerPDBFile", Bam.Core.ScaffoldLocation.ETypeHint.File);
        public static readonly Bam.Core.LocationKey PDBDir = new Bam.Core.LocationKey("LinkerPDBDir", Bam.Core.ScaffoldLocation.ETypeHint.Directory);

        private Bam.Core.IToolset toolset;
        private Bam.Core.StringArray requiredEnvironmentVariables = new Bam.Core.StringArray();

        public
        Linker(
            Bam.Core.IToolset toolset)
        {
            this.toolset = toolset;
            // temp environment variables avoid generation of _CL_<hex> temporary files in the current directory
            this.requiredEnvironmentVariables.Add("TEMP");
            this.requiredEnvironmentVariables.Add("TMP");
        }

        #region ILinkerTool Members

        string C.ILinkerTool.ExecutableSuffix
        {
            get
            {
                return ".exe";
            }
        }

        string C.ILinkerTool.MapFileSuffix
        {
            get
            {
                return ".map";
            }
        }

        string C.ILinkerTool.StartLibraryList
        {
            get
            {
                return string.Empty;
            }
        }

        string C.ILinkerTool.EndLibraryList
        {
            get
            {
                return string.Empty;
            }
        }

        string C.ILinkerTool.DynamicLibraryPrefix
        {
            get
            {
                return string.Empty;
            }
        }

        string C.ILinkerTool.DynamicLibrarySuffix
        {
            get
            {
                return ".dll";
            }
        }

        string C.ILinkerTool.BinaryOutputSubDirectory
        {
            get
            {
                return "bin";
            }
        }

        Bam.Core.StringArray
        C.ILinkerTool.LibPaths(
            Bam.Core.BaseTarget baseTarget)
        {
            if (baseTarget.HasPlatform(Bam.Core.EPlatform.Win64))
            {
                return (this.toolset as VisualCCommon.Toolset).lib64Folder;
            }
            else
            {
                return (this.toolset as VisualCCommon.Toolset).lib32Folder;
            }
        }

        #endregion

        #region C.IWinImportLibrary Members

        string C.IWinImportLibrary.ImportLibraryPrefix
        {
            get
            {
                return string.Empty;
            }
        }

        string C.IWinImportLibrary.ImportLibrarySuffix
        {
            get
            {
                return ".lib";
            }
        }

        string C.IWinImportLibrary.ImportLibrarySubDirectory
        {
            get
            {
                return "lib";
            }
        }

        #endregion

        #region ITool Members

        string
        Bam.Core.ITool.Executable(
            Bam.Core.BaseTarget baseTarget)
        {
            var binPath = this.toolset.BinPath(baseTarget);
            return System.IO.Path.Combine(binPath, "link.exe");
        }

        Bam.Core.Array<Bam.Core.LocationKey>
        Bam.Core.ITool.OutputLocationKeys(
            Bam.Core.BaseModule module)
        {
            var array = new Bam.Core.Array<Bam.Core.LocationKey>(
                C.Application.OutputFile,
                C.Application.OutputDir,
                C.Application.MapFile,
                C.Application.MapFileDir,
                PDBFile,
                PDBDir
                );
            if (module is C.DynamicLibrary)
            {
                array.AddRange(new [] {
                    C.DynamicLibrary.ImportLibraryDir,
                    C.DynamicLibrary.ImportLibraryFile});
            }
            return array;
        }

        #endregion

        #region IToolSupportsResponseFile Members

        string Bam.Core.IToolSupportsResponseFile.Option
        {
            get
            {
                return "@";
            }
        }

        #endregion

        #region IToolForwardedEnvironmentVariables Members

        Bam.Core.StringArray Bam.Core.IToolForwardedEnvironmentVariables.VariableNames
        {
            get
            {
                return this.requiredEnvironmentVariables;
            }
        }

        #endregion

        #region IToolEnvironmentVariables Members

        System.Collections.Generic.Dictionary<string, Bam.Core.StringArray>
        Bam.Core.IToolEnvironmentVariables.Variables(
            Bam.Core.BaseTarget baseTarget)
        {
            var environmentVariables = new System.Collections.Generic.Dictionary<string, Bam.Core.StringArray>();
            environmentVariables["LIB"] = (this as C.ILinkerTool).LibPaths(baseTarget);
            environmentVariables["PATH"] = this.toolset.Environment;
            if (baseTarget.HasPlatform(Bam.Core.EPlatform.Win64))
            {
                // some DLLs exist only in the 32-bit bin folder
                var baseTarget32 = Bam.Core.BaseTarget.GetInstance32bits(baseTarget);
                environmentVariables["PATH"].AddUnique(this.toolset.BinPath(baseTarget32));
            }
            return environmentVariables;
        }

        #endregion
    }
}
