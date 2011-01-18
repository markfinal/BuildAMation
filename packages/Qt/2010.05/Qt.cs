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
                return "2010.05";
            }
        }

        static Qt()
        {
            if (Opus.Core.OSUtilities.IsWindowsHosting)
            {
                using (Microsoft.Win32.RegistryKey key = Opus.Core.Win32RegistryUtilities.OpenLMSoftwareKey(@"Microsoft\Windows\CurrentVersion\Uninstall\Qt SDK 2010.05 - C:_Qt_2010.05"))
                {
                    if (null == key)
                    {
                        throw new Opus.Core.Exception("Qt 2010.05 was not installed");
                    }

                    installPath = key.GetValue("QTSDK_INSTDIR") as string;
                    Opus.Core.Log.DebugMessage("Qt installation folder is {0}", installPath);

                    string qtPath = System.IO.Path.Combine(installPath, "qt");

                    BinPath = System.IO.Path.Combine(qtPath, "bin");
                    libPath = System.IO.Path.Combine(qtPath, "lib");
                    includePath = System.IO.Path.Combine(qtPath, "include");
                }
            }
            else
            {
                throw new Opus.Core.Exception("Qt identification has not been implemented on non-Windows platforms yet");
            }
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
