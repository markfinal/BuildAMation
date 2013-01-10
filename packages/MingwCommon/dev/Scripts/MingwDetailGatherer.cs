// <copyright file="MingwDetailGatherer.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace MingwCommon
{
    public class MingwDetailGatherer
    {
        private static System.Collections.Generic.Dictionary<Opus.Core.BaseTarget, MingwDetailData> gccDetailsForTarget = new System.Collections.Generic.Dictionary<Opus.Core.BaseTarget, MingwDetailData>();

        public static MingwDetailData DetermineSpecs(Opus.Core.BaseTarget baseTarget, Opus.Core.IToolset toolset)
        {
            // get version
            string gccVersion = null;
            {
                System.Diagnostics.ProcessStartInfo processStartInfo = new System.Diagnostics.ProcessStartInfo();
                processStartInfo.FileName = toolset.Tool(typeof(C.ICompilerTool)).Executable(baseTarget);
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
                    throw new Opus.Core.Exception("'{0}': process filename '{1}'", ex.Message, processStartInfo.FileName);
                }

                if (null == process)
                {
                    throw new Opus.Core.Exception("Unable to execute '{0}'", processStartInfo.FileName);
                }

                gccVersion = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                gccVersion = gccVersion.Trim();
            }

            // get paths and targets
            string pathPrefix = null;
            string gxxIncludeDir = null;
            string gccTarget = null;
            string libExecDir = null;
            Opus.Core.StringArray includePaths = new Opus.Core.StringArray();
            {
                System.Diagnostics.ProcessStartInfo processStartInfo = new System.Diagnostics.ProcessStartInfo();
                processStartInfo.FileName = toolset.Tool(typeof(C.ICompilerTool)).Executable(baseTarget);
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
                    throw new Opus.Core.Exception("'{0}': process filename '{1}'", ex.Message, processStartInfo.FileName);
                }

                if (null == process)
                {
                    throw new Opus.Core.Exception("Unable to execute '{0}'", processStartInfo.FileName);
                }

                string details = process.StandardError.ReadToEnd();
                process.WaitForExit();

                string[] splitDetails = details.Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.None);

                foreach (string detail in splitDetails)
                {
                    const string configuredWith = "Configured with: ";
                    if (detail.StartsWith(configuredWith))
                    {
                        string configuredOptions = detail.Substring(configuredWith.Length);
                        string[] splitConfigureOptions = configuredOptions.Split(' ');

                        const string pathPrefixKey = "--prefix=";
                        const string gxxIncludeDirKey = "--with-gxx-include-dir=";
                        const string targetKey = "--target=";
                        const string libexecKey = "--libexecdir=";
                        foreach (string option in splitConfigureOptions)
                        {
                            if (option.StartsWith(pathPrefixKey))
                            {
                                pathPrefix = option.Substring(pathPrefixKey.Length).Trim(); ;
                            }
                            else if (option.StartsWith(gxxIncludeDirKey))
                            {
                                gxxIncludeDir = option.Substring(gxxIncludeDirKey.Length).Trim();
                            }
                            else if (option.StartsWith(targetKey))
                            {
                                gccTarget = option.Substring(targetKey.Length).Trim();
                            }
                            else if (option.StartsWith(libexecKey))
                            {
                                libExecDir = option.Substring(libexecKey.Length).Trim();
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

                if (null != gxxIncludeDir && !gxxIncludeDir.StartsWith(pathPrefix))
                {
                    // remove any prefix directory separator so that Combine works
                    gxxIncludeDir = gxxIncludeDir.TrimStart(System.IO.Path.DirectorySeparatorChar);
                    gxxIncludeDir = System.IO.Path.Combine(pathPrefix, gxxIncludeDir);
                }

                // C include paths
                string installPath = toolset.InstallPath(baseTarget);
                string gccIncludeFolder = System.IO.Path.Combine(installPath, "lib");
                gccIncludeFolder = System.IO.Path.Combine(gccIncludeFolder, "gcc");
                gccIncludeFolder = System.IO.Path.Combine(gccIncludeFolder, gccTarget);
                gccIncludeFolder = System.IO.Path.Combine(gccIncludeFolder, gccVersion);
                gccIncludeFolder = System.IO.Path.Combine(gccIncludeFolder, "include");

                includePaths.Add(System.IO.Path.Combine(installPath, "include"));
                includePaths.Add(gccIncludeFolder);
            }

            MingwDetailData gccDetails = new MingwDetailData(gccVersion, includePaths, gxxIncludeDir, gccTarget, libExecDir);
            gccDetailsForTarget[baseTarget] = gccDetails;

            Opus.Core.Log.DebugMessage("Mingw version for target '{0}' is '{1}'", baseTarget.ToString(), gccDetails.Version);
            Opus.Core.Log.DebugMessage("Mingw machine type for target '{0}' is '{1}'", baseTarget.ToString(), gccDetails.Target);
            Opus.Core.Log.DebugMessage("Mingw include path for target '{0}' is '{1}'", baseTarget.ToString(), gccDetails.GxxIncludePath);

            return gccDetails;
        }
    }
}
