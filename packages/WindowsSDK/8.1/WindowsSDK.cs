#region License
// Copyright 2010-2014 Mark Final
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
#endregion
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
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(Direct3D9_Libraries);
        }

        [C.ExportLinkerOptionsDelegate]
        void
        Direct3D9_Libraries(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var linkerOptions = module.Options as C.ILinkerOptions;
            if (null == linkerOptions)
            {
                return;
            }

            var libraries = new Bam.Core.StringArray();
            libraries.Add("d3d9.lib");
            libraries.Add("d3dcompiler.lib");
            linkerOptions.Libraries.AddRange(libraries);
        }
    }
}
