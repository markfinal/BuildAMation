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
    public static class OSUtilities
    {
        private static bool CheckFor64BitOS
        {
            get
            {
                if (Graph.Instance.ProcessState.RunningMono)
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
                Linux,
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
                    return OS.Linux;
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
                    CurrentPlatform = CheckFor64BitOS ? EPlatform.Win64 : EPlatform.Win32;
                    break;

                case Platform.OS.Linux:
                    CurrentPlatform = CheckFor64BitOS ? EPlatform.Linux64 : EPlatform.Linux32;
                    break;

                case Platform.OS.OSX:
                    CurrentPlatform = CheckFor64BitOS ? EPlatform.OSX64 : EPlatform.OSX32;
                    break;

                default:
                    throw new Exception("Unrecognized platform, {0}", os.ToString());
            }

            IsLittleEndian = System.BitConverter.IsLittleEndian;
        }

        public static bool
        IsWindows(
            EPlatform platform)
        {
            var isWindows = (EPlatform.Win32 == platform || EPlatform.Win64 == platform);
            return isWindows;
        }

        public static bool IsWindowsHosting
        {
            get
            {
                return IsWindows(CurrentPlatform);
            }
        }

        public static bool
        IsLinux(
            EPlatform platform)
        {
            var isLinux = (EPlatform.Linux32 == platform || EPlatform.Linux64 == platform);
            return isLinux;
        }

        public static bool IsLinuxHosting
        {
            get
            {
                return IsLinux(CurrentPlatform);
            }
        }

        public static bool
        IsOSX(
            EPlatform platform)
        {
            var isOSX = (EPlatform.OSX32 == platform || EPlatform.OSX64 == platform);
            return isOSX;
        }

        public static bool IsOSXHosting
        {
            get
            {
                return IsOSX(CurrentPlatform);
            }
        }

        public static bool
        Is64Bit(
            EPlatform platform)
        {
            var is64Bit = (EPlatform.Win64 == platform || EPlatform.Linux64 == platform || EPlatform.OSX64 == platform);
            return is64Bit;
        }

        public static bool Is64BitHosting
        {
            get
            {
                return Is64Bit(CurrentPlatform);
            }
        }

        public static bool
        IsCurrentPlatformSupported(
            EPlatform supportedPlatforms)
        {
            var isSupported = (CurrentPlatform == (supportedPlatforms & CurrentPlatform));
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
                    case Platform.OS.Linux:
                        return EPlatform.Linux;
                    case Platform.OS.OSX:
                        return EPlatform.OSX;
                    default:
                        throw new Exception("Unknown platform");
                }
            }
        }

        public static bool
        IsLittleEndian
        {
            get;
            private set;
        }

        public static EPlatform CurrentPlatform
        {
            get;
            private set;
        }
    }
}
