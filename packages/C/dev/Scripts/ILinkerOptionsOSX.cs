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
namespace C
{
    public interface ILinkerOptionsOSX
    {
        /// <summary>
        /// List of names of OSX frameworks to include in the link step
        /// </summary>
        /// <value>The OSX frameworks.</value>
        Bam.Core.StringArray Frameworks
        {
            get;
            set;
        }

        /// <summary>
        /// List of directories the linker searches for Frameworks
        /// </summary>
        /// <value>The OSX frameworks.</value>
        Bam.Core.DirectoryCollection FrameworkSearchDirectories
        {
            get;
            set;
        }

        /// <summary>
        /// Suppress read only relocations
        /// </summary>
        /// <value><c>true</c> if read only relocations; otherwise, <c>false</c>.</value>
        bool SuppressReadOnlyRelocations
        {
            get;
            set;
        }
    }
}
