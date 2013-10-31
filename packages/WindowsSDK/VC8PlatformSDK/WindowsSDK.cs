// <copyright file="WindowsSDK.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>WindowsSDK package</summary>
// <author>Mark Final</author>
namespace WindowsSDK
{
    public sealed class WindowsSDK : C.ThirdPartyModule
    {
        private static string installPath;
        private static string bin32Path;
        private static string bin64Path;
        private static string lib32Path;
        private static string lib64Path;
        private static string includePath;

        public WindowsSDK(Opus.Core.Target target)
        {
            if (!Opus.Core.OSUtilities.IsWindowsHosting)
            {
                return;
            }

            C.Toolchain tc = C.ToolchainFactory.GetTargetInstance(target);
            string platformSDKPath = System.IO.Path.Combine(tc.InstallPath(target), "PlatformSDK");

            if (System.IO.Directory.Exists(platformSDKPath))
            {
                installPath = platformSDKPath;
            }
            else
            {
                throw new Opus.Core.Exception("PlatformSDK with VisualStudio 8.0 was not installed");
            }
            Opus.Core.Log.DebugMessage("Windows SDK installation folder is from the MSVC PlatformSDK: {0}", installPath);

            bin32Path = System.IO.Path.Combine(installPath, "bin");
            bin64Path = System.IO.Path.Combine(bin32Path, "win64");
            bin64Path = System.IO.Path.Combine(bin64Path, "AMD64");

            lib32Path = System.IO.Path.Combine(installPath, "lib");
            lib64Path = System.IO.Path.Combine(lib32Path, "AMD64");

            includePath = System.IO.Path.Combine(installPath, "include");
            
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(WindowsSDK_IncludePaths);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(WindowsSDK_LibraryPaths);
        }

        [C.ExportLinkerOptionsDelegate]
        void WindowsSDK_LibraryPaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ILinkerOptions linkerOptions = module.Options as C.ILinkerOptions;
            if (target.Platform == Opus.Core.EPlatform.Win32)
            {
                linkerOptions.LibraryPaths.Add(lib32Path);
            }
            else if (target.Platform == Opus.Core.EPlatform.Win64)
            {
                linkerOptions.LibraryPaths.Add(lib64Path);
            }
            else
            {
                throw new Opus.Core.Exception("Windows SDK is not supported for platform '{0}'; use win32 or win64", target.Platform);
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void WindowsSDK_IncludePaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.IncludePaths.Add(includePath);
        }

        public override Opus.Core.StringArray Libraries(Opus.Core.Target target)
        {
            throw new System.NotImplementedException();
        }

        public static string BinPath(Opus.Core.BaseTarget baseTarget)
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