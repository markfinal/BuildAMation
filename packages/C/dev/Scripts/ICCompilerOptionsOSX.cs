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
    public interface ICCompilerOptionsOSX
    {
        /// <summary>
        /// List of directories the compiler searches for Frameworks
        /// </summary>
        /// <value>The OSX frameworks.</value>
        Bam.Core.DirectoryCollection FrameworkSearchDirectories
        {
            get;
            set;
        }

        /// <summary>
        /// OSX SDK version used to compile against
        /// </summary>
        /// <value>The OSX SDK version.</value>
        string SDKVersion
        {
            get;
            set;
        }

        /// <summary>
        /// The minimum version of OSX required in order to run this code
        /// </summary>
        /// <value>The deployment target.</value>
        string DeploymentTarget
        {
            get;
            set;
        }

        /// <summary>
        /// OSX builds can target either MacOSX or iOS
        /// </summary>
        /// <value>The supported platform.</value>
        C.EOSXPlatform SupportedPlatform
        {
            get;
            set;
        }

        /// <summary>
        /// Name of the compiler, in reverse DNS form. Usually not required to be set.
        /// </summary>
        /// <value>The name of the compiler.</value>
        string CompilerName
        {
            get;
            set;
        }
    }
}
