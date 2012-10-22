// <copyright file="Gcc.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Gcc package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.RegisterTargetToolChain("C", "gcc", "Gcc.Toolchain.VersionString")]

[assembly: Opus.Core.MapToolChainClassTypes("C", "gcc", C.ClassNames.ArchiverTool, typeof(Gcc.Archiver), typeof(Gcc.ArchiverOptionCollection))]
[assembly: Opus.Core.MapToolChainClassTypes("C", "gcc", C.ClassNames.CCompilerTool, typeof(Gcc.CCompiler), typeof(Gcc.CCompilerOptionCollection))]
[assembly: Opus.Core.MapToolChainClassTypes("C", "gcc", C.ClassNames.CPlusPlusCompilerTool, typeof(Gcc.CPlusPlusCompiler), typeof(Gcc.CPlusPlusCompilerOptionCollection))]
[assembly: Opus.Core.MapToolChainClassTypes("C", "gcc", C.ClassNames.LinkerTool, typeof(Gcc.Linker), typeof(Gcc.LinkerOptionCollection))]
#if false
[assembly: Opus.Core.MapToolChainClassTypes("C", "gcc", C.ClassNames.Toolchain, typeof(Gcc.Toolchain), typeof(Gcc.ToolchainOptionCollection))]
#endif

[assembly: C.RegisterToolchain(
    "gcc",
    typeof(Gcc.ToolsetInfo),
    typeof(Gcc.CCompiler), typeof(Gcc.CCompilerOptionCollection),
    typeof(GccCommon.CxxCompiler), typeof(Gcc.CPlusPlusCompilerOptionCollection),
    typeof(Gcc.Linker), typeof(Gcc.LinkerOptionCollection),
    typeof(Gcc.Archiver), typeof(Gcc.ArchiverOptionCollection),
    null,null)]

namespace Gcc
{
    public class ToolsetInfo : Opus.Core.IToolsetInfo, C.ICompilerInfo, GccCommon.IGCCInfo, C.ILinkerInfo, C.IArchiverInfo
    {
        private static string installPath;
        
        private class GccDetails
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

        private static System.Collections.Generic.Dictionary<Opus.Core.Target, GccDetails> gccDetailsForTarget = new System.Collections.Generic.Dictionary<Opus.Core.Target, GccDetails>();

        private void GetGccDetails(Opus.Core.Target target)
        {
            string pathPrefix = null;
            string gxxIncludeDir = null;
            string gccTarget = null;
            string gccVersion = null;

            // get version
            {
                System.Diagnostics.ProcessStartInfo processStartInfo = new System.Diagnostics.ProcessStartInfo();
                processStartInfo.FileName = this.Executable(target);
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
                processStartInfo.FileName = this.Executable(target);
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
        }
        
        static void GetInstallPath()
        {
            if (null != installPath)
            {
                return;
            }
            
            if (Opus.Core.State.HasCategory("Gcc") && Opus.Core.State.Has("Gcc", "InstallPath"))
            {
                installPath = Opus.Core.State.Get("Gcc", "InstallPath") as string;
                Opus.Core.Log.DebugMessage("Gcc install path set from command line to '{0}'", installPath);
            }

            if (null == installPath)
            {
                installPath = "/usr/bin";
            }
        }
        
        private string Executable(Opus.Core.Target target)
        {
            return System.IO.Path.Combine((this as Opus.Core.IToolsetInfo).BinPath(target), "gcc-4.4");
        }
        
        #region IToolsetInfo Members

        string Opus.Core.IToolsetInfo.BinPath(Opus.Core.Target target)
        {
            GetInstallPath();
            return installPath;
        }

        Opus.Core.StringArray Opus.Core.IToolsetInfo.Environment
        {
            get { throw new System.NotImplementedException(); }
        }

        string Opus.Core.IToolsetInfo.InstallPath(Opus.Core.Target target)
        {
            GetInstallPath();
            return installPath;
        }

        string Opus.Core.IToolsetInfo.Version(Opus.Core.Target target)
        {
            return "4.4";
        }

        #endregion

        #region ICompilerInfo Members

        string C.ICompilerInfo.PreprocessedOutputSuffix
        {
            get
            {
                return ".i";
            }
        }

        string C.ICompilerInfo.ObjectFileSuffix
        {
            get
            {
                return ".o";
            }
        }

        string C.ICompilerInfo.ObjectFileOutputSubDirectory
        {
            get
            {
                return "obj";
            }
        }

        Opus.Core.StringArray C.ICompilerInfo.IncludePaths(Opus.Core.Target target)
        {
            throw new System.NotImplementedException();
        }

        #endregion
        
        #region IGCCInfo Members

        string GccCommon.IGCCInfo.GccVersion(Opus.Core.Target target)
        {
            if (!gccDetailsForTarget.ContainsKey(target))
            {
                GetGccDetails(target);
            }

            return gccDetailsForTarget[target].Version;
        }

        string GccCommon.IGCCInfo.MachineType(Opus.Core.Target target)
        {
            if (!gccDetailsForTarget.ContainsKey(target))
            {
                GetGccDetails(target);
            }

            return gccDetailsForTarget[target].Target;
        }

        string GccCommon.IGCCInfo.GxxIncludePath(Opus.Core.Target target)
        {
            if (!gccDetailsForTarget.ContainsKey(target))
            {
                GetGccDetails(target);
            }

            return gccDetailsForTarget[target].GxxIncludePath;
        }

        #endregion

        #region ILinkerInfo Members

        string C.ILinkerInfo.ExecutableSuffix
        {
            get { throw new System.NotImplementedException(); }
        }

        string C.ILinkerInfo.MapFileSuffix
        {
            get { throw new System.NotImplementedException(); }
        }

        string C.ILinkerInfo.ImportLibraryPrefix
        {
            get { throw new System.NotImplementedException(); }
        }

        string C.ILinkerInfo.ImportLibrarySuffix
        {
            get { throw new System.NotImplementedException(); }
        }

        string C.ILinkerInfo.DynamicLibraryPrefix
        {
            get { throw new System.NotImplementedException(); }
        }

        string C.ILinkerInfo.DynamicLibrarySuffix
        {
            get { throw new System.NotImplementedException(); }
        }

        string C.ILinkerInfo.ImportLibrarySubDirectory
        {
            get { throw new System.NotImplementedException(); }
        }

        string C.ILinkerInfo.BinaryOutputSubDirectory
        {
            get { throw new System.NotImplementedException(); }
        }

        Opus.Core.StringArray C.ILinkerInfo.LibPaths(Opus.Core.Target target)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region IArchiverInfo Members

        string C.IArchiverInfo.StaticLibraryPrefix
        {
            get { throw new System.NotImplementedException(); }
        }

        string C.IArchiverInfo.StaticLibrarySuffix
        {
            get { throw new System.NotImplementedException(); }
        }

        string C.IArchiverInfo.StaticLibraryOutputSubDirectory
        {
            get { throw new System.NotImplementedException(); }
        }

        #endregion
    }
}
