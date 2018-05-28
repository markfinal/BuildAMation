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
namespace VisualCCommon
{
    public abstract class MetaData :
        Bam.Core.PackageMetaData
    {
        protected System.Collections.Generic.Dictionary<string, object> Meta = new System.Collections.Generic.Dictionary<string,object>();

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
        vswhere_getinstallpath(
            int vs_major_version)
        {
            if (vs_major_version >= 15)
            {
                var result = RunExecutable(this.vswherePath, System.String.Format("-property installationPath -version {0}", vs_major_version));
                if (System.String.IsNullOrEmpty(result))
                {
                    throw new Bam.Core.Exception("Unable to locate installation directory for Visual Studio major version {0}", vs_major_version);
                }
                Bam.Core.Log.Info("Using VisualStudio {0} installed at {1}", vs_major_version, result);
                return result;
            }
            else
            {
                var result = RunExecutable(this.vswherePath, System.String.Format("-legacy -property installationPath -version [{0}]", vs_major_version));
                if (System.String.IsNullOrEmpty(result))
                {
                    throw new Bam.Core.Exception("Unable to locate installation directory for Visual Studio major version {0}", vs_major_version);
                }
                Bam.Core.Log.Info("Using VisualStudio {0} installed at {1}", vs_major_version, result);
                return result;
            }
        }

        private System.Collections.Generic.Dictionary<string, Bam.Core.TokenizedStringArray>
        execute_vcvars(
            string subpath_to_vcvars,
            bool target64bit,
            bool has64bithost_32bitcross,
            bool hasNative64BitTools,
            Bam.Core.StringArray inherited_envvars,
            System.Collections.Generic.Dictionary<string, Bam.Core.StringArray> required_envvars
        )
        {
            var startinfo = new System.Diagnostics.ProcessStartInfo();
            startinfo.FileName = @"c:\Windows\System32\cmd.exe";
            startinfo.WorkingDirectory = System.IO.Path.Combine(this.InstallDir.ToString(), subpath_to_vcvars);
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
                    startinfo.EnvironmentVariables.Add(required.Key, System.Environment.ExpandEnvironmentVariables(required.Value.ToString(';')));
                }
            }
            startinfo.UseShellExecute = false;
            startinfo.RedirectStandardInput = true;
            startinfo.RedirectStandardOutput = true;
            startinfo.RedirectStandardError = true;

            System.Func<string> command = () =>
            {
                if (target64bit)
                {
                    if (Bam.Core.OSUtilities.Is64BitHosting && hasNative64BitTools)
                    {
                        return "vcvarsall.bat amd64";
                    }
                    else
                    {
                        return "vcvarsall.bat x86_amd64";
                    }
                }
                else
                {
                    if (Bam.Core.OSUtilities.Is64BitHosting && has64bithost_32bitcross)
                    {
                        return "vcvarsall.bat amd64_x86";
                    }
                    else
                    {
                        return "vcvarsall.bat x86";
                    }
                }
            };

            var vcvarsall_cmd = command();

            var arguments = new System.Text.StringBuilder();
            arguments.AppendFormat("/C {0} && SET", vcvarsall_cmd);
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
                throw new Bam.Core.Exception("{0} failed: {1}", vcvarsall_cmd, stderr.ToString());
            }

            var env = new System.Collections.Generic.Dictionary<string, Bam.Core.TokenizedStringArray>();
            var lines = stdout.ToString().Split(
                new[] { System.Environment.NewLine },
                System.StringSplitOptions.RemoveEmptyEntries
            );
            foreach (var line in lines)
            {
                Bam.Core.Log.DebugMessage("{0}->{1}", vcvarsall_cmd, line);
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
            Bam.Core.Log.DebugMessage(@"Running {0}\{1} gives the following environment variables:", startinfo.WorkingDirectory, vcvarsall_cmd);
            foreach (System.Collections.Generic.KeyValuePair<string,Bam.Core.TokenizedStringArray> entry in env)
            {
                Bam.Core.Log.DebugMessage("\t{0} = {1}", entry.Key, entry.Value.ToString(';'));
            }
            return env;
        }

        protected void
        get_tool_environment_variables(
            string subpath_to_vcvars,
            bool has64bithost_32bitcross = true,
            bool hasNative64BitTools = true,
            Bam.Core.StringArray inherited_envvars = null,
            System.Collections.Generic.Dictionary<string, Bam.Core.StringArray> required_envvars = null)
        {
            if (null == inherited_envvars)
            {
                inherited_envvars = new Bam.Core.StringArray();
            }
            inherited_envvars.AddUnique("PATH"); // required to run 'reg' to query for things like the WindowsSDK
            this.Environment32 = this.execute_vcvars(
                subpath_to_vcvars,
                false,
                has64bithost_32bitcross,
                hasNative64BitTools,
                inherited_envvars,
                required_envvars
            );
            this.Environment64 = this.execute_vcvars(
                subpath_to_vcvars,
                true,
                has64bithost_32bitcross,
                hasNative64BitTools,
                inherited_envvars,
                required_envvars
            );
        }

        public MetaData()
        {
            if (!Bam.Core.OSUtilities.IsWindowsHosting)
            {
                return;
            }
            this.findvswhere();
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

        public System.Collections.Generic.Dictionary<string, Bam.Core.TokenizedStringArray>
        Environment32
        {
            get
            {
                try
                {
                    return this.Meta["Environment32"] as System.Collections.Generic.Dictionary<string, Bam.Core.TokenizedStringArray>;
                }
                catch (System.Collections.Generic.KeyNotFoundException)
                {
                    throw new Bam.Core.Exception("32-bit environment has not been configured");
                }
            }

            private set
            {
                this.Meta["Environment32"] = value;
            }
        }

        public System.Collections.Generic.Dictionary<string, Bam.Core.TokenizedStringArray>
        Environment64
        {
            get
            {
                try
                {
                    return this.Meta["Environment64"] as System.Collections.Generic.Dictionary<string, Bam.Core.TokenizedStringArray>;
                }
                catch (System.Collections.Generic.KeyNotFoundException)
                {
                    throw new Bam.Core.Exception("64-bit environment has not been configured");
                }
            }

            private set
            {
                this.Meta["Environment64"] = value;
            }
        }

        private string vswherePath
        {
            get;
            set;
        }
    }
}
