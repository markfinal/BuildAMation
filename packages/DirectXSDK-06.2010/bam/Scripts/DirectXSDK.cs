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

    sealed class Direct3D9 :
        C.CSDKModule
    {
        public Direct3D9()
        {
            var installPath = Direct3D9Location.InstallPath;

            this.Macros.Add("InstallPath", installPath);
            this.Macros.Add("IncludePath", Bam.Core.TokenizedString.Create("$(InstallPath)/include", this));
            this.Macros.Add("LibraryPath", Bam.Core.TokenizedString.Create("$(InstallPath)/lib", this));
        }

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.PublicPatch((settings, appliedTo) =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    if (null != compiler)
                    {
                        compiler.IncludePaths.Add(this.Macros["IncludePath"]);
                    }

                    var linker = settings as C.ICommonLinkerSettings;
                    if (null != linker)
                    {
                        if ((appliedTo as C.CModule).BitDepth == C.EBit.ThirtyTwo)
                        {
                            linker.LibraryPaths.Add(Bam.Core.TokenizedString.Create("$(LibraryPath)/x86", this));
                        }
                        else
                        {
                            linker.LibraryPaths.Add(Bam.Core.TokenizedString.Create("$(LibraryPath)/x64", this));
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
            Bam.Core.ExecutionContext context)
        {
            // do nothing
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            // do nothing
        }
    }
}
