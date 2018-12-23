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
namespace C.DefaultSettings
{
    public static partial class DefaultSettingsExtensions
    {
        public static void
        Defaults(
            this C.ICommonPreprocessorSettings settings,
            Bam.Core.Module module)
        {
            settings.PreprocessorDefines.Add(System.String.Format("D_BAM_CONFIGURATION_{0}", module.BuildEnvironment.Configuration.ToString().ToUpper()));
            if (module.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                settings.PreprocessorDefines.Add("D_BAM_PLATFORM_WINDOWS");
            }
            else if (module.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
            {
                settings.PreprocessorDefines.Add("D_BAM_PLATFORM_LINUX");
            }
            else if (module.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                settings.PreprocessorDefines.Add("D_BAM_PLATFORM_OSX");
            }
            else
            {
                throw new Bam.Core.Exception("Unknown platform");
            }
            {
                var is64bit = Bam.Core.OSUtilities.Is64Bit(module.BuildEnvironment.Platform);
                var bits = (is64bit) ? 64 : 32;
                settings.PreprocessorDefines.Add("D_BAM_PLATFORM_BITS", bits.ToString());
            }
            {
                var isLittleEndian = Bam.Core.OSUtilities.IsLittleEndian;
                if (isLittleEndian)
                {
                    settings.PreprocessorDefines.Add("D_BAM_PLATFORM_LITTLEENDIAN");
                }
                else
                {
                    settings.PreprocessorDefines.Add("D_BAM_PLATFORM_BIGENDIAN");
                }
            }
            settings.TargetLanguage = ETargetLanguage.C;
        }

        public static void
        Empty(
            this C.ICommonPreprocessorSettings settings)
        {
            settings.IncludePaths = new Bam.Core.TokenizedStringArray();
            settings.PreprocessorDefines = new PreprocessorDefinitions();
            settings.PreprocessorUndefines = new Bam.Core.StringArray();
            settings.SystemIncludePaths = new Bam.Core.TokenizedStringArray();
        }

        public static void
        Intersect(
            this C.ICommonPreprocessorSettings shared,
            C.ICommonPreprocessorSettings other)
        {
            shared.PreprocessorDefines = shared.PreprocessorDefines.Intersect(other.PreprocessorDefines);
            shared.IncludePaths = shared.IncludePaths.Intersect(other.IncludePaths);
            shared.SystemIncludePaths = shared.SystemIncludePaths.Intersect(other.SystemIncludePaths);
            shared.PreprocessorUndefines = shared.PreprocessorUndefines.Intersect(other.PreprocessorUndefines);
            shared.TargetLanguage = shared.TargetLanguage.Intersect(other.TargetLanguage);
        }

        public static void
        Delta(
            this C.ICommonPreprocessorSettings delta,
            C.ICommonPreprocessorSettings lhs,
            C.ICommonPreprocessorSettings rhs)
        {
            delta.PreprocessorDefines = lhs.PreprocessorDefines.Complement(rhs.PreprocessorDefines);
            delta.IncludePaths = lhs.IncludePaths.Complement(rhs.IncludePaths);
            delta.SystemIncludePaths = lhs.SystemIncludePaths.Complement(rhs.SystemIncludePaths);
            delta.PreprocessorUndefines = lhs.PreprocessorUndefines.Complement(rhs.PreprocessorUndefines);
            delta.TargetLanguage = lhs.TargetLanguage.Complement(rhs.TargetLanguage);
        }

        public static void
        Clone(
            this C.ICommonPreprocessorSettings settings,
            C.ICommonPreprocessorSettings other)
        {
            foreach (var define in other.PreprocessorDefines)
            {
                settings.PreprocessorDefines.Add(define.Key, define.Value);
            }
            foreach (var path in other.IncludePaths)
            {
                settings.IncludePaths.AddUnique(path);
            }
            foreach (var path in other.SystemIncludePaths)
            {
                settings.SystemIncludePaths.AddUnique(path);
            }
            foreach (var path in other.PreprocessorUndefines)
            {
                settings.PreprocessorUndefines.AddUnique(path);
            }
            settings.TargetLanguage = other.TargetLanguage;
        }
    }
}
