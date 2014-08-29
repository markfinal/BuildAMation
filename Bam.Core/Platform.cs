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
namespace Bam.Core
{
    public static class Platform
    {
        public static EPlatform
        FromString(
            string platformName)
        {
            var platform = EPlatform.Invalid;
            if (0 == System.String.Compare(platformName, "Windows", true))
            {
                platform = EPlatform.Windows;
            }
            else if (0 == System.String.Compare(platformName, "Win32", true))
            {
                platform = EPlatform.Win32;
            }
            else if (0 == System.String.Compare(platformName, "Win64", true))
            {
                platform = EPlatform.Win64;
            }
            else if (0 == System.String.Compare(platformName, "Unix", true))
            {
                platform = EPlatform.Unix;
            }
            else if (0 == System.String.Compare(platformName, "Unix32", true))
            {
                platform = EPlatform.Unix32;
            }
            else if (0 == System.String.Compare(platformName, "Unix64", true))
            {
                platform = EPlatform.Unix64;
            }
            else if (0 == System.String.Compare(platformName, "OSX", true))
            {
                platform = EPlatform.OSX;
            }
            else if (0 == System.String.Compare(platformName, "OSX32", true))
            {
                platform = EPlatform.OSX32;
            }
            else if (0 == System.String.Compare(platformName, "OSX64", true))
            {
                platform = EPlatform.OSX64;
            }
            else
            {
                throw new Exception("Platform name '{0}' not recognized", platformName);
            }
            return platform;
        }

        private static void
        AddPlatformName(
            ref string platformString,
            string name,
            char separator,
            string prefix,
            bool toUpper)
        {
            if (null != platformString)
            {
                if ('\0' == separator)
                {
                    throw new Exception("No separator was defined for a multi platform value");
                }
                platformString += separator;
            }
            if (null != prefix)
            {
                if (toUpper)
                {
                    prefix = prefix.ToUpper();
                }
                platformString += prefix;
            }
            if (toUpper)
            {
                name = name.ToUpper();
            }
            platformString += name;
        }

        public static bool
        Contains(
            EPlatform flags,
            EPlatform specificPlatform)
        {
            var contains = (specificPlatform == (flags & specificPlatform));
            return contains;
        }

        public static string
        ToString(
            EPlatform platformFlags)
        {
            var value = ToString(platformFlags, '\0', null, false);
            return value;
        }

        public static string
        ToString(
            EPlatform platformFlags,
            char separator)
        {
            var value = ToString(platformFlags, separator, null, false);
            return value;
        }

        public static string
        ToString(
            EPlatform platformFlags,
            char separator,
            string prefix,
            bool toUpper)
        {
            string platformString = null;

            // check windows and sub-derivatives
            if (Contains(platformFlags, EPlatform.Windows))
            {
                AddPlatformName(ref platformString, "Windows", separator, prefix, toUpper);
            }
            else
            {
                if (Contains(platformFlags, EPlatform.Win32))
                {
                    AddPlatformName(ref platformString, "Win32", separator, prefix, toUpper);
                }
                else if (Contains(platformFlags, EPlatform.Win64))
                {
                    AddPlatformName(ref platformString, "Win64", separator, prefix, toUpper);
                }
            }

            // check unix and sub-derivatives
            if (Contains(platformFlags, EPlatform.Unix))
            {
                AddPlatformName(ref platformString, "Unix", separator, prefix, toUpper);
            }
            else
            {
                if (Contains(platformFlags, EPlatform.Unix32))
                {
                    AddPlatformName(ref platformString, "Unix32", separator, prefix, toUpper);
                }
                else if (Contains(platformFlags, EPlatform.Unix64))
                {
                    AddPlatformName(ref platformString, "Unix64", separator, prefix, toUpper);
                }
            }

            // check OSX and sub-derivatives
            if (Contains(platformFlags, EPlatform.OSX))
            {
                AddPlatformName(ref platformString, "OSX", separator, prefix, toUpper);
            }
            else
            {
                if (Contains(platformFlags, EPlatform.OSX32))
                {
                    AddPlatformName(ref platformString, "OSX32", separator, prefix, toUpper);
                }
                else if (Contains(platformFlags, EPlatform.OSX64))
                {
                    AddPlatformName(ref platformString, "OSX64", separator, prefix, toUpper);
                }
            }

            return platformString;
        }
    }
}
