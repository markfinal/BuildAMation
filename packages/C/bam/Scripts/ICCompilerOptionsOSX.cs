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
using System.Linq;
namespace C
{
namespace V2
{
namespace DefaultSettings
{
    public static partial class DefaultSettingsExtensions
    {
        public static void
        Defaults(
            this C.V2.ICCompilerOptionsOSX settings,
            Bam.Core.Module module)
        {
        }
        public static void
        Empty(
            this C.V2.ICCompilerOptionsOSX settings)
        {
            settings.FrameworkSearchDirectories = new Bam.Core.Array<Bam.Core.TokenizedString>();
        }
        public static void
        SharedSettings(
            this C.V2.ICCompilerOptionsOSX shared,
            C.V2.ICCompilerOptionsOSX lhs,
            C.V2.ICCompilerOptionsOSX rhs)
        {
            shared.FrameworkSearchDirectories = new Bam.Core.Array<Bam.Core.TokenizedString>(lhs.FrameworkSearchDirectories.Intersect(rhs.FrameworkSearchDirectories));
        }
        public static void
        Delta(
            this C.V2.ICCompilerOptionsOSX delta,
            C.V2.ICCompilerOptionsOSX lhs,
            C.V2.ICCompilerOptionsOSX rhs)
        {
            delta.FrameworkSearchDirectories = new Bam.Core.Array<Bam.Core.TokenizedString>(lhs.FrameworkSearchDirectories.Except(rhs.FrameworkSearchDirectories));
        }
        public static void
        Clone(
            this C.V2.ICCompilerOptionsOSX settings,
            C.V2.ICCompilerOptionsOSX other)
        {
            foreach (var path in other.FrameworkSearchDirectories)
            {
                settings.FrameworkSearchDirectories.AddUnique(path);
            }
        }
    }
}
    [Bam.Core.SettingsExtensions(typeof(C.V2.DefaultSettings.DefaultSettingsExtensions))]
    public interface ICCompilerOptionsOSX :
        Bam.Core.ISettingsBase
    {
        Bam.Core.Array<Bam.Core.TokenizedString> FrameworkSearchDirectories
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
                this C.V2.ICCompilerOptionsWin settings,
                Bam.Core.Module module)
            {
                settings.CharacterSet = ECharacterSet.NotSet;
            }
            public static void
            Empty(
                this C.V2.ICCompilerOptionsWin settings)
            {
            }
            public static void
            SharedSettings(
                this C.V2.ICCompilerOptionsWin shared,
                C.V2.ICCompilerOptionsWin lhs,
                C.V2.ICCompilerOptionsWin rhs)
            {
                shared.CharacterSet = (lhs.CharacterSet == rhs.CharacterSet) ? lhs.CharacterSet : null;
            }
            public static void
            Delta(
                this C.V2.ICCompilerOptionsWin delta,
                C.V2.ICCompilerOptionsWin lhs,
                C.V2.ICCompilerOptionsWin rhs)
            {
                delta.CharacterSet = (lhs.CharacterSet != rhs.CharacterSet) ? lhs.CharacterSet : null;
            }
            public static void
            Clone(
                this C.V2.ICCompilerOptionsWin settings,
                C.V2.ICCompilerOptionsWin other)
            {
                settings.CharacterSet = other.CharacterSet;
            }
        }
    }
    [Bam.Core.SettingsExtensions(typeof(C.V2.DefaultSettings.DefaultSettingsExtensions))]
    public interface ICCompilerOptionsWin :
        Bam.Core.ISettingsBase
    {
        C.ECharacterSet? CharacterSet
        {
            get;
            set;
        }
    }
}
}
