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
namespace XmlUtilities
{
    public interface IOSXPlistOptions
    {
        // StateOnly
        string CFBundleName
        {
            get;
            set;
        }

        // StateOnly
        string CFBundleDisplayName
        {
            get;
            set;
        }

        // StateOnly
        string CFBundleIdentifier
        {
            get;
            set;
        }

        // StateOnly
        string CFBundleVersion
        {
            get;
            set;
        }

        // StateOnly
        string CFBundleSignature
        {
            get;
            set;
        }

        // StateOnly
        string CFBundleExecutable
        {
            get;
            set;
        }

        // StateOnly
        string CFBundleShortVersionString
        {
            get;
            set;
        }

        // StateOnly
        string LSMinimumSystemVersion
        {
            get;
            set;
        }

        // StateOnly
        string NSHumanReadableCopyright
        {
            get;
            set;
        }

        // StateOnly
        string NSMainNibFile
        {
            get;
            set;
        }

        // StateOnly
        string NSPrincipalClass
        {
            get;
            set;
        }
    }
}
