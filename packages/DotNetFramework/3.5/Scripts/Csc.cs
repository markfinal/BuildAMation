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
namespace DotNetFramework
{
    public sealed class Csc :
        CSharp.ICSharpCompilerTool,
        Bam.Core.IToolForwardedEnvironmentVariables
    {
        private Bam.Core.IToolset toolset;

        public
        Csc(
            Bam.Core.IToolset toolset)
        {
            this.toolset = toolset;
        }

        #region ITool Members

        string
        Bam.Core.ITool.Executable(
            Bam.Core.BaseTarget baseTarget)
        {
            string CscPath = null;

            var installPath = this.toolset.InstallPath(baseTarget);
            if (Bam.Core.OSUtilities.IsWindowsHosting)
            {
                CscPath = System.IO.Path.Combine(installPath, "Csc.exe");
            }
            else if (Bam.Core.OSUtilities.IsUnixHosting)
            {
                CscPath = System.IO.Path.Combine(installPath, "mono-csc");
            }
            else if (Bam.Core.OSUtilities.IsOSXHosting)
            {
                CscPath = System.IO.Path.Combine(installPath, "mcs");
            }

            return CscPath;
        }

        Bam.Core.Array<Bam.Core.LocationKey>
        Bam.Core.ITool.OutputLocationKeys(
            Bam.Core.BaseModule module)
        {
            var array = new Bam.Core.Array<Bam.Core.LocationKey>(
                CSharp.Assembly.OutputFile,
                CSharp.Assembly.OutputDir,
                CSharp.Assembly.PDBFile,
                CSharp.Assembly.PDBDir
                );
            return array;
        }

        #endregion

        #region IToolForwardedEnvironmentVariables Members

        Bam.Core.StringArray Bam.Core.IToolForwardedEnvironmentVariables.VariableNames
        {
            get
            {
                var envVars = new Bam.Core.StringArray();
                envVars.Add("SystemRoot");
                envVars.Add("TEMP"); // otherwise you get errors like this CS0016: Could not write to output file 'd:\build\CSharpTest1-dev\SimpleAssembly\win32-dotnet2.0.50727-debug\bin\SimpleAssembly.dll' -- 'Access is denied.
                return envVars;
            }
        }

        #endregion
    }
}
