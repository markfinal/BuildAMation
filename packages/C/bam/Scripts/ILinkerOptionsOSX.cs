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
namespace C
{
namespace V2
{
namespace DefaultSettings
{
    public static partial class DefaultSettingsExtensions
    {
        public static void Defaults(this C.V2.ILinkerOptionsOSX settings, Bam.Core.Module module)
        {
            settings.Frameworks = new Bam.Core.Array<Bam.Core.TokenizedString>();
            settings.FrameworkSearchDirectories = new Bam.Core.Array<Bam.Core.TokenizedString>();
        }
    }
}
    [Bam.Core.SettingsExtensions(typeof(C.V2.DefaultSettings.DefaultSettingsExtensions))]
    public interface ILinkerOptionsOSX : Bam.Core.ISettingsBase
    {
        Bam.Core.Array<Bam.Core.TokenizedString> Frameworks
        {
            get;
            set;
        }

        Bam.Core.Array<Bam.Core.TokenizedString> FrameworkSearchDirectories
        {
            get;
            set;
        }

        Bam.Core.TokenizedString InstallName
        {
            get;
            set;
        }
    }
}
namespace V2
{
    namespace DefaultSettings
    {
        public static partial class DefaultSettingsExtensions
        {
            public static void
            Defaults(
                this C.V2.ILinkerOptionsWin settings,
                Bam.Core.Module module)
            {
                settings.SubSystem = ESubsystem.Console;
            }
            public static void
            Empty(
                this C.V2.ILinkerOptionsWin settings)
            {
            }
            public static void
            SharedSettings(
                this C.V2.ILinkerOptionsWin shared,
                C.V2.ILinkerOptionsWin lhs,
                C.V2.ILinkerOptionsWin rhs)
            {
                shared.SubSystem = (lhs.SubSystem == rhs.SubSystem) ? lhs.SubSystem : null;
            }
            public static void
            Delta(
                this C.V2.ILinkerOptionsWin delta,
                C.V2.ILinkerOptionsWin lhs,
                C.V2.ILinkerOptionsWin rhs)
            {
                delta.SubSystem = (lhs.SubSystem != rhs.SubSystem) ? lhs.SubSystem : null;
            }
            public static void
            Clone(
                this C.V2.ILinkerOptionsWin settings,
                C.V2.ILinkerOptionsWin other)
            {
                settings.SubSystem = other.SubSystem;
            }
        }
    }
    [Bam.Core.SettingsExtensions(typeof(C.V2.DefaultSettings.DefaultSettingsExtensions))]
    public interface ILinkerOptionsWin :
        Bam.Core.ISettingsBase
    {
        C.ESubsystem? SubSystem
        {
            get;
            set;
        }
    }
}
}
