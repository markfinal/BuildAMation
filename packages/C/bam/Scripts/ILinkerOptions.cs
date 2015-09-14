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
        public static void
        Defaults(
            this C.V2.ICxxOnlyLinkerOptions settings,
            Bam.Core.V2.Module module)
        {
            settings.StandardLibrary = C.Cxx.EStandardLibrary.NotSet;
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

    [Bam.Core.V2.SettingsExtensions(typeof(C.V2.DefaultSettings.DefaultSettingsExtensions))]
    public interface ICxxOnlyLinkerOptions :
        Bam.Core.V2.ISettingsBase
    {
        C.Cxx.EStandardLibrary? StandardLibrary
        {
            get;
            set;
        }
    }
}
}
