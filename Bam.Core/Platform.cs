#region License
// Copyright (c) 2010-2019, Mark Final
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
    /// <summary>
    /// Static utility class with utilities surrounding the Bam Platform.
    /// </summary>
    public static class Platform
    {
        /// <summary>
        /// Is the specified platform included in the enumeration?
        /// </summary>
        /// <param name="platform">Platform.</param>
        /// <param name="choice">Choice.</param>
        // can't just use HasFlag, as the logic is inversed with the combined flag enum values
        public static bool
        Includes(
            this EPlatform platform,
            EPlatform choice) => (0 != (platform & choice));

        /// <summary>
        /// Convert a string to an EPlatform enumeration.
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="platformName">Platform name, case insensitive.</param>
        public static EPlatform
        FromString(
            string platformName)
        {
            var platform = EPlatform.Invalid;
            if (platformName.Equals("Windows", System.StringComparison.OrdinalIgnoreCase))
            {
                platform = EPlatform.Windows;
            }
            else if (platformName.Equals("Win32", System.StringComparison.OrdinalIgnoreCase))
            {
                platform = EPlatform.Win32;
            }
            else if (platformName.Equals("Win64", System.StringComparison.OrdinalIgnoreCase))
            {
                platform = EPlatform.Win64;
            }
            else if (platformName.Equals("Linux", System.StringComparison.OrdinalIgnoreCase))
            {
                platform = EPlatform.Linux;
            }
            else if (platformName.Equals("Linux32", System.StringComparison.OrdinalIgnoreCase))
            {
                platform = EPlatform.Linux32;
            }
            else if (platformName.Equals("Linux64", System.StringComparison.OrdinalIgnoreCase))
            {
                platform = EPlatform.Linux64;
            }
            else if (platformName.Equals("OSX", System.StringComparison.OrdinalIgnoreCase))
            {
                platform = EPlatform.OSX;
            }
            else if (platformName.Equals("OSX32", System.StringComparison.OrdinalIgnoreCase))
            {
                platform = EPlatform.OSX32;
            }
            else if (platformName.Equals("OSX64", System.StringComparison.OrdinalIgnoreCase))
            {
                platform = EPlatform.OSX64;
            }
            else
            {
                throw new Exception($"Platform name '{platformName}' not recognized");
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

        /// <summary>
        /// Convert EPlatform to a string.
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="platformFlags">Platform flags.</param>
        public static string
        ToString(
            EPlatform platformFlags) => ToString(platformFlags, '\0', null, false);

        /// <summary>
        /// Convert EPlatform to a string.
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="platformFlags">Platform flags.</param>
        /// <param name="separator">Separator.</param>
        public static string
        ToString(
            EPlatform platformFlags,
            char separator) => ToString(platformFlags, separator, null, false);

        private static bool
        Contains(
            EPlatform flags,
            EPlatform specificPlatform) => (specificPlatform == (flags & specificPlatform));

        /// <summary>
        /// Convert an EPlatform to a string.
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="platformFlags">Platform flags.</param>
        /// <param name="separator">Separator.</param>
        /// <param name="prefix">Prefix.</param>
        /// <param name="toUpper">If set to <c>true</c> to upper.</param>
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

            // check linux and sub-derivatives
            if (Contains(platformFlags, EPlatform.Linux))
            {
                AddPlatformName(ref platformString, "Linux", separator, prefix, toUpper);
            }
            else
            {
                if (Contains(platformFlags, EPlatform.Linux32))
                {
                    AddPlatformName(ref platformString, "Linux32", separator, prefix, toUpper);
                }
                else if (Contains(platformFlags, EPlatform.Linux64))
                {
                    AddPlatformName(ref platformString, "Linux64", separator, prefix, toUpper);
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
