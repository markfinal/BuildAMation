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
    /// Class representing the cp tool settings
    /// </summary>
    [CommandLineProcessor.OutputPath(CollatedObject.CopiedFileKey, "")]
    [CommandLineProcessor.OutputPath(CollatedObject.CopiedDirectoryKey, "")]
    [CommandLineProcessor.OutputPath(CollatedObject.CopiedRenamedDirectoryKey, "")]
    [CommandLineProcessor.AnyInputFile("", path_modifier_if_directory: "$(0)/.")]
    public sealed class PosixCopyFileSettings :
        Bam.Core.Settings,
        ICopyFileSettings
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public PosixCopyFileSettings()
            :
            base(ELayout.Cmds_Inputs_Outputs)
        {}

        [CommandLineProcessor.Bool("-f", "")]
        bool ICopyFileSettings.Force { get; set; }

        [CommandLineProcessor.Bool("-v", "")]
        bool ICopyFileSettings.Verbose { get; set; }

        [CommandLineProcessor.Bool("-R", "")]
        bool ICopyFileSettings.Recursive { get; set; }

        [CommandLineProcessor.Bool("-a", "")]
        bool ICopyFileSettings.PreserveAllAttributes { get; set; }
    }

    /// <summary>
    /// Class representing the xcopy tool settings
    /// </summary>
    [CommandLineProcessor.OutputPath(CollatedObject.CopiedFileKey, "", path_modifier: "@dir($(0))/")]
    [CommandLineProcessor.OutputPath(CollatedObject.CopiedDirectoryKey, "", path_modifier: "$(0)/")]
    [CommandLineProcessor.OutputPath(CollatedObject.CopiedRenamedDirectoryKey, "", path_modifier: "$(0)/")]
    [CommandLineProcessor.AnyInputFile("")]
    public sealed class WinCopyFileSettings :
        Bam.Core.Settings,
        ICopyFileSettings
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public WinCopyFileSettings()
            :
            base(ELayout.Inputs_Outputs_Cmds)
        {}

        [CommandLineProcessor.Bool("/Y /R", "")]
        bool ICopyFileSettings.Force { get; set; }

        [CommandLineProcessor.Bool("/F", "")]
        bool ICopyFileSettings.Verbose { get; set; }

        [CommandLineProcessor.Bool("/S", "")]
        bool ICopyFileSettings.Recursive { get; set; }

        [CommandLineProcessor.Bool("/K /B", "")]
        bool ICopyFileSettings.PreserveAllAttributes { get; set; }
    }
}
