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
namespace QtCommon
{
    public sealed class MocTool :
        IMocTool
    {
        private Bam.Core.IToolset toolset;

        public
        MocTool(
            Bam.Core.IToolset toolset)
        {
            this.toolset = toolset;
        }

        #region ITool Members

        string
        Bam.Core.ITool.Executable(
            Bam.Core.BaseTarget baseTarget)
        {
            var mocExePath = System.IO.Path.Combine(this.toolset.BinPath(baseTarget), "moc");
            if (baseTarget.HasPlatform(Bam.Core.EPlatform.Windows))
            {
                mocExePath += ".exe";
            }

            return mocExePath;
        }

        Bam.Core.Array<Bam.Core.LocationKey>
        Bam.Core.ITool.OutputLocationKeys(
            Bam.Core.BaseModule module)
        {
            var array = new Bam.Core.Array<Bam.Core.LocationKey>(
                MocFile.OutputFile,
                MocFile.OutputDir
                );
            return array;
        }

        #endregion
    }
}
