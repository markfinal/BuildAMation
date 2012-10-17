// <copyright file="Mingw.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Mingw package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.RegisterTargetToolChain("C", "mingw", "Mingw.Toolchain.GetVersion")]
[assembly: Opus.Core.MapToolChainClassTypes("C", "mingw", C.ClassNames.ArchiverTool, typeof(MingwCommon.Archiver), typeof(Mingw.ArchiverOptionCollection))]
[assembly: Opus.Core.MapToolChainClassTypes("C", "mingw", C.ClassNames.CCompilerTool, typeof(Mingw.CCompiler), typeof(Mingw.CCompilerOptionCollection))]
[assembly: Opus.Core.MapToolChainClassTypes("C", "mingw", C.ClassNames.CPlusPlusCompilerTool, typeof(Mingw.CPlusPlusCompiler), typeof(Mingw.CPlusPlusCompilerOptionCollection))]
[assembly: Opus.Core.MapToolChainClassTypes("C", "mingw", C.ClassNames.LinkerTool, typeof(Mingw.Linker), typeof(Mingw.LinkerOptionCollection))]
#if false
[assembly: Opus.Core.MapToolChainClassTypes("C", "mingw", C.ClassNames.Toolchain, typeof(Mingw.Toolchain), typeof(Mingw.ToolchainOptionCollection))]
#endif
[assembly: Opus.Core.MapToolChainClassTypes("C", "mingw", C.ClassNames.Win32ResourceCompilerTool, typeof(MingwCommon.Win32ResourceCompiler), typeof(C.Win32ResourceCompilerOptionCollection))]

[assembly: C.RegisterToolchain(
    "mingw",
    typeof(Mingw.ToolsetInfo),
    typeof(Mingw.CCompiler), typeof(Mingw.CCompilerOptionCollection),
    typeof(Mingw.CPlusPlusCompiler), typeof(Mingw.CPlusPlusCompilerOptionCollection),
    typeof(Mingw.Linker), typeof(Mingw.LinkerOptionCollection),
    typeof(MingwCommon.Archiver), typeof(Mingw.ArchiverOptionCollection),
    typeof(MingwCommon.Win32ResourceCompiler), typeof(C.Win32ResourceCompilerOptionCollection))]

namespace Mingw
{
    public class ToolsetInfo : Opus.Core.IToolsetInfo, C.ICompilerInfo
    {
        private static string installPath;
        private static string binPath;
        private static Opus.Core.StringArray environment;

        static ToolsetInfo()
        {
            GetInstallPath();
        }

        static void GetInstallPath()
        {
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
            return binPath;
        }

        Opus.Core.StringArray Opus.Core.IToolsetInfo.Environment
        {
            get
            {
                return environment;
            }
        }

        string Opus.Core.IToolsetInfo.InstallPath(Opus.Core.Target target)
        {
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
    }
}
