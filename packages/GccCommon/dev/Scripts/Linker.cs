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
namespace GccCommon
{
    public abstract class Linker :
        C.ILinkerTool,
        Bam.Core.IToolEnvironmentVariables
    {
        protected Bam.Core.IToolset toolset;
        private Bam.Core.StringArray environment;

        protected
        Linker(
            Bam.Core.IToolset toolset)
        {
            this.toolset = toolset;
            this.environment = new Bam.Core.StringArray("/usr/bin");
        }

        protected abstract string Filename
        {
            get;
        }

        #region ILinkerTool Members

        string C.ILinkerTool.ExecutableSuffix
        {
            get
            {
                return string.Empty;
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
                if (Bam.Core.OSUtilities.IsUnixHosting)
                {
                    return "-Wl,--start-group";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        string C.ILinkerTool.EndLibraryList
        {
            get
            {
                if (Bam.Core.OSUtilities.IsUnixHosting)
                {
                    return "-Wl,--end-group";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        string C.ILinkerTool.DynamicLibraryPrefix
        {
            get
            {
                return "lib";
            }
        }

        string C.ILinkerTool.DynamicLibrarySuffix
        {
            get
            {
                if (Bam.Core.OSUtilities.IsUnixHosting)
                {
                    return ".so";
                }
                else if (Bam.Core.OSUtilities.IsOSXHosting)
                {
                    return ".dylib";
                }
                else
                {
                    return null;
                }
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
            throw new System.NotImplementedException();
        }

        #endregion

        #region ITool Members

        string
        Bam.Core.ITool.Executable(
            Bam.Core.BaseTarget baseTarget)
        {
            var installPath = this.toolset.BinPath(baseTarget);
            var executablePath = System.IO.Path.Combine(installPath, this.Filename);
            return executablePath;
        }

        Bam.Core.Array<Bam.Core.LocationKey>
        Bam.Core.ITool.OutputLocationKeys(
            Bam.Core.BaseModule module)
        {
            var array = new Bam.Core.Array<Bam.Core.LocationKey>(
                C.Application.OutputFile,
                C.Application.OutputDir,
                C.Application.MapFile,
                C.Application.MapFileDir);
            if (module is C.DynamicLibrary)
            {
                array.AddRange(new [] {
                    C.DynamicLibrary.ImportLibraryFile,
                    C.DynamicLibrary.ImportLibraryDir});
            }
            return array;
        }

        #endregion

        #region IToolEnvironmentVariables implementation
        System.Collections.Generic.Dictionary<string, Bam.Core.StringArray>
        Bam.Core.IToolEnvironmentVariables.Variables(
            Bam.Core.BaseTarget baseTarget)
        {
            var dictionary = new System.Collections.Generic.Dictionary<string, Bam.Core.StringArray>();
            dictionary["PATH"] = this.environment;
            return dictionary;
        }
        #endregion
    }
}
