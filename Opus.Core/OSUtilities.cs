// <copyright file="OSUtilities.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public static class OSUtilities
    {
        private static bool CheckFor64BitOS
        {
            get
            {
                if (State.RunningMono)
                {
					// TODO: System.Environment.GetEnvironmentVariable("HOSTTYPE") returns null instead of something like "x86_64"
					// TODO: this is a hack and a big assumption that you're not running a 32-bit OS on a 64-bit processor
					bool is64Bit = (8 == System.IntPtr.Size);
					return is64Bit;
                }
                else
                {
                    // cannot do a check for the Wow6432Node as it does exist on some 32-bit Windows OS (Vista for example)
                    bool is64Bit = (System.Environment.GetEnvironmentVariable("ProgramFiles(x86)") != null);
                    return is64Bit;
                }
            }
        }

        public static void SetupPlatform()
        {
            switch (System.Environment.OSVersion.Platform)
            {
                case System.PlatformID.Win32NT:
                case System.PlatformID.Win32S:
                case System.PlatformID.Win32Windows:
                case System.PlatformID.WinCE:
                    {
                        if (CheckFor64BitOS)
                        {
                            State.Add<EPlatform>("System", "Platform", EPlatform.Win64);
                        }
                        else
                        {
                            State.Add<EPlatform>("System", "Platform", EPlatform.Win32);
                        }
                    }
                    break;

                case System.PlatformID.Unix:
                    {
                        if (CheckFor64BitOS)
                        {
                            State.Add<EPlatform>("System", "Platform", EPlatform.Unix64);
                        }
                        else
                        {
                            State.Add<EPlatform>("System", "Platform", EPlatform.Unix32);
                        }
                    }
                    break;

                case System.PlatformID.MacOSX:
                    {
                        if (CheckFor64BitOS)
                        {
                            State.Add<EPlatform>("System", "Platform", EPlatform.OSX64);
                        }
                        else
                        {
                            State.Add<EPlatform>("System", "Platform", EPlatform.OSX32);
                        }
                    }
                    break;

                default:
                    throw new Exception("Unrecognized platform");
            }
        }

        public static bool IsWindows(EPlatform platform)
        {
            bool isWindows = (EPlatform.Win32 == platform || EPlatform.Win64 == platform);
            return isWindows;
        }

        public static bool IsWindowsHosting
        {
            get
            {
                EPlatform platform = State.Platform;
                return IsWindows(platform);
            }
        }

        public static bool IsUnix(EPlatform platform)
        {
            bool isUnix = (EPlatform.Unix32 == platform || EPlatform.Unix64 == platform);
            return isUnix;
        }

        public static bool IsUnixHosting
        {
            get
            {
                EPlatform platform = State.Platform;
                return IsUnix(platform);
            }
        }

        public static bool IsOSX(EPlatform platform)
        {
            bool isOSX = (EPlatform.OSX32 == platform || EPlatform.OSX64 == platform);
            return isOSX;
        }

        public static bool IsOSXHosting
        {
            get
            {
                EPlatform platform = State.Platform;
                return IsOSX(platform);
            }
        }

        public static bool Is64Bit(EPlatform platform)
        {
            bool is64Bit = (EPlatform.Win64 == platform || EPlatform.Unix64 == platform || EPlatform.OSX64 == platform);
            return is64Bit;
        }

        public static bool Is64BitHosting
        {
            get
            {
                EPlatform platform = State.Platform;
                return Is64Bit(platform);
            }
        }
    }
}