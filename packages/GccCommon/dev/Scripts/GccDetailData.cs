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
    public class GccDetailData
    {
        public
        GccDetailData(
            string version,
            Bam.Core.StringArray includePaths,
            string gxxIncludePath,
            string target,
            string libExecDir)
        {
            if (null == version)
            {
                throw new Bam.Core.Exception("Unable to determine Gcc version");
            }
            if (null == target)
            {
                throw new Bam.Core.Exception("Unable to determine Gcc target");
            }

            this.Version = version;
            this.IncludePaths = includePaths;
            this.GxxIncludePath = gxxIncludePath;
            this.Target = target;
            this.LibExecDir = libExecDir;
        }

        public string Version
        {
            get;
            private set;
        }

        public Bam.Core.StringArray IncludePaths
        {
            get;
            private set;
        }

        public string GxxIncludePath
        {
            get;
            private set;
        }

        public string Target
        {
            get;
            private set;
        }

        public string LibExecDir
        {
            get;
            private set;
        }
    }
}
