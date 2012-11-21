// <copyright file="GccDetailGatherer.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    public class GccDetailGatherer
    {
        private static System.Collections.Generic.Dictionary<Opus.Core.Target, GccDetailData> gccDetailsForTarget = new System.Collections.Generic.Dictionary<Opus.Core.Target, GccDetailData>();

        // TODO: change this to BaseTarget and an IToolset as arguments
        // requires Executable to be changed first
        public static GccDetailData GetGccDetails(Opus.Core.Target target)
        {
            Opus.Core.IToolset toolset = target.Toolset;

            string pathPrefix = null;
            string gxxIncludeDir = null;
            string gccTarget = null;
            string gccVersion = null;

            // get version
            {
                System.Diagnostics.ProcessStartInfo processStartInfo = new System.Diagnostics.ProcessStartInfo();
                processStartInfo.FileName = toolset.Tool(typeof(C.ICompilerTool)).Executable(target);
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
                processStartInfo.FileName = toolset.Tool(typeof(C.ICompilerTool)).Executable(target);
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

            GccDetailData gccDetails = new GccDetailData(gccVersion, gxxIncludeDir, gccTarget);
            gccDetailsForTarget[target] = gccDetails;

            Opus.Core.Log.DebugMessage("Gcc version for target '{0}' is '{1}'", target.ToString(), gccDetails.Version);
            Opus.Core.Log.DebugMessage("Gcc machine type for target '{0}' is '{1}'", target.ToString(), gccDetails.Target);
            Opus.Core.Log.DebugMessage("Gxx include path for target '{0}' is '{1}'", target.ToString(), gccDetails.GxxIncludePath);

            return gccDetails;
        }
    }
}
