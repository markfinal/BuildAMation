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
namespace VisualCCommon
{
    public abstract class AssemblerBase :
        C.AssemblerTool
    {
        protected AssemblerBase()
        {
            var meta = Bam.Core.Graph.Instance.PackageMetaData<VisualC.MetaData>("VisualC");
            this.Macros.Add("InstallPath", meta.InstallDir);
            this.Macros.AddVerbatim("objext", ".obj");

            if (null != meta.RequiredExecutablePaths)
            {
                this.EnvironmentVariables.Add("PATH", meta.RequiredExecutablePaths);
            }

            if (meta.UseWindowsSDKPublicPatches)
            {
                var windowsSDK = Bam.Core.Graph.Instance.FindReferencedModule<WindowsSDK.WindowsSDK>();
                this.UsePublicPatches(windowsSDK);
            }
        }

        public override Bam.Core.TokenizedString Executable
        {
            get
            {
                return this.Macros["AssemblerPath"];
            }
        }

        public override string UseResponseFileOption
        {
            get
            {
                return "@";
            }
        }

        public override Bam.Core.Settings CreateDefaultSettings<T>(T module)
        {
            if (typeof(C.AssembledObjectFile).IsInstanceOfType(module) ||
                typeof(C.AssembledObjectFileCollection).IsInstanceOfType(module))
            {
                var settings = new VisualC.AssemblerSettings(module);
                this.OverrideDefaultSettings(settings);
                return settings;
            }
            else
            {
                throw new Bam.Core.Exception("Could not determine type of module {0}", typeof(T).ToString());
            }
        }

        protected abstract void
        OverrideDefaultSettings(
            Bam.Core.Settings settings);
    }

    [C.RegisterAssembler("VisualC", Bam.Core.EPlatform.Windows, C.EBit.ThirtyTwo)]
    public class Assembler32 :
        AssemblerBase
    {
        public Assembler32()
        {
            var meta = Bam.Core.Graph.Instance.PackageMetaData<VisualC.MetaData>("VisualC");
            this.Macros.Add("BinPath", meta.Bin32Dir);
            this.Macros.Add("AssemblerPath", this.CreateTokenizedString(@"$(BinPath)\ml.exe"));
        }

        protected override void
        OverrideDefaultSettings(
            Bam.Core.Settings settings)
        {
        }
    }

    [C.RegisterAssembler("VisualC", Bam.Core.EPlatform.Windows, C.EBit.SixtyFour)]
    public class Assembler64 :
        AssemblerBase
    {
        public Assembler64()
            : base()
        {
            var meta = Bam.Core.Graph.Instance.PackageMetaData<VisualC.MetaData>("VisualC");
            this.Macros.Add("BinPath", meta.Bin64Dir);
            this.Macros.Add("AssemblerPath", this.CreateTokenizedString(@"$(BinPath)\ml64.exe"));
        }

        protected override void
        OverrideDefaultSettings(
            Bam.Core.Settings settings)
        {
        }
    }
}
