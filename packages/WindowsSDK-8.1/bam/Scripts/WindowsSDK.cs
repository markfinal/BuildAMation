#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
namespace WindowsSDK
{
    public sealed class WindowsSDKV2 :
        C.V2.CSDKModule
    {
        public WindowsSDKV2()
        {
            string installPath;
            using (var key = Bam.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(@"Microsoft\Windows Kits\Installed Roots"))
            {
                if (null == key)
                {
                    throw new Bam.Core.Exception("Windows SDKs were not installed");
                }

                installPath = key.GetValue("KitsRoot81") as string;
                Bam.Core.Log.DebugMessage("Windows 8.1 SDK installation folder is {0}", installPath);
            }

            this.Macros.Add("InstallPath", installPath);
            this.PublicPatch((settings, appliedTo) =>
            {
                var compilation = settings as C.V2.ICommonCompilerOptions;
                if (null != compilation)
                {
                    compilation.IncludePaths.AddUnique(Bam.Core.V2.TokenizedString.Create(@"$(InstallPath)Include\um", this));
                    compilation.IncludePaths.AddUnique(Bam.Core.V2.TokenizedString.Create(@"$(InstallPath)Include\shared", this));
                }

                var linking = settings as C.V2.ICommonLinkerOptions;
                if (null != linking)
                {
                    if ((appliedTo as C.V2.CModule).BitDepth == C.V2.EBit.ThirtyTwo)
                    {
                        linking.LibraryPaths.AddUnique(Bam.Core.V2.TokenizedString.Create(@"$(InstallPath)Lib\winv6.3\um\x86", this));
                    }
                    else
                    {
                        linking.LibraryPaths.AddUnique(Bam.Core.V2.TokenizedString.Create(@"$(InstallPath)Lib\winv6.3\um\x64", this));
                    }
                }
            });
        }

        public override void
        Evaluate()
        {
            this.ReasonToExecute = null;
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
