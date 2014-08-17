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
namespace AMDAPPSDK
{
    // Add modules here
    class AMDAPPSDK : C.ThirdPartyModule
    {
        static AMDAPPSDK()
        {
            InstallPath = null;

            if (Bam.Core.State.HasCategory("AMDAPPSDK") && Bam.Core.State.Has("AMDAPPSDK", "InstallPath"))
            {
                InstallPath = Bam.Core.State.Get("AMDAPPSDK", "InstallPath") as string;
                Bam.Core.Log.DebugMessage("AMDAPPSDK install path set from command line to '{0}'", InstallPath);
            }

            if (!Bam.Core.OSUtilities.IsWindowsHosting)
            {
                throw new Bam.Core.Exception("AMDAPPSDK is only supported on Windows currently");
            }

            if (null == InstallPath)
            {
                using (var key = Bam.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(@"AMD\APPSDK\2.9"))
                {
                    if (null == key)
                    {
                        throw new Bam.Core.Exception("AMDAPPSDK was not installed");
                    }

                    var installPath = key.GetValue("InstallDir") as string;
                    if (null == installPath)
                    {
                        throw new Bam.Core.Exception("AMDAPPSDK was not installed correctly");
                    }

                    installPath = installPath.TrimEnd(new[] { System.IO.Path.DirectorySeparatorChar });
                    Bam.Core.Log.DebugMessage("AMD APP SDK 2.9: Installation path from registry '{0}'", installPath);
                    installPath = System.IO.Path.Combine(installPath, "AMD APP SDK");
                    installPath = System.IO.Path.Combine(installPath, "2.9");
                    InstallPath = installPath;
                }
            }

            IncludePath = System.IO.Path.Combine(InstallPath, "include");
            LibraryPath = System.IO.Path.Combine(InstallPath, "lib");
        }

        public AMDAPPSDK()
        {
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(AMDAPPSDK_IncludePath);
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(AMDAPPSDK_LinkerOptions);
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(AMDAPPSDK_EnableExceptionHandling);
        }

        [C.ExportCompilerOptionsDelegate]
        void AMDAPPSDK_EnableExceptionHandling(Bam.Core.IModule module, Bam.Core.Target target)
        {
            var compilerOptions = module.Options as C.ICxxCompilerOptions;
            if (null == compilerOptions)
            {
                return;
            }

            compilerOptions.ExceptionHandler = C.Cxx.EExceptionHandler.Asynchronous;
        }

        [C.ExportCompilerOptionsDelegate]
        void AMDAPPSDK_IncludePath(Bam.Core.IModule module, Bam.Core.Target target)
        {
            var compilerOptions = module.Options as C.ICCompilerOptions;
            if (null == compilerOptions)
            {
                return;
            }

            compilerOptions.IncludePaths.Add(IncludePath);
        }

        [C.ExportLinkerOptionsDelegate]
        void AMDAPPSDK_LinkerOptions(Bam.Core.IModule module, Bam.Core.Target target)
        {
            var linkerOptions = module.Options as C.ILinkerOptions;
            if (null == linkerOptions)
            {
                return;
            }

            // set library paths
            string platformLibraryPath = null;
            if (target.HasPlatform(Bam.Core.EPlatform.Win32))
            {
                platformLibraryPath = System.IO.Path.Combine(LibraryPath, "x86");
            }
            else if (target.HasPlatform(Bam.Core.EPlatform.Win64))
            {
                platformLibraryPath = System.IO.Path.Combine(LibraryPath, "x86_64");
            }
            else
            {
                throw new Bam.Core.Exception("Unsupported platform for the DirectX package");
            }
            linkerOptions.LibraryPaths.Add(platformLibraryPath);

            // libraries to link against
            linkerOptions.Libraries.Add("OpenCL.lib");
        }

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows)]
        Bam.Core.TypeArray winDependents = new Bam.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));

        public static string InstallPath
        {
            get;
            private set;
        }

        public static string IncludePath
        {
            get;
            private set;
        }

        public static string LibraryPath
        {
            get;
            private set;
        }
    }
}
