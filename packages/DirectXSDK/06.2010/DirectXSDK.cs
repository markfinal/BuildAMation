// <copyright file="DirectXSDK.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>DirectXSDK package</summary>
// <author>Mark Final</author>
namespace DirectXSDK
{
    // TODO: need to add modules for Direct3D10, Direct3D11, and the other DX components
    class Direct3D9 :
        C.ThirdPartyModule
    {
        private static string installLocation;
        private static string includePath;
        private static string libraryBasePath;

        static
        Direct3D9()
        {
            if (!Bam.Core.OSUtilities.IsWindowsHosting)
            {
                throw new Bam.Core.Exception("DirectX package only valid on Windows");
            }

            const string registryPath = @"Microsoft\DirectX\Microsoft DirectX SDK (June 2010)";
            using (var dxInstallLocation = Bam.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(registryPath))
            {
                if (null == dxInstallLocation)
                {
                    throw new Bam.Core.Exception("DirectX SDK has not been installed on this machine");
                }

                installLocation = dxInstallLocation.GetValue("InstallPath") as string;
            }

            includePath = System.IO.Path.Combine(installLocation, "include");
            libraryBasePath = System.IO.Path.Combine(installLocation, "lib");
        }

        public
        Direct3D9()
        {
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(Direct3D9_IncludePaths);
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(Direct3D9_LinkerOptions);
        }

        [C.ExportLinkerOptionsDelegate]
        void
        Direct3D9_LinkerOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var linkerOptions = module.Options as C.ILinkerOptions;
            if (null == linkerOptions)
            {
                return;
            }

            // add library paths
            string platformLibraryPath = null;
            if (target.HasPlatform(Bam.Core.EPlatform.Win32))
            {
                platformLibraryPath = System.IO.Path.Combine(libraryBasePath, "x86");
            }
            else if (target.HasPlatform(Bam.Core.EPlatform.Win64))
            {
                platformLibraryPath = System.IO.Path.Combine(libraryBasePath, "x64");
            }
            else
            {
                throw new Bam.Core.Exception("Unsupported platform for the DirectX package");
            }
            linkerOptions.LibraryPaths.Add(platformLibraryPath);

            // add libraries
            var libraries = new Bam.Core.StringArray();
            libraries.Add("d3d9.lib");
            if (target.HasConfiguration(Bam.Core.EConfiguration.Debug))
            {
                libraries.Add("d3dx9d.lib");
                libraries.Add("dxerr.lib");
            }
            else
            {
                libraries.Add("d3dx9.lib");
            }
            linkerOptions.Libraries.AddRange(libraries);
        }

        [C.ExportCompilerOptionsDelegate]
        void
        Direct3D9_IncludePaths(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var compilerOptions = module.Options as C.ICCompilerOptions;
            if (compilerOptions == null)
            {
                return;
            }

            compilerOptions.IncludePaths.Add(includePath);
        }
    }
}
