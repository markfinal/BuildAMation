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
        C.CSDKModule
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
                var compilation = settings as C.ICommonCompilerOptions;
                if (null != compilation)
                {
                    compilation.IncludePaths.AddUnique(Bam.Core.TokenizedString.Create(@"$(InstallPath)Include\um", this));
                    compilation.IncludePaths.AddUnique(Bam.Core.TokenizedString.Create(@"$(InstallPath)Include\shared", this));
                }

                var linking = settings as C.ICommonLinkerOptions;
                if (null != linking)
                {
                    if ((appliedTo as C.CModule).BitDepth == C.EBit.ThirtyTwo)
                    {
                        linking.LibraryPaths.AddUnique(Bam.Core.TokenizedString.Create(@"$(InstallPath)Lib\winv6.3\um\x86", this));
                    }
                    else
                    {
                        linking.LibraryPaths.AddUnique(Bam.Core.TokenizedString.Create(@"$(InstallPath)Lib\winv6.3\um\x64", this));
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

        protected override void GetExecutionPolicy(string mode)
        {
            // do nothing
        }
    }
}
