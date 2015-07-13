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
namespace WindowsSDK
{
    public sealed class WindowsSDKV2 :
            C.V2.CSDKModule
    {
        public WindowsSDKV2()
        {
            this.Macros.Add("InstallPath", @"C:\Program Files\Microsoft SDKs\Windows\v6.0A");
            this.PublicPatch((settings, appliedTo) =>
            {
                var compilation = settings as C.V2.ICommonCompilerOptions;
                if (null != compilation)
                {
                    compilation.IncludePaths.Add(Bam.Core.V2.TokenizedString.Create(@"$(InstallPath)\Include", this));
                }

                var linking = settings as C.V2.ICommonLinkerOptions;
                if (null != linking)
                {
                    if ((appliedTo as C.V2.CModule).BitDepth == C.V2.EBit.ThirtyTwo)
                    {
                        linking.LibraryPaths.Add(Bam.Core.V2.TokenizedString.Create(@"$(InstallPath)\Lib", this));
                    }
                    else
                    {
                        linking.LibraryPaths.Add(Bam.Core.V2.TokenizedString.Create(@"$(InstallPath)\Lib\x64", this));
                    }
                }
            });
        }

        public override void Evaluate()
        {
            // do nothing
            this.IsUpToDate = true;
        }

        protected override void
        ExecuteInternal(
            Bam.Core.V2.ExecutionContext context)
        {
            // do nothing
        }

        protected override void GetExecutionPolicy(string mode)
        {
            // do nothing
        }
    }

    public sealed class WindowsSDK :
        C.ThirdPartyModule
    {
        private static string installPath;
        private static string bin32Path;
        private static string bin64Path;
        private static string lib32Path;
        private static string lib64Path;
        private static string includePath;
        private static string sharedIncludePath;

        static
        WindowsSDK()
        {
            if (!Bam.Core.OSUtilities.IsWindowsHosting)
            {
                return;
            }

            using (var key = Bam.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(@"Microsoft\Windows Kits\Installed Roots"))
            {
                if (null == key)
                {
                    throw new Bam.Core.Exception("Windows SDKs were not installed");
                }

                installPath = key.GetValue("KitsRoot81") as string;
                Bam.Core.Log.DebugMessage("Windows 8.1 SDK installation folder is {0}", installPath);

                var binPath = System.IO.Path.Combine(installPath, "bin");

                bin32Path = System.IO.Path.Combine(binPath, "x86");
                bin64Path = System.IO.Path.Combine(binPath, "x64");

                var libPath = System.IO.Path.Combine(installPath, "Lib");
                var winv64Path = System.IO.Path.Combine(libPath, "winv6.3");
                var umPath = System.IO.Path.Combine(winv64Path, "um");
                lib32Path = System.IO.Path.Combine(umPath, "x86");
                lib64Path = System.IO.Path.Combine(umPath, "x64");

                var include = System.IO.Path.Combine(installPath, "include");
                includePath = System.IO.Path.Combine(include, "um");
                sharedIncludePath = System.IO.Path.Combine(include, "shared");
            }
        }

        public
        WindowsSDK()
        {
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
                throw new Bam.Core.Exception("Windows 8.1 SDK is not supported on '{0}'; use win32 or win64", target.ToString());
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
            compilerOptions.IncludePaths.Add(sharedIncludePath);
        }

        public static string
        InstallPath()
        {
            return installPath;
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

    public sealed class Direct3D9 :
        C.ThirdPartyModule
    {
        public
        Direct3D9()
        {
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(Direct3D9_Library);
        }

        [C.ExportLinkerOptionsDelegate]
        void
        Direct3D9_Library(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var linkerOptions = module.Options as C.ILinkerOptions;
            if (null == linkerOptions)
            {
                return;
            }

            linkerOptions.Libraries.Add("d3d9.lib");
        }
    }

    public sealed class DXGuid :
        C.ThirdPartyModule
    {
        public
        DXGuid()
        {
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(DXGuid_Library);
        }

        [C.ExportLinkerOptionsDelegate]
        void
        DXGuid_Library(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var linkerOptions = module.Options as C.ILinkerOptions;
            if (null == linkerOptions)
            {
                return;
            }

            linkerOptions.Libraries.Add("dxguid.lib");
        }
    }

    public sealed class Direct3D11 :
        C.ThirdPartyModule
    {
        public
        Direct3D11()
        {
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(Direct3D11_Library);
        }

        [C.ExportLinkerOptionsDelegate]
        void
        Direct3D11_Library(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var linkerOptions = module.Options as C.ILinkerOptions;
            if (null == linkerOptions)
            {
                return;
            }

            linkerOptions.Libraries.Add("d3d11.lib");
        }
    }

    public sealed class Direct3DShaderCompiler :
        C.ThirdPartyModule
    {
        public
        Direct3DShaderCompiler(
            Bam.Core.Target target)
        {
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(Direct3DCompiler_Library);

            var installLocation = Bam.Core.DirectoryLocation.Get(WindowsSDK.InstallPath());
            var redistFolder = installLocation.SubDirectory("Redist");
            var d3dFolder = redistFolder.SubDirectory("D3D");
            var archFolder = Bam.Core.OSUtilities.Is64Bit(target) ? d3dFolder.SubDirectory("x64") : d3dFolder.SubDirectory("x86");
            this.Locations[C.DynamicLibrary.OutputFile] = Bam.Core.FileLocation.Get(archFolder, "d3dcompiler_47.dll", Bam.Core.Location.EExists.Exists);
        }

        [C.ExportLinkerOptionsDelegate]
        void
        Direct3DCompiler_Library(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var linkerOptions = module.Options as C.ILinkerOptions;
            if (null == linkerOptions)
            {
                return;
            }

            linkerOptions.Libraries.Add("d3dcompiler.lib");
        }

#if D_PACKAGE_PUBLISHER_DEV
        [Publisher.CopyFileLocations(Platform=Bam.Core.EPlatform.Windows)]
        private Bam.Core.Array<Publisher.PublishDependency> publishKeys = new Bam.Core.Array<Publisher.PublishDependency>(
            new Publisher.PublishDependency(C.DynamicLibrary.OutputFile));
#endif
    }
}
