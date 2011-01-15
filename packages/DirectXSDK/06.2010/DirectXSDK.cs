// <copyright file="DirectXSDK.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>DirectXSDK package</summary>
// <author>Mark Final</author>
namespace DirectXSDK
{
    // TODO: potentially this should be called Direct3D9, and then Direct3D10, and Direct3D11 and so on
    class Direct3D9 : C.ThirdPartyModule
    {
        private static string installLocation;
        private static string includePath;
        private static string libraryBasePath;

        static Direct3D9()
        {
            const string registryPath = @"Microsoft\DirectX\Microsoft DirectX SDK (June 2010)";
            using (Microsoft.Win32.RegistryKey dxInstallLocation = Opus.Core.Win32RegistryUtilities.OpenLMSoftwareKey(registryPath))
            {
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
            if (target.Platform == Opus.Core.EPlatform.Win32)
            {
                platformLibraryPath = System.IO.Path.Combine(libraryBasePath, "x86");
            }
            else if (target.Platform == Opus.Core.EPlatform.Win64)
            {
                platformLibraryPath = System.IO.Path.Combine(libraryBasePath, "x64");
            }
            else
            {
                throw new Opus.Core.Exception("Unsupported platform for the DirectX package");
            }
            linkerOptions.LibraryPaths.Add(Opus.Core.State.PackageInfo["DirectXSDK"], platformLibraryPath);
        }

        [C.ExportCompilerOptionsDelegate]
        void Direct3D9_IncludePaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.IncludePaths.Add(Opus.Core.State.PackageInfo["DirectXSDK"], includePath);
        }

        public override Opus.Core.StringArray Libraries(Opus.Core.Target target)
        {
            Opus.Core.StringArray libraries = new Opus.Core.StringArray();

            libraries.Add(@"d3d9.lib");
            if (Opus.Core.EConfiguration.Debug == target.Configuration)
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
