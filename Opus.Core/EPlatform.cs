// <copyright file="EPlatform.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    [System.Flags]
    public enum EPlatform
    {
        Invalid = 0,
        Win32   = (1 << 0),
        Win64   = (1 << 1),
        Unix32  = (1 << 2),
        Unix64  = (1 << 3),
        OSX32   = (1 << 4),
        OSX64   = (1 << 5),

        Windows = Win32 | Win64,
        Unix    = Unix32 | Unix64,
        OSX     = OSX32 | OSX64,

        All     = Windows | Unix | OSX
    }

    public static class Platform
    {
        public static EPlatform FromString(string platformName)
        {
            EPlatform platform = EPlatform.Invalid;
            if (0 == System.String.Compare(platformName, "Win32", true))
            {
                platform = EPlatform.Win32;
            }
            else if (0 == System.String.Compare(platformName, "Win64", true))
            {
                platform = EPlatform.Win64;
            }
            else if (0 == System.String.Compare(platformName, "Unix32", true))
            {
                platform = EPlatform.Unix32;
            }
            else if (0 == System.String.Compare(platformName, "Unix64", true))
            {
                platform = EPlatform.Unix64;
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
                throw new Exception(System.String.Format("Platform name '{0}' not recognized", platformName));
            }
            return platform;
        }

        private static void AddPlatformName(ref string platformString, string name, char separator)
        {
            if (null != platformString)
            {
                platformString += separator;
            }
            platformString += name;
        }

        public static bool Contains(EPlatform flags, EPlatform specificPlatform)
        {
            bool contains = (specificPlatform == (flags & specificPlatform));
            return contains;
        }

        public static string ToString(EPlatform platformFlags, char separator)
        {
            string platformString = null;

            // check windows and sub-derivatives
            if (Contains(platformFlags, EPlatform.Windows))
            {
                AddPlatformName(ref platformString, "Windows", separator);
            }
            else
            {
                if (Contains(platformFlags, EPlatform.Win32))
                {
                    AddPlatformName(ref platformString, "Win32", separator);
                }
                else if (Contains(platformFlags, EPlatform.Win64))
                {
                    AddPlatformName(ref platformString, "Win64", separator);
                }
            }

            // check unix and sub-derivatives
            if (Contains(platformFlags, EPlatform.Unix))
            {
                AddPlatformName(ref platformString, "Unix", separator);
            }
            else
            {
                if (Contains(platformFlags, EPlatform.Unix32))
                {
                    AddPlatformName(ref platformString, "Unix32", separator);
                }
                else if (Contains(platformFlags, EPlatform.Unix64))
                {
                    AddPlatformName(ref platformString, "Unix64", separator);
                }
            }

            // check OSX and sub-derivatives
            if (Contains(platformFlags, EPlatform.OSX))
            {
                AddPlatformName(ref platformString, "OSX", separator);
            }
            else
            {
                if (Contains(platformFlags, EPlatform.OSX32))
                {
                    AddPlatformName(ref platformString, "OSX32", separator);
                }
                else if (Contains(platformFlags, EPlatform.OSX64))
                {
                    AddPlatformName(ref platformString, "OSX64", separator);
                }
            }

            return platformString;
        }
    }
}