// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Gcc package</summary>
// <author>Mark Final</author>
namespace Gcc
{
    public class GccDetails
    {
        public GccDetails(string version,
                          string gxxIncludePath,
                          string target)
        {
            if (null == version)
            {
                throw new Opus.Core.Exception("Unable to determine Gcc version", false);
            }
            if (null == target)
            {
                throw new Opus.Core.Exception("Unable to determine Gcc target", false);
            }

            this.Version = version;
            this.GxxIncludePath = gxxIncludePath;
            this.Target = target;
        }

        public string Version
        {
            get;
            private set;
        }

        public string GxxIncludePath
        {
            get;
            private set;
        }

        public string Target
        {
            get;
            private set;
        }
    }

    public sealed class Toolset : GccCommon.Toolset
    {
        private static System.Collections.Generic.Dictionary<Opus.Core.Target, GccDetails> gccDetailsForTarget = new System.Collections.Generic.Dictionary<Opus.Core.Target, GccDetails>();

        private GccDetails GetGccDetails(Opus.Core.Target target)
        {
            string pathPrefix = null;
            string gxxIncludeDir = null;
            string gccTarget = null;
            string gccVersion = null;

            // get version
            {
                System.Diagnostics.ProcessStartInfo processStartInfo = new System.Diagnostics.ProcessStartInfo();
                processStartInfo.FileName = this.toolMap[typeof(C.ICompilerTool)].Executable(target);
                processStartInfo.ErrorDialog = true;
                processStartInfo.UseShellExecute = false;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.Arguments = "-dumpversion";

                System.Diagnostics.Process process = null;
                try
                {
                    process = System.Diagnostics.Process.Start(processStartInfo);
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    throw new Opus.Core.Exception(System.String.Format("'{0}': process filename '{1}'", ex.Message, processStartInfo.FileName), false);
                }

                if (null == process)
                {
                    throw new Opus.Core.Exception(System.String.Format("Unable to execute '{0}'", processStartInfo.FileName), false);
                }

                gccVersion = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                gccVersion = gccVersion.Trim();
            }

            // get paths and targets
            {
                System.Diagnostics.ProcessStartInfo processStartInfo = new System.Diagnostics.ProcessStartInfo();
                processStartInfo.FileName = this.toolMap[typeof(C.ICompilerTool)].Executable(target);
                processStartInfo.ErrorDialog = true;
                processStartInfo.UseShellExecute = false;
                processStartInfo.RedirectStandardError = true;
                processStartInfo.Arguments = "-v";

                System.Diagnostics.Process process = null;
                try
                {
                    process = System.Diagnostics.Process.Start(processStartInfo);
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    throw new Opus.Core.Exception(System.String.Format("'{0}': process filename '{1}'", ex.Message, processStartInfo.FileName), false);
                }

                if (null == process)
                {
                    throw new Opus.Core.Exception(System.String.Format("Unable to execute '{0}'", processStartInfo.FileName), false);
                }

                string details = process.StandardError.ReadToEnd();
                process.WaitForExit();

                string[] splitDetails = details.Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.None);

                foreach (string detail in splitDetails)
                {
                    string configuredWith = "Configured with: ";
                    if (detail.StartsWith(configuredWith))
                    {
                        string configuredOptions = detail.Substring(configuredWith.Length);
                        string[] splitConfigureOptions = configuredOptions.Split(' ');

                        const string pathPrefixKey = "--prefix=";
                        const string gxxIncludeDirKey = "--with-gxx-include-dir=";
                        const string targetKey = "--target=";
                        foreach (string option in splitConfigureOptions)
                        {
                            if (option.StartsWith(pathPrefixKey))
                            {
                                pathPrefix = option.Substring(pathPrefixKey.Length).Trim();;
                            }
                            else if (option.StartsWith(gxxIncludeDirKey))
                            {
                                gxxIncludeDir = option.Substring(gxxIncludeDirKey.Length).Trim();
                            }
                            else if (option.StartsWith(targetKey))
                            {
                                gccTarget = option.Substring(targetKey.Length).Trim();
                            }
                        }

                        break;
                    }
                }

                if (null == gccTarget)
                {
                    foreach (string detail in splitDetails)
                    {
                        string targetKey = "Target: ";
                        if (detail.StartsWith(targetKey))
                        {
                            gccTarget = detail.Substring(targetKey.Length).Trim();
                        }
                    }
                }

                if (!gxxIncludeDir.StartsWith(pathPrefix))
                {
                    // remove any prefix directory separator so that Combine works
                    gxxIncludeDir = gxxIncludeDir.TrimStart(System.IO.Path.DirectorySeparatorChar);
                    gxxIncludeDir = System.IO.Path.Combine(pathPrefix, gxxIncludeDir);
                }
            }

            GccDetails gccDetails = new GccDetails(gccVersion, gxxIncludeDir, gccTarget);
            gccDetailsForTarget[target] = gccDetails;

            Opus.Core.Log.DebugMessage("Gcc version for target '{0}' is '{1}'", target.ToString(), gccDetails.Version);
            Opus.Core.Log.DebugMessage("Gcc machine type for target '{0}' is '{1}'", target.ToString(), gccDetails.Target);
            Opus.Core.Log.DebugMessage("Gxx include path for target '{0}' is '{1}'", target.ToString(), gccDetails.GxxIncludePath);

            return gccDetails;
        }

        public Toolset()
        {
            this.toolMap[typeof(C.ICompilerTool)] = new CCompiler(this);
            this.toolMap[typeof(C.ICxxCompilerTool)] = new CxxCompiler(this);
            this.toolMap[typeof(C.ILinkerTool)] = new Linker(this);
            this.toolMap[typeof(C.IArchiverTool)] = new GccCommon.Archiver(this);

            this.toolOptionsMap[typeof(C.ICompilerTool)] = typeof(Gcc.CCompilerOptionCollection);
            this.toolOptionsMap[typeof(C.ICxxCompilerTool)] = typeof(Gcc.CPlusPlusCompilerOptionCollection);
            this.toolOptionsMap[typeof(C.ILinkerTool)] = typeof(Gcc.LinkerOptionCollection);
            this.toolOptionsMap[typeof(C.IArchiverTool)] = typeof(Gcc.ArchiverOptionCollection);
        }

        protected override void GetInstallPath(Opus.Core.BaseTarget baseTarget)
        {
            if (null != this.installPath)
            {
                return;
            }

            string installPath = null;
            if (Opus.Core.State.HasCategory("Gcc") && Opus.Core.State.Has("Gcc", "InstallPath"))
            {
                installPath = Opus.Core.State.Get("Gcc", "InstallPath") as string;
                Opus.Core.Log.DebugMessage("Gcc install path set from command line to '{0}'", installPath);
            }

            if (null == installPath)
            {
                installPath = "/usr/bin";
            }

            this.installPath = installPath;

            GccDetails details = this.GetGccDetails(Opus.Core.Target.GetInstance(baseTarget, "TODO", this));

            // C include paths
            this.includePaths.Add("/usr/include");
            {
                // this is for some Linux distributions
                string path = System.String.Format("/usr/include/{0}", details.Target);
                if (System.IO.Directory.Exists(path))
                {
                    this.includePaths.Add(path);
                }
            }
            string gccLibFolder = System.String.Format("/usr/lib/gcc/{0}/{1}", details.Target, details.Version);
            string gccIncludeFolder = System.String.Format("{0}/include", gccLibFolder);
            string gccIncludeFixedFolder = System.String.Format("{0}/include-fixed", gccLibFolder);

            if (!System.IO.Directory.Exists(gccIncludeFolder))
            {
                throw new Opus.Core.Exception(System.String.Format("Gcc include folder '{0}' does not exist", gccIncludeFolder), false);
            }
            this.includePaths.Add(gccIncludeFolder);
            
            if (!System.IO.Directory.Exists(gccIncludeFolder))
            {
                throw new Opus.Core.Exception(System.String.Format("Gcc include folder '{0}' does not exist", gccIncludeFixedFolder), false);
            }
            this.includePaths.Add(gccIncludeFixedFolder);

            // C++ include paths
            this.cxxIncludePath = details.GxxIncludePath;
        }

        protected override string GetVersion (Opus.Core.BaseTarget baseTarget)
        {
            return "4.4";
        }

        public override string GetMachineType(Opus.Core.BaseTarget baseTarget)
        {
            Opus.Core.Target target = Opus.Core.Target.GetInstance(baseTarget, "TODO", this);

            if (!gccDetailsForTarget.ContainsKey(target))
            {
                this.GetGccDetails(target);
            }

            return gccDetailsForTarget[target].Target;
        }
    }
}
