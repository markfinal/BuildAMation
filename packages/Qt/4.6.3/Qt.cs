// <copyright file="Qt.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Qt package</summary>
// <author>Mark Final</author>
namespace Qt
{
    public sealed class Qt : C.ThirdPartyModule
    {
        private static string installPath;
        private static string libPath;
        private static string includePath;

        public static string BinPath
        {
            get;
            private set;
        }

        public static string VersionString
        {
            get
            {
                return "4.6.3";
            }
        }

        static Qt()
        {
            if (Opus.Core.State.HasCategory("Qt") && Opus.Core.State.Has("Qt", "InstallPath"))
            {
                installPath = Opus.Core.State.Get("Qt", "InstallPath") as string;
                Opus.Core.Log.DebugMessage("Qt install path set from command line to '{0}'", installPath);
            }

            if (null == installPath)
            {
                if (Opus.Core.OSUtilities.IsWindowsHosting)
                {
                    using (Microsoft.Win32.RegistryKey key = Opus.Core.Win32RegistryUtilities.OpenLMSoftwareKey(@"Trolltech\Versions\4.6.3"))
                    {
                        if (null == key)
                        {
                            throw new Opus.Core.Exception("Qt libraries for 4.6.3 were not installed");
                        }

                        installPath = key.GetValue("InstallDir") as string;
                        if (null == installPath)
                        {
                            throw new Opus.Core.Exception("Unable to locate InstallDir registry key for Qt 4.6.3");
                        }
                        Opus.Core.Log.DebugMessage("Qt installation folder is {0}", installPath);
                    }
                }
                else if (Opus.Core.OSUtilities.IsUnixHosting)
                {
                    installPath = @"/usr/local/Trolltech/Qt-4.6.3"; // default installation directory
                }
                else
                {
                    throw new Opus.Core.Exception("Qt identification has not been implemented on non-Windows and Linux platforms yet");
                }
            }

            BinPath = System.IO.Path.Combine(installPath, "bin");
            libPath = System.IO.Path.Combine(installPath, "lib");
            includePath = System.IO.Path.Combine(installPath, "include");
        }

        public Qt()
        {
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(Qt_IncludePaths);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(Qt_LibraryPaths);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(Qt_VisualCWarningLevel);
        }

        [C.ExportLinkerOptionsDelegate]
        void Qt_LibraryPaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ILinkerOptions linkerOptions = module.Options as C.ILinkerOptions;
            linkerOptions.LibraryPaths.Add(libPath, true);
        }

        [C.ExportCompilerOptionsDelegate]
        void Qt_IncludePaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.IncludePaths.Add(includePath, true);
        }

        [C.ExportCompilerOptionsDelegate]
        void Qt_VisualCWarningLevel(Opus.Core.IModule module, Opus.Core.Target target)
        {
            VisualCCommon.ICCompilerOptions compilerOptions = module.Options as VisualCCommon.ICCompilerOptions;
            if (null != compilerOptions)
            {
                compilerOptions.WarningLevel = VisualCCommon.EWarningLevel.Level3;
            }
        }

        public override Opus.Core.StringArray Libraries(Opus.Core.Target target)
        {
            throw new System.NotImplementedException();
        }
    }
}
