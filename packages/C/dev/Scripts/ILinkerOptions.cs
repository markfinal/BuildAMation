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
using Bam.Core.V2; // for EPlatform.PlatformExtensions
namespace C
{
namespace V2
{
namespace DefaultSettings
{
    public static partial class DefaultSettingsExtensions
    {
        public static void Defaults(this C.V2.ICommonLinkerOptions settings, Bam.Core.V2.Module module)
        {
            settings.OutputType = ELinkerOutput.Executable;
            settings.LibraryPaths = new Bam.Core.Array<Bam.Core.V2.TokenizedString>();
            settings.Libraries = new Bam.Core.StringArray();
            settings.DebugSymbols = (module.BuildEnvironment.Configuration == Bam.Core.EConfiguration.Debug || module.BuildEnvironment.Configuration == Bam.Core.EConfiguration.Profile);
        }
    }
}

    [Bam.Core.V2.SettingsExtensions(typeof(C.V2.DefaultSettings.DefaultSettingsExtensions))]
    public interface ICommonLinkerOptions : Bam.Core.V2.ISettingsBase
    {
        C.ELinkerOutput OutputType
        {
            get;
            set;
        }

        Bam.Core.Array<Bam.Core.V2.TokenizedString> LibraryPaths
        {
            get;
            set;
        }

        Bam.Core.StringArray Libraries
        {
            get;
            set;
        }

        bool? DebugSymbols
        {
            get;
            set;
        }
    }
}
    public interface ILinkerOptions
    {
        /// <summary>
        /// Specify the output type of the linked binary
        /// </summary>
        C.ELinkerOutput OutputType
        {
            get;
            set;
        }

        /// <summary>
        /// Exclude standard libraries from the linking phase
        /// </summary>
        bool DoNotAutoIncludeStandardLibraries
        {
            get;
            set;
        }

        /// <summary>
        /// Generate debug symbols for the linked binary
        /// </summary>
        bool DebugSymbols
        {
            get;
            set;
        }

        /// <summary>
        /// Specify the subsystem for the linked binary
        /// </summary>
        C.ESubsystem SubSystem
        {
            get;
            set;
        }

        /// <summary>
        /// Specify search paths for libraries
        /// </summary>
        Bam.Core.DirectoryCollection LibraryPaths
        {
            get;
            set;
        }

        /// <summary>
        /// Specify standard libraries to link against
        /// </summary>
        Bam.Core.FileCollection StandardLibraries
        {
            get;
            set;
        }

        /// <summary>
        /// Specify user libraries to link against
        /// </summary>
        Bam.Core.FileCollection Libraries
        {
            get;
            set;
        }

        /// <summary>
        /// The link step generates a map file for the binary
        /// </summary>
        bool GenerateMapFile
        {
            get;
            set;
        }

        /// <summary>
        /// Additional options passed to the linker
        /// </summary>
        string AdditionalOptions
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the major version. Used by Posix shared libraries.
        /// </summary>
        /// <value>
        /// The major version.
        /// </value>
        [Bam.Core.ValueOnlyOption]
        int MajorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minor version. Used by Posix shared libraries.
        /// </summary>
        /// <value>
        /// The minor version.
        /// </value>
        [Bam.Core.ValueOnlyOption]
        int MinorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the patch version. Used by Posix shared libraries.
        /// </summary>
        /// <value>
        /// The patch version.
        /// </value>
        [Bam.Core.ValueOnlyOption]
        int PatchVersion
        {
            get;
            set;
        }
    }
}
