// <copyright file="ToolsetInfo.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Mingw package</summary>
// <author>Mark Final</author>
namespace Mingw
{
    public class ToolsetInfo : Opus.Core.IToolsetInfo, C.ICompilerInfo, C.ILinkerInfo, C.IWinResourceCompilerInfo, C.IArchiverInfo
    {
        private static string installPath;
        private static string binPath;
        private static Opus.Core.StringArray environment;

        static void GetInstallPath()
        {
            if (null != installPath)
            {
                return;
            }

            if (Opus.Core.State.HasCategory("Mingw") && Opus.Core.State.Has("Mingw", "InstallPath"))
            {
                installPath = Opus.Core.State.Get("Mingw", "InstallPath") as string;
                Opus.Core.Log.DebugMessage("Mingw install path set from command line to '{0}'", installPath);
            }

            if (null == installPath)
            {
                using (Microsoft.Win32.RegistryKey key = Opus.Core.Win32RegistryUtilities.OpenLMSoftwareKey(@"Microsoft\Windows\CurrentVersion\Uninstall\MinGW"))
                {
                    if (null == key)
                    {
                        throw new Opus.Core.Exception("mingw was not installed");
                    }

                    installPath = key.GetValue("InstallLocation") as string;
                    Opus.Core.Log.DebugMessage("Mingw: Install path from registry '{0}'", installPath);
                }
            }

            binPath = System.IO.Path.Combine(installPath, "bin");

            environment = new Opus.Core.StringArray();
            environment.Add(binPath);
        }

        #region IToolsetInfo Members

        string Opus.Core.IToolsetInfo.BinPath(Opus.Core.Target target)
        {
            GetInstallPath();
            return binPath;
        }

        Opus.Core.StringArray Opus.Core.IToolsetInfo.Environment
        {
            get
            {
                GetInstallPath();
                return environment;
            }
        }

        string Opus.Core.IToolsetInfo.InstallPath(Opus.Core.Target target)
        {
            GetInstallPath();
            return installPath;
        }

        string Opus.Core.IToolsetInfo.Version(Opus.Core.Target target)
        {
            return "3.4.5";
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

        #region ILinkerInfo Members

        string C.ILinkerInfo.ExecutableSuffix
        {
            get
            {
                return string.Empty;
            }
        }

        string C.ILinkerInfo.MapFileSuffix
        {
            get
            {
                return ".map";
            }
        }

        string C.ILinkerInfo.ImportLibraryPrefix
        {
            get
            {
                return "lib";
            }
        }

        string C.ILinkerInfo.ImportLibrarySuffix
        {
            get
            {
                return ".a";
            }
        }

        string C.ILinkerInfo.DynamicLibraryPrefix
        {
            get
            {
                return string.Empty;
            }
        }

        string C.ILinkerInfo.DynamicLibrarySuffix
        {
            get
            {
                return ".dll";
            }
        }

        string C.ILinkerInfo.ImportLibrarySubDirectory
        {
            get
            {
                return "lib";
            }
        }

        string C.ILinkerInfo.BinaryOutputSubDirectory
        {
            get
            {
                return "bin";
            }
        }

        Opus.Core.StringArray C.ILinkerInfo.LibPaths(Opus.Core.Target target)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region IWinResourceCompilerInfo Members

        string C.IWinResourceCompilerInfo.CompiledResourceSuffix
        {
            get
            {
                return ".obj";
            }
        }

        #endregion

        #region IArchiverInfo Members

        string C.IArchiverInfo.StaticLibraryPrefix
        {
            get
            {
                return "lib";
            }
        }

        string C.IArchiverInfo.StaticLibrarySuffix
        {
            get
            {
                return ".a";
            }
        }

        string C.IArchiverInfo.StaticLibraryOutputSubDirectory
        {
            get
            {
                return "lib";
            }
        }

        #endregion
    }
}
