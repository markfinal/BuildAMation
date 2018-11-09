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
using Bam.Core;
namespace Publisher
{
    [CommandLineProcessor.OutputPath(StripModule.StripBinaryKey, "-o ")]
    [CommandLineProcessor.InputPaths(CollatedObject.CopiedFileKey, "", max_file_count: 1)]
    public sealed class StripToolSettings :
        Bam.Core.Settings,
        IStripToolSettings
    {
        public StripToolSettings()
        {}

        public StripToolSettings(
            Bam.Core.Module module) => this.InitializeAllInterfaces(module, false, true);

        [CommandLineProcessor.Bool("-v", "")]
        bool IStripToolSettings.Verbose { get; set; }

        [CommandLineProcessor.Bool("-p", "")]
        bool IStripToolSettings.PreserveTimestamp { get; set; }

        [CommandLineProcessor.Bool("-S", "")]
        bool IStripToolSettings.StripDebugSymbols { get; set; }

        [CommandLineProcessor.Bool("-x", "")]
        bool IStripToolSettings.StripLocalSymbols { get; set; }

        public override void
        AssignFileLayout()
        {
            this.FileLayout = ELayout.Cmds_Outputs_Inputs;
        }

        public override void
        Validate()
        {
            base.Validate();

            if ((this as IStripToolSettings).Verbose || (this as IStripToolSettings).PreserveTimestamp)
            {
                if (this.Module.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
                {
                    throw new Bam.Core.Exception(
                        "Cannot use Verbose or PreserveTimestamp settings for macOS strip"
                    );
                }
            }
        }
    }
}
