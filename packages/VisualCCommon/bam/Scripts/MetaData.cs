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
using System.Linq;
namespace VisualCCommon
{
    /// <summary>
    /// Meta data class for this package
    /// </summary>
    public abstract class MetaData :
        Bam.Core.PackageMetaData,
        C.IToolchainDiscovery
    {
        /// <summary>
        /// Dictionary of key, object pairs that are the metadata.
        /// </summary>
        protected System.Collections.Generic.Dictionary<string, object> Meta = new System.Collections.Generic.Dictionary<string, object>();

        /// <summary>
        /// Indexer into the metadata
        /// </summary>
        /// <param name="index">String index to use</param>
        /// <returns>Indexed result</returns>
        public override object this[string index] => this.Meta[index];

        /// <summary>
        /// Does the meta data contain an index.
        /// </summary>
        /// <param name="index">String index to query</param>
        /// <returns>True if the meta data contains the index.</returns>
        public override bool
        Contains(
            string index)
        {
            return this.Meta.ContainsKey(index);
        }

        private void
        Findvswhere()
        {
#if D_NUGET_NUGET_CLIENT && D_NUGET_VSWHERE
            var nugetHomeDir = NuGet.Common.NuGetEnvironment.GetFolderPath(NuGet.Common.NuGetFolderPath.NuGetHome);
            var nugetPackageDir = System.IO.Path.Combine(nugetHomeDir, "packages");
            var repo = new NuGet.Repositories.NuGetv3LocalRepository(nugetPackageDir);
            var vswhereInstalls = repo.FindPackagesById("vswhere");
            if (!vswhereInstalls.Any())
            {
                // this should not happen as package restoration should handle this
                throw new Bam.Core.Exception("Unable to locate any NuGet package for vswhere");
            }
            var visualCCommon = Bam.Core.Graph.Instance.Packages.First(item => item.Name.Equals("VisualCCommon", System.StringComparison.Ordinal));
            var requiredVSWhere = visualCCommon.NuGetPackages.First(item => item.Identifier.Equals("vswhere", System.StringComparison.Ordinal));
            var requestedVSWhere = vswhereInstalls.First(item => item.Version.ToNormalizedString().Equals(requiredVSWhere.Version, System.StringComparison.Ordinal));
            var vswhere_tools_dir = System.IO.Path.Combine(requestedVSWhere.ExpandedPath, "tools");
            var vswhere_exe_path = System.IO.Path.Combine(vswhere_tools_dir, "vswhere.exe");
            if (!System.IO.File.Exists(vswhere_exe_path))
            {
                throw new Bam.Core.Exception($"Unable to locate vswhere.exe from NuGet package at '{vswhere_exe_path}'");
            }
            this.VswherePath = vswhere_exe_path;
#endif
        }

        /// <summary>
        /// Get the vswhere install path
        /// </summary>
        /// <returns>Installation path</returns>
        protected string
        Vswhere_getinstallpath()
        {
            var package_version = Bam.Core.Graph.Instance.Packages.First(item => item.Name == "VisualC").Version;
            var major_version = System.Convert.ToInt32(package_version.Split('.').First());
            try
            {
                var args = new System.Text.StringBuilder();
                var legacy = major_version < 15;
                if (Bam.Core.CommandLineProcessor.Evaluate(new Options.DiscoverPrereleases()))
                {
                    args.Append("-prerelease ");
                }
                args.Append("-property installationPath -version ");
                if (legacy)
                {
                    // note the [] around the version to specify only that version
                    args.Append($"[{major_version}] -legacy");
                }
                else
                {
                    // note the [) around the version and version+1 to consider only the minor-releases of that version
                    args.Append($"[{major_version},{major_version + 1})");
                }
                var installpath = Bam.Core.OSUtilities.RunExecutable(
                    this.VswherePath,
                    args.ToString()
                ).StandardOutput;
                if (System.String.IsNullOrEmpty(installpath))
                {
                    throw new Bam.Core.Exception(
                        $"Unable to locate installation directory for Visual Studio major version {major_version} using '{this.VswherePath} {args.ToString()}'"
                    );
                }
                Bam.Core.Log.Info($"Using VisualStudio {major_version} installed at {installpath}");
                return installpath;
            }
            catch (Bam.Core.RunExecutableException)
            {
                throw new Bam.Core.Exception(
                    $"Unable to locate installation directory for Visual Studio major version {major_version}"
                );
            }
        }

        private System.Collections.Generic.Dictionary<string, Bam.Core.TokenizedStringArray>
        Execute_vcvars(
            C.EBit depth,
            bool hasNative64BitTools,
            Bam.Core.StringArray inherited_envvars,
            System.Collections.Generic.Dictionary<string, Bam.Core.StringArray> required_envvars
        )
        {
            var startinfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = @"c:\Windows\System32\cmd.exe"
            };
            startinfo.EnvironmentVariables.Clear();
            if (null != inherited_envvars)
            {
                foreach (var inherited in inherited_envvars)
                {
                    startinfo.EnvironmentVariables.Add(inherited, System.Environment.GetEnvironmentVariable(inherited));
                }
            }
            if (null != required_envvars)
            {
                foreach (System.Collections.Generic.KeyValuePair<string, Bam.Core.StringArray> required in required_envvars)
                {
                    if (startinfo.EnvironmentVariables.ContainsKey(required.Key))
                    {
                        var existing_value = startinfo.EnvironmentVariables[required.Key];
                        var updated_value = $"{System.Environment.ExpandEnvironmentVariables(required.Value.ToString(';'))};{existing_value}";
                        startinfo.EnvironmentVariables[required.Key] = updated_value;
                    }
                    else
                    {
                        startinfo.EnvironmentVariables.Add(required.Key, System.Environment.ExpandEnvironmentVariables(required.Value.ToString(';')));
                    }
                }
            }
            startinfo.UseShellExecute = false;
            startinfo.RedirectStandardInput = true;
            startinfo.RedirectStandardOutput = true;
            startinfo.RedirectStandardError = true;

            string vcvarsall_command()
            {
                var command_and_args = new System.Text.StringBuilder();
                command_and_args.Append("vcvarsall.bat ");
                switch (depth)
                {
                    case C.EBit.ThirtyTwo:
                        // amd64_x86 seems to be troublesome in terms of finding dependent DLLs, so ignore it
                        command_and_args.Append("x86 ");
                        break;

                    case C.EBit.SixtyFour:
                        {
                            if (Bam.Core.OSUtilities.Is64BitHosting && hasNative64BitTools)
                            {
                                command_and_args.Append("amd64 ");
                            }
                            else
                            {
                                command_and_args.Append("x86_amd64 ");
                            }
                        }
                        break;
                }
                // VisualC packages define their 'default' WindowsSDK package to function with
                // if this is different to what is being used, append the version fo the vcvarsall.bat command
                var visualC = Bam.Core.Graph.Instance.Packages.First(item => item.Name.Equals("VisualC", System.StringComparison.Ordinal));
                var defaultWindowsSDKVersion = visualC.Dependents.First(item => item.Item1.Equals("WindowsSDK", System.StringComparison.Ordinal)).Item2;
                var windowsSDK = Bam.Core.Graph.Instance.Packages.FirstOrDefault(item => item.Name.Equals("WindowsSDK", System.StringComparison.Ordinal));
                if (null != windowsSDK)
                {
                    if (windowsSDK.Version != defaultWindowsSDKVersion)
                    {
                        command_and_args.Append($"{windowsSDK.Version} ");
                    }
                    else
                    {
                        var option_type = System.Type.GetType("WindowsSDK.Options.WindowsSDK10Version", throwOnError: false);
                        if (null != option_type)
                        {
                            if (System.Activator.CreateInstance(option_type) is Bam.Core.IStringCommandLineArgument option_type_instance)
                            {
                                var win10Option = Bam.Core.CommandLineProcessor.Evaluate(option_type_instance);
                                if (null != win10Option)
                                {
                                    command_and_args.Append($"{win10Option} ");
                                }
                            }
                        }
                    }
                }
                return command_and_args.ToString();
            }

            var environment_generator_cmdline = System.String.Empty;
            // allow the WindowsSDK to provide an alternative mechanism for generating
            // and environment in which to execute VisualC and WindowsSDK tools
            var windowssdk_meta = Bam.Core.Graph.Instance.PackageMetaData<WindowsSDK.MetaData>("WindowsSDK");
            if (windowssdk_meta.Contains("setenvdir") && windowssdk_meta.Contains("setenvcmd"))
            {
                startinfo.WorkingDirectory = windowssdk_meta["setenvdir"] as string;
                environment_generator_cmdline = windowssdk_meta["setenvcmd"] as string;
                switch (depth)
                {
                    case C.EBit.ThirtyTwo:
                        environment_generator_cmdline += " /x86";
                        break;
                    case C.EBit.SixtyFour:
                        environment_generator_cmdline += " /x64";
                        break;
                }
            }
            else
            {
                startinfo.WorkingDirectory = System.IO.Path.Combine(this.InstallDir.ToString(), this.Subpath_to_vcvars);
                environment_generator_cmdline = vcvarsall_command();
            }

            // allow the WindowsSDK to override the VisualStudio project's PlatformToolset
            if (windowssdk_meta.Contains("PlatformToolset"))
            {
                var vc_meta = Bam.Core.Graph.Instance.PackageMetaData<VisualC.MetaData>("VisualC");
                vc_meta.PlatformToolset = windowssdk_meta["PlatformToolset"] as string;
            }

            var arguments = new System.Text.StringBuilder();
            arguments.Append($"/C {environment_generator_cmdline} && SET");
            startinfo.Arguments = arguments.ToString();

            var process = new System.Diagnostics.Process();
            process.StartInfo = startinfo;

            // if you don't async read the output, then the process will never finish
            // as the buffer is filled up
            // EOLs will also be trimmed from these, so always append whole lines
            var stdout = new System.Text.StringBuilder();
            process.OutputDataReceived += (sender, args) => stdout.AppendLine(args.Data);
            var stderr = new System.Text.StringBuilder();
            process.ErrorDataReceived += (sender, args) => stderr.AppendLine(args.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.StandardInput.Close();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                throw new Bam.Core.Exception($"{environment_generator_cmdline} failed: {stderr.ToString()}");
            }

            var env = new System.Collections.Generic.Dictionary<string, Bam.Core.TokenizedStringArray>();
            var lines = stdout.ToString().Split(
                new[] { System.Environment.NewLine },
                System.StringSplitOptions.RemoveEmptyEntries
            );
            foreach (var line in lines)
            {
                Bam.Core.Log.DebugMessage($"{environment_generator_cmdline}->{line}");
                var equals_index = line.IndexOf('=');
                if (-1 == equals_index)
                {
                    continue;
                }
                var key = line.Remove(equals_index);
                var value = line.Remove(0, equals_index + 1);
                if (System.String.IsNullOrEmpty(key) || System.String.IsNullOrEmpty(value))
                {
                    continue;
                }
                var splitValue = value.Split(new[] { ';' });
                var valueArray = new Bam.Core.TokenizedStringArray();
                foreach (var v in splitValue)
                {
                    valueArray.Add(Bam.Core.TokenizedString.CreateVerbatim(v));
                }
                env.Add(key, valueArray);
            }
            Bam.Core.Log.Info(
                $@"Generating {(int)depth}-bit build environment using '{startinfo.WorkingDirectory}\{environment_generator_cmdline.TrimEnd()}'"
            );
            foreach (System.Collections.Generic.KeyValuePair<string, Bam.Core.TokenizedStringArray> entry in env)
            {
                Bam.Core.Log.DebugMessage($"\t{entry.Key} = {entry.Value.ToString(';')}");
            }
            return env;
        }

        /// <summary>
        /// Get the environment variables to apply to tools
        /// </summary>
        /// <param name="depth">Bit depth of tool</param>
        /// <param name="hasNative64BitTools">Does the toolchain have native 64-bit tools?</param>
        /// <param name="inherited_envvars">Optional, array of environment variables to inherit from the system. Default is null.</param>
        /// <param name="required_envvars">Optional, dictionary of environment variables to set. Default is null.</param>
        protected void
        Get_tool_environment_variables(
            C.EBit depth,
            bool hasNative64BitTools,
            Bam.Core.StringArray inherited_envvars = null,
            System.Collections.Generic.Dictionary<string, Bam.Core.StringArray> required_envvars = null)
        {
            // for 'reg' used in vcvarsall subroutines
            if (null == required_envvars)
            {
                required_envvars = new System.Collections.Generic.Dictionary<string, Bam.Core.StringArray>();
            }
            if (required_envvars.ContainsKey("PATH"))
            {
                var existing = required_envvars["PATH"];
                existing.AddUnique("%WINDIR%\\System32");
                required_envvars["PATH"] = existing;
            }
            else
            {
                required_envvars.Add("PATH", new Bam.Core.StringArray { "%WINDIR%\\System32" });
            }

            // VsDevCmd.bat in VS2019+ tries to invoke powershell.exe to send telemetry
            // which can cause non-interactive continuous integration to halt indefinitely
            // because a dialog is opened if powershell cannot be found
            // disabling telemetry avoids this issue
            required_envvars.Add("VSCMD_SKIP_SENDTELEMETRY", new Bam.Core.StringArray { "1" });

            // for WindowsSDK-7.1
            if (null == inherited_envvars)
            {
                inherited_envvars = new Bam.Core.StringArray();
            }
            inherited_envvars.AddUnique("PROCESSOR_ARCHITECTURE");

            var env = this.Execute_vcvars(
                depth,
                hasNative64BitTools,
                inherited_envvars,
                required_envvars
            );
            this.Meta.Add(EnvironmentKey(depth), env);
        }

        /// <summary>
        /// Get or set the installation directory for VisualStudio
        /// </summary>
        public Bam.Core.TokenizedString
        InstallDir
        {
            get
            {
                return this.Meta["InstallDir"] as Bam.Core.TokenizedString;
            }

            protected set
            {
                this.Meta["InstallDir"] = value;
            }
        }

        static private string
        EnvironmentKey(
            C.EBit depth)
        {
            switch (depth)
            {
                case C.EBit.ThirtyTwo:
                    return "Environment32";
                case C.EBit.SixtyFour:
                    return "Environment64";
                default:
                    throw new Bam.Core.Exception($"Unknown bit depth, {depth.ToString()}");
            }
        }

        /// <summary>
        /// Get the environment variables for the given bitdepth
        /// </summary>
        /// <param name="depth">Bit depth</param>
        /// <returns>Environment variable dictionary</returns>
        public System.Collections.Generic.Dictionary<string, Bam.Core.TokenizedStringArray>
        Environment(
            C.EBit depth)
        {
            var environmentKey = EnvironmentKey(depth);
            if (!this.Meta.ContainsKey(environmentKey))
            {
                return new System.Collections.Generic.Dictionary<string, Bam.Core.TokenizedStringArray>();
            }
            return this.Meta[environmentKey] as System.Collections.Generic.Dictionary<string, Bam.Core.TokenizedStringArray>;
        }

        private string VswherePath { get; set; }

        /// <summary>
        /// Get the toolchain version
        /// </summary>
        public C.ToolchainVersion ToolchainVersion
        {
            get
            {
                return this.Meta["ToolchainVersion"] as C.ToolchainVersion;
            }

            private set
            {
                this.Meta["ToolchainVersion"] = value;
            }
        }

        /// <summary>
        /// Get the sub-path to where vcvars.bat resides
        /// </summary>
        protected abstract string Subpath_to_vcvars { get; }

        /// <summary>
        /// Whether there are native 64-bit tools available
        /// </summary>
        protected virtual bool HasNative64BitTools => true;

        private static bool report_WindowsSDK_done = false;
        private static void
        Report_WindowsSDK(
            System.Collections.Generic.Dictionary<string, Bam.Core.TokenizedStringArray> env,
            C.EBit depth)
        {
            // only need to report it once, not for each environment
            if (report_WindowsSDK_done)
            {
                return;
            }
            report_WindowsSDK_done = true;

            var report = new System.Text.StringBuilder();
            report.Append("Using WindowsSDK ");
            if (env.ContainsKey("WindowsSDKVersion"))
            {
                // the WindowsSDKVersion environment variable has a trailing back slash
                var version = env["WindowsSDKVersion"].ToString();
                version = version.TrimEnd(System.IO.Path.DirectorySeparatorChar);
                report.Append($"version {version} ");
            }
            var winsdk_installdir = System.String.Empty;
            if (env.ContainsKey("WindowsSdkDir"))
            {
                // WindowsSDK 7.0A, 8.1 has this form
                winsdk_installdir = env["WindowsSdkDir"].ToString();
            }
            else if (env.ContainsKey("WindowsSDKDir"))
            {
                // WindowsSDK 7.1 has this form
                winsdk_installdir = env["WindowsSDKDir"].ToString();
            }
            else
            {
                throw new Bam.Core.Exception(
                    $"Unable to locate WindowsSDK installation directory environment variable for {(int)depth}-bit builds"
                );
            }
            report.Append($"installed at {winsdk_installdir} ");
            if (env.ContainsKey("UniversalCRTSdkDir") && env.ContainsKey("UCRTVersion"))
            {
                var ucrt_installdir = env["UniversalCRTSdkDir"].ToString();
                if (ucrt_installdir != winsdk_installdir)
                {
                    report.Append($"with UniversalCRT SDK {env["UCRTVersion"].ToString()} installed at {ucrt_installdir} ");
                }
            }
            Bam.Core.Log.Info(report.ToString());
        }

        private C.ToolchainVersion
        GetCompilerVersion()
        {
            var temp_file = System.IO.Path.GetTempFileName();
            System.IO.File.WriteAllText(temp_file, "_MSC_VER");
            var result = Bam.Core.OSUtilities.RunExecutable(
                System.IO.Path.Combine(
                    System.IO.Path.Combine(
                        this.InstallDir.ToString(),
                        this.Subpath_to_vcvars
                    ),
                    "vcvarsall.bat"
                ),
                $"x86 && cl /EP /nologo {temp_file}"
            );
            var mscver = result.StandardOutput.Split(System.Environment.NewLine.ToCharArray()).Reverse().First();
            return VisualCCommon.ToolchainVersion.FromMSCVer(System.Convert.ToInt32(mscver));
        }

        void
        C.IToolchainDiscovery.discover(
            C.EBit? depth)
        {
            if (null == this.VswherePath)
            {
                this.Findvswhere();
            }
            if (!this.Meta.ContainsKey("InstallDir"))
            {
                var install_dir = this.Vswhere_getinstallpath();
                if (install_dir.Contains(System.Environment.NewLine))
                {
                    throw new Bam.Core.Exception(
                        $"Multiple install directories were detected for VisualStudio:{System.Environment.NewLine}{install_dir}"
                    );
                }
                this.InstallDir = Bam.Core.TokenizedString.CreateVerbatim(install_dir);
            }
            var bitdepth = depth.Value;
            if (!this.Meta.ContainsKey(EnvironmentKey(bitdepth)))
            {
                this.Get_tool_environment_variables(
                    bitdepth,
                    this.HasNative64BitTools
                );
                Report_WindowsSDK(this.Environment(bitdepth), bitdepth);
            }
            if (!this.Meta.ContainsKey("ToolchainVersion"))
            {
                this.ToolchainVersion = this.GetCompilerVersion();
            }
            var runtimeChoice = Bam.Core.CommandLineProcessor.Evaluate(new Options.Runtime());
            if (runtimeChoice.Any())
            {
                switch (runtimeChoice.First().First())
                {
                    case "MD":
                        this.RuntimeLibrary = ERuntimeLibrary.MultiThreadedDLL;
                        break;

                    case "MDd":
                        this.RuntimeLibrary = ERuntimeLibrary.MultiThreadedDebugDLL;
                        break;

                    case "MT":
                        this.RuntimeLibrary = ERuntimeLibrary.MultiThreaded;
                        break;

                    case "MTd":
                        this.RuntimeLibrary = ERuntimeLibrary.MultiThreadedDebug;
                        break;

                    default:
                        throw new Bam.Core.Exception($"Unknown runtime library type: {runtimeChoice.First().First()}");
                }
            }
            if (!this.Meta.ContainsKey("RuntimeLibrary"))
            {
                this.RuntimeLibrary = ERuntimeLibrary.MultiThreadedDLL;
            }
        }

        /// <summary>
        /// Get or set the runtime library to use
        /// </summary>
        public ERuntimeLibrary RuntimeLibrary
        {
            get
            {
                return (ERuntimeLibrary)this.Meta["RuntimeLibrary"];
            }

            set
            {
                this.Meta["RuntimeLibrary"] = value;
            }
        }
    }
}
