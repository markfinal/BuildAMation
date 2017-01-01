#region License
// Copyright (c) 2010-2017, Mark Final
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
namespace Publisher
{
    public sealed class StripTool :
        Bam.Core.PreBuiltTool
    {
        private Bam.Core.TokenizedString ExecutablePath;

        public StripTool()
        {
            if (Bam.Core.OSUtilities.IsWindowsHosting)
            {
                var mingwMeta = Bam.Core.Graph.Instance.PackageMetaData<Bam.Core.PackageMetaData>("Mingw");
                if (null == mingwMeta)
                {
                    throw new Bam.Core.Exception("Unable to locate Mingw");
                }
                this.ExecutablePath = this.CreateTokenizedString("$(0)/bin/strip.exe", mingwMeta["InstallDir"] as Bam.Core.TokenizedString);
            }
            else
            {
                this.ExecutablePath = Bam.Core.TokenizedString.CreateVerbatim("strip");
            }
        }

        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module)
        {
            return new StripToolSettings(module);
        }

        public override Bam.Core.TokenizedString Executable
        {
            get
            {
                return this.ExecutablePath;
            }
        }

        public override void
        Evaluate()
        {
            this.ReasonToExecute = null;
        }
    }
}
