// <copyright file="WindowsSDK.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>WindowsSDK package</summary>
// <author>Mark Final</author>
namespace WindowsSDK
{
    public sealed class WindowsSDK :
        C.ThirdPartyModule
    {
        private static string installPath;
        private static string bin32Path;
        private static string bin64Path;
        private static string lib32Path;
        private static string lib64Path;
        private static string includePath;

        static
        WindowsSDK()
        {
            if (!Opus.Core.OSUtilities.IsWindowsHosting)
            {
                return;
            }

            using (var key = Opus.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(@"Microsoft\Microsoft SDKs\Windows\v7.1"))
            {
                if (null == key)
                {
                    throw new Opus.Core.Exception("WindowsSDK 7.1 was not installed");
                }

                installPath = key.GetValue("InstallationFolder") as string;
                Opus.Core.Log.DebugMessage("Windows SDK installation folder is {0}", installPath);

                bin32Path = System.IO.Path.Combine(installPath, "bin");
                bin64Path = System.IO.Path.Combine(bin32Path, "x64");

                lib32Path = System.IO.Path.Combine(installPath, "lib");
                lib64Path = System.IO.Path.Combine(lib32Path, "x64");

                includePath = System.IO.Path.Combine(installPath, "include");
            }
        }

        public
        WindowsSDK()
        {
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(WindowsSDK_IncludePaths);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(WindowsSDK_LibraryPaths);
        }

        [C.ExportLinkerOptionsDelegate]
        void
        WindowsSDK_LibraryPaths(
            Opus.Core.IModule module,
            Opus.Core.Target target)
        {
            var linkerOptions = module.Options as C.ILinkerOptions;
            if (null == linkerOptions)
            {
                return;
            }

            if (target.HasPlatform(Opus.Core.EPlatform.Win32))
            {
                linkerOptions.LibraryPaths.Add(lib32Path);
            }
            else if (target.HasPlatform(Opus.Core.EPlatform.Win64))
            {
                linkerOptions.LibraryPaths.Add(lib64Path);
            }
            else
            {
                throw new Opus.Core.Exception("Windows SDK is not supported on '{0}'; use win32 or win64", target.ToString());
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void
        WindowsSDK_IncludePaths(
            Opus.Core.IModule module,
            Opus.Core.Target target)
        {
            var compilerOptions = module.Options as C.ICCompilerOptions;
            if (null == compilerOptions)
            {
                return;
            }

            compilerOptions.IncludePaths.Add(includePath);
        }

        public static string
        BinPath(
            Opus.Core.BaseTarget baseTarget)
        {
            string binPath;
            if (Opus.Core.OSUtilities.Is64Bit(baseTarget))
            {
                binPath = bin64Path;
            }
            else
            {
                binPath = bin32Path;
            }

            return binPath;
        }
    }
}
