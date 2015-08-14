#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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
