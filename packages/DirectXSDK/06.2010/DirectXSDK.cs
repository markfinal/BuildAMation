// <copyright file="DirectXSDK.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>DirectXSDK package</summary>
// <author>Mark Final</author>
namespace DirectXSDK
{
    // TODO: need to add modules for Direct3D10, Direct3D11, and the other DX components
    class Direct3D9 : C.ThirdPartyModule
    {
        private static string installLocation;
        private static string includePath;
        private static string libraryBasePath;

        static Direct3D9()
        {
            if (!Opus.Core.OSUtilities.IsWindowsHosting)
            {
                throw new Opus.Core.Exception("DirectX package only valid on Windows");
            }

            const string registryPath = @"Microsoft\DirectX\Microsoft DirectX SDK (June 2010)";
            using (Microsoft.Win32.RegistryKey dxInstallLocation = Opus.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(registryPath))
            {
                if (null == dxInstallLocation)
                {
                    throw new Opus.Core.Exception("DirectX SDK has not been installed on this machine");
                }

                installLocation = dxInstallLocation.GetValue("InstallPath") as string;
            }

            includePath = System.IO.Path.Combine(installLocation, "include");
            libraryBasePath = System.IO.Path.Combine(installLocation, "lib");
        }

        public Direct3D9()
        {
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(Direct3D9_IncludePaths);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(Direct3D9_LibraryPaths);
        }

        [C.ExportLinkerOptionsDelegate]
        void Direct3D9_LibraryPaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ILinkerOptions linkerOptions = module.Options as C.ILinkerOptions;
            string platformLibraryPath = null;
            if (target.HasPlatform(Opus.Core.EPlatform.Win32))
            {
                platformLibraryPath = System.IO.Path.Combine(libraryBasePath, "x86");
            }
            else if (target.HasPlatform(Opus.Core.EPlatform.Win64))
            {
                platformLibraryPath = System.IO.Path.Combine(libraryBasePath, "x64");
            }
            else
            {
                throw new Opus.Core.Exception("Unsupported platform for the DirectX package");
            }

            linkerOptions.LibraryPaths.AddAbsoluteDirectory(platformLibraryPath, true);
        }

        [C.ExportCompilerOptionsDelegate]
        void Direct3D9_IncludePaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.IncludePaths.AddAbsoluteDirectory(includePath, true);
        }

        public override Opus.Core.StringArray Libraries(Opus.Core.Target target)
        {
            Opus.Core.StringArray libraries = new Opus.Core.StringArray();

            libraries.Add(@"d3d9.lib");
            if (target.HasConfiguration(Opus.Core.EConfiguration.Debug))
            {
                libraries.Add(@"d3dx9d.lib");
            }
            else
            {
                libraries.Add(@"d3dx9.lib");
            }

            return libraries;
        }
    }
}
