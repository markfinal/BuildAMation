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

        public
        WindowsSDK(
            Bam.Core.Target target)
        {
            if (!Bam.Core.OSUtilities.IsWindowsHosting)
            {
                return;
            }

            using (var key = Bam.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(@"Microsoft\Microsoft SDKs\Windows\v6.0A"))
            {
                if (null == key)
                {
                    // TODO: do I want to hard code VisualC here?
                    var toolset = Bam.Core.State.Get("Toolset", "visualc") as Bam.Core.IToolset;
                    if (null == toolset)
                    {
                        throw new Bam.Core.Exception("Toolset information for 'visualc' is missing");
                    }

                    var platformSDKPath = System.IO.Path.Combine(toolset.InstallPath((Bam.Core.BaseTarget)target), "PlatformSDK");

                    if (System.IO.Directory.Exists(platformSDKPath))
                    {
                        installPath = platformSDKPath;
                    }
                    else
                    {
                        throw new Bam.Core.Exception("WindowsSDK 6.0A was not installed");
                    }
                    Bam.Core.Log.DebugMessage("Windows SDK installation folder is from the MSVC PlatformSDK: {0}", installPath);

                    bin32Path = System.IO.Path.Combine(installPath, "bin");
                    bin64Path = System.IO.Path.Combine(bin32Path, "win64");
                    bin64Path = System.IO.Path.Combine(bin64Path, "AMD64");

                    lib32Path = System.IO.Path.Combine(installPath, "lib");
                    lib64Path = System.IO.Path.Combine(lib32Path, "AMD64");
                }
                else
                {
                    installPath = key.GetValue("InstallationFolder") as string;
                    Bam.Core.Log.DebugMessage("Windows SDK installation folder is {0}", installPath);

                    bin32Path = System.IO.Path.Combine(installPath, "bin");
                    bin64Path = bin32Path;

                    lib32Path = System.IO.Path.Combine(installPath, "lib");
                    lib64Path = System.IO.Path.Combine(lib32Path, "x64");
                }

                includePath = System.IO.Path.Combine(installPath, "include");
            }

            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(WindowsSDK_IncludePaths);
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(WindowsSDK_LibraryPaths);
        }

        [C.ExportLinkerOptionsDelegate]
        void
        WindowsSDK_LibraryPaths(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var linkerOptions = module.Options as C.ILinkerOptions;
            if (null == linkerOptions)
            {
                return;
            }

            if (target.HasPlatform(Bam.Core.EPlatform.Win32))
            {
                linkerOptions.LibraryPaths.Add(lib32Path);
            }
            else if (target.HasPlatform(Bam.Core.EPlatform.Win64))
            {
                linkerOptions.LibraryPaths.Add(lib64Path);
            }
            else
            {
                throw new Bam.Core.Exception("Windows SDK is not supported for the target '{0}'; only platforms win32 or win64", target.ToString());
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void
        WindowsSDK_IncludePaths(
            Bam.Core.IModule module,
            Bam.Core.Target target)
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
            Bam.Core.BaseTarget baseTarget)
        {
            string binPath;
            if (Bam.Core.OSUtilities.Is64Bit(baseTarget))
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
