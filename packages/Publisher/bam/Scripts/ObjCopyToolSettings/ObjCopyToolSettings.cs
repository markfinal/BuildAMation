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
namespace Publisher
{
    /// <summary>
    /// Class representing objcopy tool settings
    /// </summary>
    [CommandLineProcessor.OutputPath(MakeDebugSymbolFile.DebugSymbolFileKey, "")]
    [CommandLineProcessor.InputPaths(StripModule.StripBinaryKey, "", max_file_count: 1)]
    [CommandLineProcessor.InputPaths(C.ConsoleApplication.ExecutableKey, "", max_file_count: 1)]
    public sealed class MakeDebugSymbolFileSettings :
        Bam.Core.Settings,
        IObjCopyToolSettings
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public MakeDebugSymbolFileSettings()
        {}

        /// <summary>
        /// Create a settings instance
        /// </summary>
        /// <param name="module">for this Module</param>
        public MakeDebugSymbolFileSettings(
            Bam.Core.Module module)
        {
            this.InitializeAllInterfaces(module, false, true);
        }

        [CommandLineProcessor.Bool("--only-keep-debug", "")]
        bool IObjCopyToolSettings.OnlyKeepDebug { get; set; }

        [CommandLineProcessor.Bool("-v", "")]
        bool IObjCopyToolSettings.Verbose { get; set; }

        public override void AssignFileLayout ()
        {
            this.FileLayout = ELayout.Cmds_Inputs_Outputs;
        }
    }

    [CommandLineProcessor.OutputPath(LinkBackDebugSymbolFile.UpdateOriginalExecutable, "")]
    [CommandLineProcessor.InputPaths(MakeDebugSymbolFile.DebugSymbolFileKey, "--add-gnu-debuglink=", max_file_count: 1)]
    public sealed class LinkBackDebugSymbolFileSettings :
        Bam.Core.Settings,
        IObjCopyToolSettings
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public LinkBackDebugSymbolFileSettings()
        {}

        /// <summary>
        /// Create a settings instance
        /// </summary>
        /// <param name="module">for this module</param>
        public LinkBackDebugSymbolFileSettings(
            Bam.Core.Module module)
        {
            this.InitializeAllInterfaces(module, false, true);
        }

        [CommandLineProcessor.Bool("--only-keep-debug", "")]
        bool IObjCopyToolSettings.OnlyKeepDebug { get; set; }

        [CommandLineProcessor.Bool("-v", "")]
        bool IObjCopyToolSettings.Verbose { get; set; }

        public override void AssignFileLayout ()
        {
            this.FileLayout = ELayout.Cmds_Inputs_Outputs;
        }
    }
}
