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
    /// Static utility class for configuring and querying OS specific details.
    /// </summary>
    public static class OSUtilities
    {
        private static System.Collections.Generic.Dictionary<string, StringArray> InstallLocationCache = new System.Collections.Generic.Dictionary<string, StringArray>();

        static OSUtilities()
        {
            var is64Bit = System.Environment.Is64BitOperatingSystem;
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                CurrentOS = EPlatform.Windows;
                CurrentPlatform = is64Bit ? EPlatform.Win64 : EPlatform.Win32;
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                CurrentOS = EPlatform.Linux;
                CurrentPlatform = is64Bit ? EPlatform.Linux64 : EPlatform.Linux32;
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
            {
                CurrentOS = EPlatform.OSX;
                CurrentPlatform = is64Bit ? EPlatform.OSX64 : EPlatform.OSX32;
            }
            else
            {
                throw new Exception("Unrecognized platform");
            }
            IsLittleEndian = System.BitConverter.IsLittleEndian;
        }

        /// <summary>
        /// Determines if is Windows the specified platform.
        /// </summary>
        /// <returns><c>true</c> if is windows the specified platform; otherwise, <c>false</c>.</returns>
        /// <param name="platform">Platform.</param>
        public static bool
        IsWindows(
            EPlatform platform) => (EPlatform.Win32 == platform || EPlatform.Win64 == platform);

        /// <summary>
        /// Determines if Windows is the current platform.
        /// </summary>
        /// <value><c>true</c> if is windows hosting; otherwise, <c>false</c>.</value>
        public static bool IsWindowsHosting => IsWindows(CurrentPlatform);

        /// <summary>
        /// Determines if is Linux the specified platform.
        /// </summary>
        /// <returns><c>true</c> if is linux the specified platform; otherwise, <c>false</c>.</returns>
        /// <param name="platform">Platform.</param>
        public static bool
        IsLinux(
            EPlatform platform) => (EPlatform.Linux32 == platform || EPlatform.Linux64 == platform);

        /// <summary>
        /// Determines if Linux is the current platform.
        /// </summary>
        /// <value><c>true</c> if is linux hosting; otherwise, <c>false</c>.</value>
        public static bool IsLinuxHosting => IsLinux(CurrentPlatform);

        /// <summary>
        /// Determines if is OSX the specified platform.
        /// </summary>
        /// <returns><c>true</c> if is OS the specified platform; otherwise, <c>false</c>.</returns>
        /// <param name="platform">Platform.</param>
        public static bool
        IsOSX(
            EPlatform platform) => (EPlatform.OSX32 == platform || EPlatform.OSX64 == platform);

        /// <summary>
        /// Determines if OSX is the current platform.
        /// </summary>
        /// <value><c>true</c> if is OSX hosting; otherwise, <c>false</c>.</value>
        public static bool IsOSXHosting => IsOSX(CurrentPlatform);

        /// <summary>
        /// Determines if the current platform is 64-bits.
        /// </summary>
        /// <returns><c>true</c> if is64 bit the specified platform; otherwise, <c>false</c>.</returns>
        /// <param name="platform">Platform.</param>
        public static bool
        Is64Bit(
            EPlatform platform) => (EPlatform.Win64 == platform || EPlatform.Linux64 == platform || EPlatform.OSX64 == platform);

        /// <summary>
        /// Determines if the current OS is 64-bit.
        /// </summary>
        /// <value><c>true</c> if is64 bit hosting; otherwise, <c>false</c>.</value>
        public static bool Is64BitHosting => Is64Bit(CurrentPlatform);

        /// <summary>
        /// Determines if the specified platform is supported by the current platform.
        /// </summary>
        /// <returns><c>true</c> if is current platform supported the specified supportedPlatforms; otherwise, <c>false</c>.</returns>
        /// <param name="supportedPlatforms">Supported platforms.</param>
        public static bool
        IsCurrentPlatformSupported(
            EPlatform supportedPlatforms) => (CurrentPlatform == (supportedPlatforms & CurrentPlatform));

        /// <summary>
        /// Get the current platform in terms of the EPlatform enumeration.
        /// </summary>
        /// <value>The current OS.</value>
        public static EPlatform CurrentOS { get; private set; }

        /// <summary>
        /// Determine if the current platform is little endian.
        /// </summary>
        /// <value><c>true</c> if is little endian; otherwise, <c>false</c>.</value>
        public static bool IsLittleEndian { get; private set; }

        /// <summary>
        /// Retrieve the current platform.
        /// </summary>
        /// <value>The current platform.</value>
        public static EPlatform CurrentPlatform { get; private set; }

        /// <summary>
        /// Retrieve the path to 'Program Files'. This is the path where native architecture programs are installed.
        /// The same path is returned on both 32-bit and 64-bit editions of Windows.
        /// </summary>
        public static TokenizedString WindowsProgramFilesPath
        {
            get
            {
                if (!IsWindowsHosting)
                {
                    throw new Exception("Only available on Windows");
                }
                return TokenizedString.CreateVerbatim(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles)
                );
            }
        }

        /// <summary>
        /// Retrive the path to 'Program Files (x86)'. This is the path where 32-bit architecture programs are installed.
        /// On 32-bit Windows, this is the same as WindowsProgramFilesPath.
        /// On 64-bit Windows, it is a different path.
        /// </summary>
        public static TokenizedString WindowsProgramFilesx86Path
        {
            get
            {
                if (!IsWindowsHosting)
                {
                    throw new Exception("Only available on Windows");
                }
                var envVar = Is64BitHosting ?
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86) :
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles);
                return TokenizedString.CreateVerbatim(envVar);
            }
        }

        /// <summary>
        /// Run an executable with a specified set of arguments.
        /// Will return a result, containing standard output, error and exit code.
        /// Will throw an exception if the exit code fro the executable is not zero.
        /// </summary>
        /// <param name="executable">Executable path to run.</param>
        /// <param name="arguments">Arguments to pass to the executable.</param>
        /// <returns>Result of running the executable.</returns>
        public static RunExecutableResult
        RunExecutable(
            string executable,
            string arguments)
        {
            var processStartInfo = new System.Diagnostics.ProcessStartInfo();
            processStartInfo.FileName = executable;
            processStartInfo.Arguments = arguments;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.UseShellExecute = false; // to redirect IO streams
            try
            {
                System.Diagnostics.Process process = System.Diagnostics.Process.Start(processStartInfo);
                process.StandardInput.Close();

                string outputBuffer = null;
                var outputThread = new System.Threading.Thread(() => { outputBuffer = process.StandardOutput.ReadToEnd(); });
                outputThread.Start();

                string errorBuffer = null;
                var errorThread = new System.Threading.Thread(() => { errorBuffer = process.StandardError.ReadToEnd(); });
                errorThread.Start();

                process.WaitForExit();
                errorThread.Join();
                outputThread.Join();

                var result = new RunExecutableResult(
                    outputBuffer?.TrimEnd(System.Environment.NewLine.ToCharArray()),
                    errorBuffer?.TrimEnd(System.Environment.NewLine.ToCharArray()),
                    process.ExitCode
                );
                if (0 != process.ExitCode)
                {
                    throw new RunExecutableException(
                        result,
                        $"Failed while running '{executable} {arguments}'"
                    );
                }

                return result;
            }
            catch (System.ComponentModel.Win32Exception exception)
            {
                var result = new RunExecutableResult(
                    "",
                    "",
                    -1
                );
                throw new RunExecutableException(
                    result,
                    $"Failed while trying to run '{executable} {arguments}' because {exception.Message}"
                );
            }
        }

        /// <summary>
        /// Gets the install location of an executable.
        /// The PATH is searched initially.
        /// If not found, on Windows, the x64 (if applicable) and x86 program files directories are recursively
        /// searched, in that order for the executable. This may be slow.
        /// If the searchDirectory argument is non-null, this is used instead of the Windows program file
        /// directories.
        /// An exception is thrown if it cannot be located in the system.
        /// Executable locations are cached (thread safe), so that multiple queries for the same
        /// executable does not need to invoke any external processes. If uniqueName is non-null, then
        /// this is used as the key in the cache, otherwise the executable name is used. The unique name
        /// may be useful to save different flavours of the same executable name.
        /// </summary>
        /// <returns>The installed locations of the executable (may be more than one). Or null if throwOnFailure is false when no match is found.</returns>
        /// <param name="executable">Filename of the executable to locate.</param>
        /// <param name="searchDirectory">Optional directory to search (Windows only).</param>
        /// <param name="uniqueName">Optional unique name to save as the key in the cache.</param>
        /// <param name="throwOnFailure">Optional Boolean, defaults to true, to indicate whether an exception is thrown when the executable is not found.</param>
        public static StringArray
        GetInstallLocation(
            string executable,
            string searchDirectory = null,
            string uniqueName = null,
            bool throwOnFailure = true)
        {
            lock (InstallLocationCache)
            {
                var key = uniqueName ?? executable;
                if (InstallLocationCache.ContainsKey(key))
                {
                    return InstallLocationCache[key];
                }
                string location;
                try
                {
                    if (OSUtilities.IsWindowsHosting)
                    {
                        if (null != searchDirectory)
                        {
                            var args = new System.Text.StringBuilder();
                            args.Append($"/R \"{searchDirectory}\" {executable}");
                            location = RunExecutable("where", args.ToString()).StandardOutput;
                        }
                        else
                        {
                            try
                            {
                                location = RunExecutable("where", executable).StandardOutput;
                            }
                            catch (RunExecutableException)
                            {
                                var args = new System.Text.StringBuilder();
                                args.Append($"/R \"{WindowsProgramFilesPath.ToString()}\" {executable}");
                                try
                                {
                                    location = RunExecutable("where", args.ToString()).StandardOutput;
                                }
                                catch (RunExecutableException)
                                {
                                    args.Length = 0;
                                    args.Capacity = 0;
                                    args.Append($"/R \"{WindowsProgramFilesx86Path.ToString()}\" {executable}");
                                    location = RunExecutable("where", args.ToString()).StandardOutput;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (null != searchDirectory)
                        {
                            Log.DebugMessage($"Search path '{searchDirectory}' is ignored on non-Windows platforms");
                        }
                        location = RunExecutable("which", executable).StandardOutput;
                    }
                }
                catch (RunExecutableException exception)
                {
                    if (throwOnFailure)
                    {
                        throw new Exception(exception, $"Unable to locate '{executable}' in the system.");
                    }
                    else
                    {
                        return null;
                    }
                }
                var results = new StringArray(
                    location.Split(
                        new[] { System.Environment.NewLine },
                        System.StringSplitOptions.RemoveEmptyEntries
                    )
                );
                InstallLocationCache.Add(key, results);
                return results;
            }
        }
    }
}
