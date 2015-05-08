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

namespace Bam.Core
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
                    var is64Bit = (8 == System.IntPtr.Size);
                    return is64Bit;
                }
                else
                {
                    // cannot do a check for the Wow6432Node as it does exist on some 32-bit Windows OS (Vista for example)
                    var is64Bit = (System.Environment.GetEnvironmentVariable("ProgramFiles(x86)") != null);
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

            static public OS
            GetOS()
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
            static bool
            IsRunningOnMac()
            {
                var buf = System.IntPtr.Zero;
                try
                {
                    buf = System.Runtime.InteropServices.Marshal.AllocHGlobal(8192);
                    // This is a hacktastic way of getting sysname from uname ()
                    if (uname(buf) == 0)
                    {
                        var os = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(buf);
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

        public static void
        SetupPlatform()
        {
            var os = Platform.GetOS();
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

            var isLittleEndian = System.BitConverter.IsLittleEndian;
            State.Add<bool>("System", "IsLittleEndian", isLittleEndian);
        }

        public static bool
        IsWindows(
            EPlatform platform)
        {
            var isWindows = (EPlatform.Win32 == platform || EPlatform.Win64 == platform);
            return isWindows;
        }

        public static bool
        IsWindows(
            BaseTarget baseTarget)
        {
            var isWindows = baseTarget.HasPlatform(EPlatform.Windows);
            return isWindows;
        }

        public static bool
        IsWindows(
            Target target)
        {
            return IsWindows((BaseTarget)target);
        }

        public static bool IsWindowsHosting
        {
            get
            {
                var platform = State.Platform;
                return IsWindows(platform);
            }
        }

        public static bool
        IsUnix(
            EPlatform platform)
        {
            var isUnix = (EPlatform.Unix32 == platform || EPlatform.Unix64 == platform);
            return isUnix;
        }

        public static bool
        IsUnix(
            BaseTarget baseTarget)
        {
            var isUnix = baseTarget.HasPlatform(EPlatform.Unix);
            return isUnix;
        }

        public static bool
        IsUnix(
            Target target)
        {
            return IsUnix((BaseTarget)target);
        }

        public static bool IsUnixHosting
        {
            get
            {
                var platform = State.Platform;
                return IsUnix(platform);
            }
        }

        public static bool
        IsOSX(
            EPlatform platform)
        {
            var isOSX = (EPlatform.OSX32 == platform || EPlatform.OSX64 == platform);
            return isOSX;
        }

        public static bool
        IsOSX(
            BaseTarget baseTarget)
        {
            var isOSX = baseTarget.HasPlatform(EPlatform.OSX);
            return isOSX;
        }

        public static bool
        IsOSX(
            Target target)
        {
            return IsOSX((BaseTarget)target);
        }

        public static bool IsOSXHosting
        {
            get
            {
                var platform = State.Platform;
                return IsOSX(platform);
            }
        }

        public static bool
        Is64Bit(
            EPlatform platform)
        {
            var is64Bit = (EPlatform.Win64 == platform || EPlatform.Unix64 == platform || EPlatform.OSX64 == platform);
            return is64Bit;
        }

        public static bool
        Is64Bit(
            BaseTarget baseTarget)
        {
            var is64Bit = baseTarget.HasPlatform(EPlatform.Win64 | EPlatform.Unix64 | EPlatform.OSX64);
            return is64Bit;
        }

        public static bool
        Is64Bit(
            Target target)
        {
            return Is64Bit((BaseTarget)target);
        }

        public static bool Is64BitHosting
        {
            get
            {
                var platform = State.Platform;
                return Is64Bit(platform);
            }
        }

        public static bool
        IsCurrentPlatformSupported(
            EPlatform supportedPlatforms)
        {
            var currentPlatform = State.Platform;
            var isSupported = (currentPlatform == (supportedPlatforms & currentPlatform));
            return isSupported;
        }

        public static EPlatform
        CurrentOS
        {
            get
            {
                var os = Platform.GetOS();
                switch (os)
                {
                    case Platform.OS.Windows:
                        return EPlatform.Windows;
                    case Platform.OS.Unix:
                        return EPlatform.Unix;
                    case Platform.OS.OSX:
                        return EPlatform.OSX;
                    default:
                        throw new Exception("Unknown platform");
                }
            }
        }
    }
}
