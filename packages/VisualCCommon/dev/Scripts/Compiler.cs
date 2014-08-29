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
    public sealed class CCompiler :
        C.ICompilerTool,
        Bam.Core.IToolSupportsResponseFile,
        Bam.Core.IToolForwardedEnvironmentVariables,
        Bam.Core.IToolEnvironmentVariables
    {
        public static readonly Bam.Core.LocationKey PDBFile = new Bam.Core.LocationKey("CompilerPDBFile", Bam.Core.ScaffoldLocation.ETypeHint.File);
        public static readonly Bam.Core.LocationKey PDBDir = new Bam.Core.LocationKey("CompilerPDBDir", Bam.Core.ScaffoldLocation.ETypeHint.Directory);

        private Bam.Core.IToolset toolset;
        private Bam.Core.StringArray requiredEnvironmentVariables = new Bam.Core.StringArray();

        public
        CCompiler(
            Bam.Core.IToolset toolset)
        {
            this.toolset = toolset;
            this.requiredEnvironmentVariables.Add("SystemRoot");
            // temp environment variables avoid generation of _CL_<hex> temporary files in the current directory
            this.requiredEnvironmentVariables.Add("TEMP");
            this.requiredEnvironmentVariables.Add("TMP");
        }

        #region ICompilerTool Members

        string C.ICompilerTool.PreprocessedOutputSuffix
        {
            get
            {
                return ".i";
            }
        }

        string C.ICompilerTool.ObjectFileSuffix
        {
            get
            {
                return ".obj";
            }
        }

        string C.ICompilerTool.ObjectFileOutputSubDirectory
        {
            get
            {
                return "obj";
            }
        }

        Bam.Core.StringArray
        C.ICompilerTool.IncludePaths(
            Bam.Core.BaseTarget baseTarget)
        {
            var installPath = this.toolset.InstallPath(baseTarget);
            var includePaths = new Bam.Core.StringArray();
            includePaths.Add(System.IO.Path.Combine(installPath, "include"));
            return includePaths;
        }

        Bam.Core.StringArray C.ICompilerTool.IncludePathCompilerSwitches
        {
            get
            {
                return new Bam.Core.StringArray("-I");
            }
        }

        #endregion

        #region ITool Members

        string
        Bam.Core.ITool.Executable(
            Bam.Core.BaseTarget baseTarget)
        {
            var binPath = this.toolset.BinPath(baseTarget);
            return System.IO.Path.Combine(binPath, "cl.exe");
        }

        Bam.Core.Array<Bam.Core.LocationKey>
        Bam.Core.ITool.OutputLocationKeys(
            Bam.Core.BaseModule module)
        {
            var array = new Bam.Core.Array<Bam.Core.LocationKey>(
                C.ObjectFile.OutputFile,
                C.ObjectFile.OutputDir,
                PDBFile,
                PDBDir
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
            if (baseTarget.HasPlatform(Bam.Core.EPlatform.Win64))
            {
                // some DLLs exist only in the 32-bit bin folder
                var baseTarget32 = Bam.Core.BaseTarget.GetInstance32bits(baseTarget);
                dictionary["PATH"].AddUnique(this.toolset.BinPath(baseTarget32));
            }

            var compilerTool = this as C.ICompilerTool;
            dictionary["INCLUDE"] = compilerTool.IncludePaths(baseTarget);

            return dictionary;
        }

        #endregion
    }
}
