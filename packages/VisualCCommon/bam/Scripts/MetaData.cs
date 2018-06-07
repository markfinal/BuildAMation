#region License
// Copyright (c) 2010-2018, Mark Final
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
    public abstract class MetaData :
        Bam.Core.PackageMetaData,
        C.IToolchainDiscovery
    {
        protected System.Collections.Generic.Dictionary<string, object> Meta = new System.Collections.Generic.Dictionary<string, object>();

        private void
        findvswhere()
        {
            const string vswhere_version = "vswhere.2.4.1"; // match packages.config in the Bam project

            // find vswhere from the NuGet package download
            var nugetDir = Bam.Core.Graph.Instance.ProcessState.NuGetDirectory;
            var vswhere_dir = System.IO.Path.Combine(nugetDir, vswhere_version);
            if (!System.IO.Directory.Exists(vswhere_dir))
            {
                throw new Bam.Core.Exception("Unable to locate NuGet package for {0} at {1}", vswhere_version, vswhere_dir);
            }
            var vswhere_tools_dir = System.IO.Path.Combine(vswhere_dir, "tools");
            var vswhere_exe_path = System.IO.Path.Combine(vswhere_tools_dir, "vswhere.exe");
            if (!System.IO.File.Exists(vswhere_exe_path))
            {
                throw new Bam.Core.Exception("Unable to locate vswhere.exe from NuGet package at '{0}'", vswhere_exe_path);
            }
            this.vswherePath = vswhere_exe_path;
        }

        private static string
        RunExecutable(
            string executable,
            string arguments)
        {
            var processStartInfo = new System.Diagnostics.ProcessStartInfo();
            processStartInfo.FileName = executable;
            processStartInfo.Arguments = arguments;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true; // swallow
            processStartInfo.UseShellExecute = false;
            System.Diagnostics.Process process = System.Diagnostics.Process.Start(processStartInfo);
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                return null;
            }
            return process.StandardOutput.ReadToEnd().TrimEnd(System.Environment.NewLine.ToCharArray());
        }

        protected string
        vswhere_getinstallpath()
        {
            string installpath = System.String.Empty;
            if (this.major_version >= 15)
            {
                installpath = RunExecutable(this.vswherePath, System.String.Format("-property installationPath -version {0}", this.major_version));
                if (System.String.IsNullOrEmpty(installpath))
                {
                    throw new Bam.Core.Exception("Unable to locate installation directory for Visual Studio major version {0}", this.major_version);
                }
            }
            else
            {
                installpath = RunExecutable(this.vswherePath, System.String.Format("-legacy -property installationPath -version [{0}]", this.major_version));
                if (System.String.IsNullOrEmpty(installpath))
                {
                    throw new Bam.Core.Exception("Unable to locate installation directory for Visual Studio major version {0}", this.major_version);
                }
            }
            Bam.Core.Log.Info("Using VisualStudio {0} installed at {1}", this.major_version, installpath);
            return installpath;
        }

        private System.Collections.Generic.Dictionary<string, Bam.Core.TokenizedStringArray>
        execute_vcvars(
            C.EBit depth,
            bool has64bithost_32bitcross,
            bool hasNative64BitTools,
            Bam.Core.StringArray inherited_envvars,
            System.Collections.Generic.Dictionary<string, Bam.Core.StringArray> required_envvars
        )
        {
            var startinfo = new System.Diagnostics.ProcessStartInfo();
            startinfo.FileName = @"c:\Windows\System32\cmd.exe";
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
                        var updated_value = System.String.Format(
                            "{0};{1}",
                            System.Environment.ExpandEnvironmentVariables(required.Value.ToString(';')),
                            existing_value
                        );
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

            System.Func<string> vcvarsall_command = () =>
            {
                var command_and_args = new System.Text.StringBuilder();
                command_and_args.Append("vcvarsall.bat ");
                switch (depth)
                {
                    case C.EBit.ThirtyTwo:
                        {
                            if (Bam.Core.OSUtilities.Is64BitHosting && has64bithost_32bitcross)
                            {
                                command_and_args.Append("amd64_x86 ");
                            }
                            else
                            {
                                command_and_args.Append("x86 ");
                            }
                        }
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
                var visualC = Bam.Core.Graph.Instance.Packages.First(item => item.Name == "VisualC");
                var defaultWindowsSDKVersion = visualC.Dependents.First(item => item.Item1 == "WindowsSDK").Item2;
                var windowsSDK = Bam.Core.Graph.Instance.Packages.FirstOrDefault(item => item.Name == "WindowsSDK");
                if (null != windowsSDK)
                {
                    if (windowsSDK.Version != defaultWindowsSDKVersion)
                    {
                        command_and_args.Append(System.String.Format("{0} ", windowsSDK.Version));
                    }
                    else
                    {
                        var option_type = System.Type.GetType("WindowsSDK.Options.WindowsSDK10Version", throwOnError: false);
                        if (null != option_type)
                        {
                            var option_type_instance = System.Activator.CreateInstance(option_type) as Bam.Core.IStringCommandLineArgument;
                            if (null != option_type_instance)
                            {
                                var win10Option = Bam.Core.CommandLineProcessor.Evaluate(option_type_instance);
                                if (null != win10Option)
                                {
                                    command_and_args.Append(System.String.Format("{0} ", win10Option));
                                }
                            }
                        }
                    }
                }
                return command_and_args.ToString();
            };

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
                startinfo.WorkingDirectory = System.IO.Path.Combine(this.InstallDir.ToString(), subpath_to_vcvars);
                environment_generator_cmdline = vcvarsall_command();
            }

            // allow the WindowsSDK to override the VisualStudio project's PlatformToolset
            if (windowssdk_meta.Contains("PlatformToolset"))
            {
                var vc_meta = Bam.Core.Graph.Instance.PackageMetaData<VisualC.MetaData>("VisualC");
                vc_meta.PlatformToolset = windowssdk_meta["PlatformToolset"] as string;
            }

            var arguments = new System.Text.StringBuilder();
            arguments.AppendFormat("/C {0} && SET", environment_generator_cmdline);
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
                throw new Bam.Core.Exception("{0} failed: {1}", environment_generator_cmdline, stderr.ToString());
            }

            var env = new System.Collections.Generic.Dictionary<string, Bam.Core.TokenizedStringArray>();
            var lines = stdout.ToString().Split(
                new[] { System.Environment.NewLine },
                System.StringSplitOptions.RemoveEmptyEntries
            );
            foreach (var line in lines)
            {
                Bam.Core.Log.DebugMessage("{0}->{1}", environment_generator_cmdline, line);
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
            Bam.Core.Log.Info(@"Generating {0}-bit build environment using '{1}\{2}'",
                (int)depth,
                startinfo.WorkingDirectory,
                environment_generator_cmdline.TrimEnd()
            );
            foreach (System.Collections.Generic.KeyValuePair<string, Bam.Core.TokenizedStringArray> entry in env)
            {
                Bam.Core.Log.DebugMessage("\t{0} = {1}", entry.Key, entry.Value.ToString(';'));
            }
            return env;
        }

        protected void
        get_tool_environment_variables(
            C.EBit depth,
            bool has64bithost_32bitcross,
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
            // for WindowsSDK-7.1
            if (null == inherited_envvars)
            {
                inherited_envvars = new Bam.Core.StringArray();
            }
            inherited_envvars.AddUnique("PROCESSOR_ARCHITECTURE");

            var env = this.execute_vcvars(
                depth,
                has64bithost_32bitcross,
                hasNative64BitTools,
                inherited_envvars,
                required_envvars
            );
            this.Meta.Add(EnvironmentKey(depth), env);
        }

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
                    throw new Bam.Core.Exception("Unknown bit depth, {0}", depth.ToString());
            }
        }

        public System.Collections.Generic.Dictionary<string, Bam.Core.TokenizedStringArray>
        Environment(
            C.EBit depth)
        {
            return this.Meta[EnvironmentKey(depth)] as System.Collections.Generic.Dictionary<string, Bam.Core.TokenizedStringArray>; 
        }

        private string vswherePath
        {
            get;
            set;
        }

        protected abstract int major_version
        {
            get;
        }

        protected abstract string subpath_to_vcvars
        {
            get;
        }

        protected virtual bool has64bithost_32bitcross
        {
            get
            {
                return true;
            }
        }

        protected virtual bool hasNative64BitTools
        {
            get
            {
                return true;
            }
        }

        private static bool report_WindowsSDK_done = false;
        private static void
        report_WindowsSDK(
            System.Collections.Generic.Dictionary<string, Bam.Core.TokenizedStringArray> env)
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
                report.AppendFormat("version {0} ", env["WindowsSDKVersion"].ToString());
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
                throw new Bam.Core.Exception("Unable to locate WindowsSDK installation directory environment variable");
            }
            report.AppendFormat("installed at {0} ", winsdk_installdir);
            if (env.ContainsKey("UniversalCRTSdkDir") && env.ContainsKey("UCRTVersion"))
            {
                var ucrt_installdir = env["UniversalCRTSdkDir"].ToString();
                if (ucrt_installdir != winsdk_installdir)
                {
                    report.AppendFormat("with UniversalCRT SDK {0} installed at {1} ",
                        env["UCRTVersion"].ToString(),
                        ucrt_installdir
                    );
                }
            }
            Bam.Core.Log.Info(report.ToString());
        }

        void
        C.IToolchainDiscovery.discover(
            C.EBit? depth)
        {
            if (null == this.vswherePath)
            {
                this.findvswhere();
            }
            if (!this.Meta.ContainsKey("InstallDir"))
            {
                var install_dir = this.vswhere_getinstallpath();
                this.InstallDir = Bam.Core.TokenizedString.CreateVerbatim(install_dir);
            }
            var bitdepth = depth.Value;
            if (!this.Meta.ContainsKey(EnvironmentKey(bitdepth)))
            {
                this.get_tool_environment_variables(
                    bitdepth,
                    this.has64bithost_32bitcross,
                    this.hasNative64BitTools
                );
                report_WindowsSDK(this.Environment(bitdepth));
            }
        }
    }
}
