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

            using (Microsoft.Win32.RegistryKey key = Opus.Core.Win32RegistryUtilities.OpenLMSoftwareKey(@"Microsoft\Microsoft SDKs\Windows\v6.0A"))
            {
                if (null == key)
                {
                    // NEW STYLE
#if true
                    // TODO: do I want to hard code VisualC here?
                    Opus.Core.IToolset toolset = Opus.Core.State.Get("Toolset", "visualc") as Opus.Core.IToolset;
                    if (null == toolset)
                    {
                        throw new Opus.Core.Exception("Toolset information for 'visualc' is missing", false);
                    }

                    string platformSDKPath = System.IO.Path.Combine(toolset.InstallPath((Opus.Core.BaseTarget)target), "PlatformSDK");
                    
                    if (System.IO.Directory.Exists(platformSDKPath))
                    {
                        installPath = platformSDKPath;
                    }
                    else
                    {
                        throw new Opus.Core.Exception("WindowsSDK 6.0A was not installed");
                    }
                    Opus.Core.Log.DebugMessage("Windows SDK installation folder is from the MSVC PlatformSDK: {0}", installPath);
                    
                    bin32Path = System.IO.Path.Combine(installPath, "bin");
                    bin64Path = System.IO.Path.Combine(bin32Path, "win64");
                    bin64Path = System.IO.Path.Combine(bin64Path, "AMD64");
                    
                    lib32Path = System.IO.Path.Combine(installPath, "lib");
                    lib64Path = System.IO.Path.Combine(lib32Path, "AMD64");
#else
                    C.Toolchain tc = C.ToolchainFactory.GetTargetInstance(target);
                    string platformSDKPath = System.IO.Path.Combine(tc.InstallPath(target), "PlatformSDK");

                    if (System.IO.Directory.Exists(platformSDKPath))
                    {
                        installPath = platformSDKPath;
                    }
                    else
                    {
                        throw new Opus.Core.Exception("WindowsSDK 6.0A was not installed");
                    }
                    Opus.Core.Log.DebugMessage("Windows SDK installation folder is from the MSVC PlatformSDK: {0}", installPath);

                    bin32Path = System.IO.Path.Combine(installPath, "bin");
                    bin64Path = System.IO.Path.Combine(bin32Path, "win64");
                    bin64Path = System.IO.Path.Combine(bin64Path, "AMD64");

                    lib32Path = System.IO.Path.Combine(installPath, "lib");
                    lib64Path = System.IO.Path.Combine(lib32Path, "AMD64");
#endif
                }
                else
                {
                    installPath = key.GetValue("InstallationFolder") as string;
                    Opus.Core.Log.DebugMessage("Windows SDK installation folder is {0}", installPath);

                    bin32Path = System.IO.Path.Combine(installPath, "bin");
                    bin64Path = bin32Path;

                    lib32Path = System.IO.Path.Combine(installPath, "lib");
                    lib64Path = System.IO.Path.Combine(lib32Path, "x64");
                }

                includePath = System.IO.Path.Combine(installPath, "include");
            }

            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(WindowsSDK_IncludePaths);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(WindowsSDK_LibraryPaths);
        }

        [C.ExportLinkerOptionsDelegate]
        void WindowsSDK_LibraryPaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ILinkerOptions linkerOptions = module.Options as C.ILinkerOptions;
            if (target.HasPlatform(Opus.Core.EPlatform.Win32))
            {
                linkerOptions.LibraryPaths.AddAbsoluteDirectory(lib32Path, true);
            }
            else if (target.HasPlatform(Opus.Core.EPlatform.Win64))
            {
                linkerOptions.LibraryPaths.AddAbsoluteDirectory(lib64Path, true);
            }
            else
            {
                throw new Opus.Core.Exception(System.String.Format("Windows SDK is not supported for the target '{0}'; only platforms win32 or win64", target.ToString()));
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void WindowsSDK_IncludePaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.IncludePaths.AddAbsoluteDirectory(includePath, true);
        }

        public override Opus.Core.StringArray Libraries(Opus.Core.Target target)
        {
            throw new System.NotImplementedException();
        }

        public static string BinPath(Opus.Core.Target target)
        {
            string binPath;
            if (Opus.Core.OSUtilities.Is64Bit(target))
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