// <copyright file="GccDetailGatherer.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ComposerXECommon package</summary>
// <author>Mark Final</author>
namespace ComposerXECommon
{
    public class GccDetailGatherer
    {
        private static System.Collections.Generic.Dictionary<Opus.Core.BaseTarget, GccDetailData> gccDetailsForTarget = new System.Collections.Generic.Dictionary<Opus.Core.BaseTarget, GccDetailData>();

        public static GccDetailData
        DetermineSpecs(
            Opus.Core.BaseTarget baseTarget,
            Opus.Core.IToolset toolset)
        {
            // get version
            string gccVersion = null;
            {
                var processStartInfo = new System.Diagnostics.ProcessStartInfo();
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

            // get target
            string gccTarget = null;
            {
                var processStartInfo = new System.Diagnostics.ProcessStartInfo();
                processStartInfo.FileName = toolset.Tool(typeof(C.ICompilerTool)).Executable(baseTarget);
                processStartInfo.ErrorDialog = true;
                processStartInfo.UseShellExecute = false;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.Arguments = "-dumpmachine";

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

                gccTarget = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                gccTarget = gccTarget.Trim();
            }

            // get paths and targets
            string pathPrefix = null;
            string gxxIncludeDir = null;
            string libDir = null;
            var includePaths = new Opus.Core.StringArray();
            {
                var processStartInfo = new System.Diagnostics.ProcessStartInfo();
                processStartInfo.FileName = toolset.Tool(typeof(C.ICompilerTool)).Executable(baseTarget);
                processStartInfo.ErrorDialog = true;
                processStartInfo.UseShellExecute = false;
                processStartInfo.RedirectStandardOutput = true;
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

                var details = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                var splitDetails = details.Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.None);

                foreach (var detail in splitDetails)
                {
                    const string configuredWith = "Configured with: ";
                    if (detail.StartsWith(configuredWith))
                    {
                        var configuredOptions = detail.Substring(configuredWith.Length);
                        var splitConfigureOptions = configuredOptions.Split(' ');

                        const string pathPrefixKey = "--prefix=";
                        const string gxxIncludeDirKey = "--with-gxx-include-dir=";
                        const string targetKey = "--target=";
                        const string libexecKey = "--libexecdir=";
                        const string slibDirKey = "--with-slibdir=";
                        foreach (var option in splitConfigureOptions)
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
                                if (null != libDir)
                                {
                                    throw new Opus.Core.Exception("lib dir already defined");
                                }
                                libDir = option.Substring(libexecKey.Length).Trim();
                            }
                            else if (option.StartsWith(slibDirKey))
                            {
                                if (null != libDir)
                                {
                                    throw new Opus.Core.Exception("lib dir already defined");
                                }
                                libDir = option.Substring(slibDirKey.Length).Trim();
                            }
                        }

                        break;
                    }
                }

                if (null == gccTarget)
                {
                    foreach (var detail in splitDetails)
                    {
                        var targetKey = "Target: ";
                        if (detail.StartsWith(targetKey))
                        {
                            gccTarget = detail.Substring(targetKey.Length).Trim();
                        }
                    }
                }

                if ((null != gxxIncludeDir) && !gxxIncludeDir.StartsWith(pathPrefix))
                {
                    // remove any prefix directory separator so that Combine works
                    gxxIncludeDir = gxxIncludeDir.TrimStart(System.IO.Path.DirectorySeparatorChar);
                    gxxIncludeDir = System.IO.Path.Combine(pathPrefix, gxxIncludeDir);
                }

                // C include paths (http://gcc.gnu.org/onlinedocs/cpp/Search-Path.html)
                includePaths.Add("/usr/local/include");
                includePaths.Add("/usr/include");

                // TODO: this looks like the targetIncludeFolder, and has been necessary
                {
                    // this is for some Linux distributions
                    var path = System.String.Format("/usr/include/{0}", gccTarget);
                    if (System.IO.Directory.Exists(path))
                    {
                        includePaths.Add(path);
                    }
                }
            }

            var gccDetails = new GccDetailData(gccVersion, includePaths, gxxIncludeDir, gccTarget, libDir);
            gccDetailsForTarget[baseTarget] = gccDetails;

            Opus.Core.Log.DebugMessage("Gcc version for target '{0}' is '{1}'", baseTarget.ToString(), gccDetails.Version);
            Opus.Core.Log.DebugMessage("Gcc machine type for target '{0}' is '{1}'", baseTarget.ToString(), gccDetails.Target);
            Opus.Core.Log.DebugMessage("Gxx include path for target '{0}' is '{1}'", baseTarget.ToString(), gccDetails.GxxIncludePath);

            return gccDetails;
        }
    }
}
