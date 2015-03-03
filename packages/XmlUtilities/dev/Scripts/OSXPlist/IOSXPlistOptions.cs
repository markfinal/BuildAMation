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
namespace XmlUtilities
{
    public interface IOSXPlistOptions
    {
        [Bam.Core.ValueOnlyOption]
        string CFBundleName
        {
            get;
            set;
        }

        [Bam.Core.ValueOnlyOption]
        string CFBundleDisplayName
        {
            get;
            set;
        }

        [Bam.Core.ValueOnlyOption]
        string CFBundleIdentifier
        {
            get;
            set;
        }

        [Bam.Core.ValueOnlyOption]
        string CFBundleVersion
        {
            get;
            set;
        }

        [Bam.Core.ValueOnlyOption]
        string CFBundleSignature
        {
            get;
            set;
        }

        [Bam.Core.ValueOnlyOption]
        string CFBundleExecutable
        {
            get;
            set;
        }

        [Bam.Core.ValueOnlyOption]
        string CFBundleShortVersionString
        {
            get;
            set;
        }

        [Bam.Core.ValueOnlyOption]
        string LSMinimumSystemVersion
        {
            get;
            set;
        }

        [Bam.Core.ValueOnlyOption]
        string NSHumanReadableCopyright
        {
            get;
            set;
        }

        [Bam.Core.ValueOnlyOption]
        string NSMainNibFile
        {
            get;
            set;
        }

        [Bam.Core.ValueOnlyOption]
        string NSPrincipalClass
        {
            get;
            set;
        }
    }
}
