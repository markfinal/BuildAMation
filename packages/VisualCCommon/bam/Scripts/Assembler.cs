#region License
// Copyright (c) 2010-2019, Mark Final
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
    /// <summary>
    /// Abstract base class for VisualC assemblers
    /// </summary>
    public abstract class AssemblerBase :
        C.AssemblerTool
    {
        private string
        getAssemblerPath(
            string executable,
            C.EBit depth)
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
            message.AppendLine($"Unable to locate {executable} for {(int)depth}-bit on these search locations:");
            foreach (var path in this.EnvironmentVariables["PATH"])
            {
                message.AppendLine($"\t{path.ToString()}");
            }
            throw new Bam.Core.Exception(message.ToString());
        }

        /// <summary>
        /// Create an instance
        /// </summary>
        /// <param name="depth">for this bit depth</param>
        /// <param name="basename">basename of the assembler tool</param>
        protected AssemblerBase(
            C.EBit depth,
            string basename)
        {
            this.Macros.AddVerbatim(C.ModuleMacroNames.ObjectFileExtension, ".obj");

            var meta = Bam.Core.Graph.Instance.PackageMetaData<VisualC.MetaData>("VisualC");
            var discovery = meta as C.IToolchainDiscovery;
            discovery.Discover(depth);
            this.Version = meta.ToolchainVersion;
            this.EnvironmentVariables = meta.Environment(depth);
            this.Macros.Add("InstallPath", meta.InstallDir);
            var fullAsmExePath = this.getAssemblerPath(basename, depth);
            this.Macros.Add("AssemblerPath", Bam.Core.TokenizedString.CreateVerbatim(fullAsmExePath));
        }

        /// <summary>
        /// Executable path for the tool
        /// </summary>
        public override Bam.Core.TokenizedString Executable => this.Macros["AssemblerPath"];

        /// <summary>
        /// Command line switch for response files
        /// </summary>
        public override string UseResponseFileOption => "@";
    }

    /// <summary>
    /// Class for 32-bit assembler
    /// </summary>
    [C.RegisterAssembler("VisualC", Bam.Core.EPlatform.Windows, C.EBit.ThirtyTwo)]
    public class Assembler32 :
        AssemblerBase
    {
        /// <summary>
        /// Create an instance
        /// </summary>
        public Assembler32()
            :
            base(C.EBit.ThirtyTwo, "ml.exe")
        {}

        /// <summary>
        /// \copydoc Bam.Core.ITool.SettingsType
        /// </summary>
        public override System.Type SettingsType => typeof(VisualC.AssemblerSettings);
    }

    /// <summary>
    /// Class for 64-bit assembler
    /// </summary>
    [C.RegisterAssembler("VisualC", Bam.Core.EPlatform.Windows, C.EBit.SixtyFour)]
    public class Assembler64 :
        AssemblerBase
    {
        /// <summary>
        /// Create an instance
        /// </summary>
        public Assembler64()
            : base(C.EBit.SixtyFour, "ml64.exe")
        {}

        /// <summary>
        /// \copydoc Bam.Core.ITool.SettingsType
        /// </summary>
        public override System.Type SettingsType => typeof(VisualC.AssemblerSettings);
    }
}
