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

            var tc = C.ToolchainFactory.GetTargetInstance(target);
            var platformSDKPath = System.IO.Path.Combine(tc.InstallPath(target), "PlatformSDK");

            if (System.IO.Directory.Exists(platformSDKPath))
            {
                installPath = platformSDKPath;
            }
            else
            {
                throw new Bam.Core.Exception("PlatformSDK with VisualStudio 8.0 was not installed");
            }
            Bam.Core.Log.DebugMessage("Windows SDK installation folder is from the MSVC PlatformSDK: {0}", installPath);

            bin32Path = System.IO.Path.Combine(installPath, "bin");
            bin64Path = System.IO.Path.Combine(bin32Path, "win64");
            bin64Path = System.IO.Path.Combine(bin64Path, "AMD64");

            lib32Path = System.IO.Path.Combine(installPath, "lib");
            lib64Path = System.IO.Path.Combine(lib32Path, "AMD64");

            includePath = System.IO.Path.Combine(installPath, "include");

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
            if (target.Platform == Bam.Core.EPlatform.Win32)
            {
                linkerOptions.LibraryPaths.Add(lib32Path);
            }
            else if (target.Platform == Bam.Core.EPlatform.Win64)
            {
                linkerOptions.LibraryPaths.Add(lib64Path);
            }
            else
            {
                throw new Bam.Core.Exception("Windows SDK is not supported for platform '{0}'; use win32 or win64", target.Platform);
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void
        WindowsSDK_IncludePaths(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.IncludePaths.Add(includePath);
        }

        public override Bam.Core.StringArray
        Libraries(
            Bam.Core.Target target)
        {
            throw new System.NotImplementedException();
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
