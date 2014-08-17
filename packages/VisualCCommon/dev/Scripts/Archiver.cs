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
namespace VisualCCommon
{
    public sealed class Archiver :
        C.IArchiverTool,
        Bam.Core.IToolSupportsResponseFile,
        Bam.Core.IToolForwardedEnvironmentVariables,
        Bam.Core.IToolEnvironmentVariables
    {
        private Bam.Core.IToolset toolset;
        private Bam.Core.StringArray requiredEnvironmentVariables = new Bam.Core.StringArray();

        public
        Archiver(
            Bam.Core.IToolset toolset)
        {
            this.toolset = toolset;
            this.requiredEnvironmentVariables.Add("SystemRoot");
            // temp environment variables avoid generation of _CL_<hex> temporary files in the current directory
            this.requiredEnvironmentVariables.Add("TEMP");
            this.requiredEnvironmentVariables.Add("TMP");
        }

        #region IArchiverTool Members

        string C.IArchiverTool.StaticLibraryPrefix
        {
            get
            {
                return string.Empty;
            }
        }

        string C.IArchiverTool.StaticLibrarySuffix
        {
            get
            {
                return ".lib";
            }
        }

        string C.IArchiverTool.StaticLibraryOutputSubDirectory
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
            return System.IO.Path.Combine(binPath, "lib.exe");
        }

        Bam.Core.Array<Bam.Core.LocationKey>
        Bam.Core.ITool.OutputLocationKeys(
            Bam.Core.BaseModule module)
        {
            var array = new Bam.Core.Array<Bam.Core.LocationKey>(
                C.StaticLibrary.OutputFileLocKey,
                C.StaticLibrary.OutputDirLocKey
                );
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
            var dictionary = new System.Collections.Generic.Dictionary<string, Bam.Core.StringArray>();
            dictionary["PATH"] = this.toolset.Environment;
            return dictionary;
        }

        #endregion
    }
}
