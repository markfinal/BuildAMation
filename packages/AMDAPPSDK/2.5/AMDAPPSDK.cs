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
                using (var key = Bam.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(@"ATI Technologies\Install\AMD APP SDK Developer"))
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
                    Bam.Core.Log.DebugMessage("AMD APP SDK 2.5: Installation path from registry '{0}'", installPath);
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
