#region License
// Copyright (c) 2010-2018, Mark Final
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
using System.Linq;
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
                return settings;
            }
            else
            {
                throw new Bam.Core.Exception("Could not determine type of module {0}", typeof(T).ToString());
            }
        }

        protected string
        getAssemblerPath(
            string executable)
        {
            foreach (var path in this.EnvironmentVariables["PATH"])
            {
                var installLocation = Bam.Core.OSUtilities.GetInstallLocation(
                    executable,
                    path.ToString(),
                    this.GetType().Name,
                    throwOnFailure: false
                );
                if (null != installLocation)
                {
                    return installLocation.First();
                }
            }
            var message = new System.Text.StringBuilder();
            message.AppendFormat("Unable to locate {0} on these search locations:", executable);
            message.AppendLine();
            foreach (var path in this.EnvironmentVariables["PATH"])
            {
                message.AppendFormat("\t{0}", path.ToString());
                message.AppendLine();
            }
            throw new Bam.Core.Exception(message.ToString());
        }
    }

    [C.RegisterAssembler("VisualC", Bam.Core.EPlatform.Windows, C.EBit.ThirtyTwo)]
    public class Assembler32 :
        AssemblerBase
    {
        public Assembler32()
        {
            var meta = Bam.Core.Graph.Instance.PackageMetaData<VisualC.MetaData>("VisualC");
            this.EnvironmentVariables = meta.Environment32;
            var fullAsmExePath = this.getAssemblerPath("ml.exe");
            this.Macros.Add("AssemblerPath", Bam.Core.TokenizedString.CreateVerbatim(fullAsmExePath));
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
            this.EnvironmentVariables = meta.Environment64;
            var fullAsmExePath = this.getAssemblerPath("ml64.exe");
            this.Macros.Add("AssemblerPath", Bam.Core.TokenizedString.CreateVerbatim(fullAsmExePath));
        }
    }
}
