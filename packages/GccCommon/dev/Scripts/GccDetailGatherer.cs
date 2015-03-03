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
namespace GccCommon
{
    public class GccDetailGatherer
    {
        private static System.Collections.Generic.Dictionary<Bam.Core.BaseTarget, GccDetailData> gccDetailsForTarget = new System.Collections.Generic.Dictionary<Bam.Core.BaseTarget, GccDetailData>();

        public static GccDetailData
        DetermineSpecs(
            Bam.Core.BaseTarget baseTarget,
            Bam.Core.IToolset toolset)
        {
            var executable = toolset.Tool(typeof(C.ICompilerTool)).Executable(baseTarget);
            var isLLVMGcc = System.IO.Path.GetFileName(executable).StartsWith("llvm");

            // get version
            string gccVersion = null;
            {
                var processStartInfo = new System.Diagnostics.ProcessStartInfo();
                processStartInfo.FileName = executable;
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
                    throw new Bam.Core.Exception("'{0}': process filename '{1}'", ex.Message, processStartInfo.FileName);
                }

                if (null == process)
                {
                    throw new Bam.Core.Exception("Unable to execute '{0}'", processStartInfo.FileName);
                }

                gccVersion = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                gccVersion = gccVersion.Trim();
            }

            // get paths and targets
            string pathPrefix = null;
            string gxxIncludeDir = null;
            string gccTarget = null;
            string libDir = null;
            var includePaths = new Bam.Core.StringArray();
            {
                var processStartInfo = new System.Diagnostics.ProcessStartInfo();
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
                    throw new Bam.Core.Exception("'{0}': process filename '{1}'", ex.Message, processStartInfo.FileName);
                }

                if (null == process)
                {
                    throw new Bam.Core.Exception("Unable to execute '{0}'", processStartInfo.FileName);
                }

                var details = process.StandardError.ReadToEnd();
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
                        const string buildKey = "--build=";
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
                            else if (option.StartsWith(buildKey))
                            {
                                gccTarget = option.Substring(buildKey.Length).Trim();
                            }
                            else if (option.StartsWith(libexecKey))
                            {
                                if (null != libDir)
                                {
                                    throw new Bam.Core.Exception("lib dir already defined");
                                }
                                libDir = option.Substring(libexecKey.Length).Trim();
                            }
                            else if (option.StartsWith(slibDirKey))
                            {
                                if (null != libDir)
                                {
                                    throw new Bam.Core.Exception("lib dir already defined");
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
                        string targetKey = "Target: ";
                        if (detail.StartsWith(targetKey))
                        {
                            gccTarget = detail.Substring(targetKey.Length).Trim();
                        }
                    }
                }

                if (!isLLVMGcc)
                {
                    if (string.IsNullOrEmpty(gxxIncludeDir))
                    {
                        // default
                        gxxIncludeDir = System.String.Format("/usr/include/c++/{0}", gccVersion);
                    }

                    if (!gxxIncludeDir.StartsWith(pathPrefix))
                    {
                        // remove any prefix directory separator so that Combine works
                        gxxIncludeDir = gxxIncludeDir.TrimStart(System.IO.Path.DirectorySeparatorChar);
                        gxxIncludeDir = System.IO.Path.Combine(pathPrefix, gxxIncludeDir);
                    }
                }

                // C include paths (http://gcc.gnu.org/onlinedocs/cpp/Search-Path.html)
                if (string.IsNullOrEmpty(libDir))
                {
                    libDir = "/usr/lib";
                }

                includePaths.Add("/usr/local/include");
                var gccLibFolder = System.String.Format("{0}/gcc/{1}/{2}", libDir, gccTarget, gccVersion);
                var gccIncludeFolder = System.String.Format("{0}/include", gccLibFolder);
                var gccIncludeFixedFolder = System.String.Format("{0}/include-fixed", gccLibFolder);

                if (!isLLVMGcc)
                {
                    if (!System.IO.Directory.Exists(gccIncludeFolder))
                    {
                        throw new Bam.Core.Exception("Gcc include folder '{0}' does not exist", gccIncludeFolder);
                    }
                    includePaths.Add(gccIncludeFolder);
                }
                else
                {
                    gccIncludeFolder = System.String.Format("{0}/lib/gcc/{1}/{2}/include", pathPrefix, gccTarget, gccVersion);
                    if (!System.IO.Directory.Exists(gccIncludeFolder))
                    {
                        throw new Bam.Core.Exception("Gcc include folder '{0}' does not exist", gccIncludeFolder);
                    }
                    includePaths.Add(gccIncludeFolder);
                }

                // OSX does not have this path
                if (!baseTarget.HasPlatform(Bam.Core.EPlatform.OSX))
                {
                    // TODO: decide whether this is necessary, as apparently it's an implementation detail (http://sourceware.org/ml/crossgcc/2008-11/msg00028.html)
                    if (System.IO.Directory.Exists(gccIncludeFixedFolder))
                    {
                        includePaths.Add(gccIncludeFixedFolder);
                    }
                }

                var targetIncludeFolder = System.String.Format("/usr/{0}/include", gccTarget);
                if (System.IO.Directory.Exists(targetIncludeFolder))
                {
                    includePaths.Add(targetIncludeFolder);
                }
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

            Bam.Core.Log.DebugMessage("Gcc version for target '{0}' is '{1}'", baseTarget.ToString(), gccDetails.Version);
            Bam.Core.Log.DebugMessage("Gcc machine type for target '{0}' is '{1}'", baseTarget.ToString(), gccDetails.Target);
            Bam.Core.Log.DebugMessage("Gxx include path for target '{0}' is '{1}'", baseTarget.ToString(), gccDetails.GxxIncludePath);

            return gccDetails;
        }
    }
}
