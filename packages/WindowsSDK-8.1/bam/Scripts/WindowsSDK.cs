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
        C.CSDKModule
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            var meta = Bam.Core.Graph.Instance.PackageMetaData<Bam.Core.PackageMetaData>("WindowsSDK");
            var installDir = meta["InstallDir"] as Bam.Core.TokenizedString;
            this.PublicPatch((settings, appliedTo) =>
            {
                var compilation = settings as C.ICommonCompilerSettings;
                if (null != compilation)
                {
                    compilation.IncludePaths.AddUnique(this.CreateTokenizedString(@"$(0)Include\um", installDir));
                    compilation.IncludePaths.AddUnique(this.CreateTokenizedString(@"$(0)Include\shared", installDir));
                }

                var linking = settings as C.ICommonLinkerSettings;
                if (null != linking)
                {
                    if ((appliedTo as C.CModule).BitDepth == C.EBit.ThirtyTwo)
                    {
                        linking.LibraryPaths.AddUnique(this.CreateTokenizedString(@"$(0)Lib\winv6.3\um\x86", installDir));
                    }
                    else
                    {
                        linking.LibraryPaths.AddUnique(this.CreateTokenizedString(@"$(0)Lib\winv6.3\um\x64", installDir));
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
