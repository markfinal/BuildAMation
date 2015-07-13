#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
namespace DirectXSDK
{
    public static class Direct3D9Location
    {
        static Direct3D9Location()
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

                InstallPath = dxInstallLocation.GetValue("InstallPath") as string;
            }
        }

        public static string InstallPath
        {
            get;
            set;
        }
    }

    sealed class Direct3D9V2 :
        C.V2.CSDKModule
    {
        public Direct3D9V2()
        {
            var installPath = Direct3D9Location.InstallPath;

            this.Macros.Add("InstallPath", installPath);
            this.Macros.Add("IncludePath", Bam.Core.V2.TokenizedString.Create("$(InstallPath)/include", this));
            this.Macros.Add("LibraryPath", Bam.Core.V2.TokenizedString.Create("$(InstallPath)/lib", this));
        }

        protected override void Init(Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.PublicPatch((settings, appliedTo) =>
                {
                    var compiler = settings as C.V2.ICommonCompilerOptions;
                    if (null != compiler)
                    {
                        compiler.IncludePaths.Add(this.Macros["IncludePath"]);
                    }

                    var linker = settings as C.V2.ICommonLinkerOptions;
                    if (null != linker)
                    {
                        if ((appliedTo as C.V2.CModule).BitDepth == C.V2.EBit.ThirtyTwo)
                        {
                            linker.LibraryPaths.Add(Bam.Core.V2.TokenizedString.Create("$(LibraryPath)/x86", this));
                        }
                        else
                        {
                            linker.LibraryPaths.Add(Bam.Core.V2.TokenizedString.Create("$(LibraryPath)/x64", this));
                        }
                    }
                });
        }

        public override void Evaluate()
        {
            this.IsUpToDate = true;
        }

        protected override void ExecuteInternal(Bam.Core.V2.ExecutionContext context)
        {
            // do nothing
        }

        protected override void GetExecutionPolicy(string mode)
        {
            // do nothing
        }
    }

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
