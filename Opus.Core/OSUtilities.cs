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

        // based on http://go-mono.com/forums/#nabble-td1549244
        private static class Platform
        {
            [System.Runtime.InteropServices.DllImport("libc")]
            static extern int uname(System.IntPtr buf);

            static private bool mIsWindows;
            static private bool mIsMac;
            public enum OS
            {
                Windows,
                OSX,
                Unix,
                unknown
            };
            static public OS GetOS()
            {
                if (mIsWindows = (System.IO.Path.DirectorySeparatorChar == '\\'))
                {
                    return OS.Windows;
                }

                if (mIsMac = (!mIsWindows && IsRunningOnMac()))
                {
                    return OS.OSX;
                }

                if (!mIsMac && System.Environment.OSVersion.Platform == System.PlatformID.Unix)
                {
                    return OS.Unix;
                }

                return OS.unknown;
            }

            //From Managed.Windows.Forms/XplatUI
            static bool IsRunningOnMac()
            {
                System.IntPtr buf = System.IntPtr.Zero;
                try
                {
                    buf = System.Runtime.InteropServices.Marshal.AllocHGlobal(8192);
                    // This is a hacktastic way of getting sysname from uname ()
                    if (uname(buf) == 0)
                    {
                        string os = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(buf);
                        if ("Darwin" == os)
                        {
                            return true;
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    if (buf != System.IntPtr.Zero)
                    {
                        System.Runtime.InteropServices.Marshal.FreeHGlobal(buf);
                    }
                }

                return false;
            }
        }

        public static void SetupPlatform()
        {
            Platform.OS os = Platform.GetOS();
            switch (os)
            {
                case Platform.OS.Windows:
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

                case Platform.OS.Unix:
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

                case Platform.OS.OSX:
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

            bool isLittleEndian = System.BitConverter.IsLittleEndian;
            State.Add<bool>("System", "IsLittleEndian", isLittleEndian);
        }

        public static bool IsWindows(EPlatform platform)
        {
            bool isWindows = (EPlatform.Win32 == platform || EPlatform.Win64 == platform);
            return isWindows;
        }

        public static bool IsWindows(BaseTarget baseTarget)
        {
            bool isWindows = baseTarget.HasPlatform(EPlatform.Windows);
            return isWindows;
        }

        public static bool IsWindows(Target target)
        {
            return IsWindows((BaseTarget)target);
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

        public static bool IsUnix(BaseTarget baseTarget)
        {
            bool isUnix = baseTarget.HasPlatform(EPlatform.Unix);
            return isUnix;
        }

        public static bool IsUnix(Target target)
        {
            return IsUnix((BaseTarget)target);
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

        public static bool IsOSX(BaseTarget baseTarget)
        {
            bool isOSX = baseTarget.HasPlatform(EPlatform.OSX);
            return isOSX;
        }

        public static bool IsOSX(Target target)
        {
            return IsOSX((BaseTarget)target);
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

        public static bool Is64Bit(BaseTarget baseTarget)
        {
            bool is64Bit = baseTarget.HasPlatform(EPlatform.Win64 | EPlatform.Unix64 | EPlatform.OSX64);
            return is64Bit;
        }

        public static bool Is64Bit(Target target)
        {
            return Is64Bit((BaseTarget)target);
        }

        public static bool Is64BitHosting
        {
            get
            {
                EPlatform platform = State.Platform;
                return Is64Bit(platform);
            }
        }

        public static bool IsCurrentPlatformSupported(EPlatform supportedPlatforms)
        {
            EPlatform currentPlatform = State.Platform;
            bool isSupported = (currentPlatform == (supportedPlatforms & currentPlatform));
            return isSupported;
        }
    }
}