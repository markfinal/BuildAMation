#region License
// Copyright (c) 2010-2016, Mark Final
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
using System.Linq;
namespace C.DefaultSettings
{
    public static partial class DefaultSettingsExtensions
    {
        public static void
        Defaults(
            this C.ICommonCompilerSettings settings,
            Bam.Core.Module module)
        {
            settings.Bits = (module as CModule).BitDepth;
            settings.DebugSymbols = (0 != (module.BuildEnvironment.Configuration & (Bam.Core.EConfiguration.Debug | Bam.Core.EConfiguration.Profile)));
            settings.OmitFramePointer = (0 != (module.BuildEnvironment.Configuration & Bam.Core.EConfiguration.NotDebug));
            settings.Optimization = (0 != (module.BuildEnvironment.Configuration & Bam.Core.EConfiguration.NotDebug)) ? EOptimization.Speed : EOptimization.Off;
            settings.OutputType = ECompilerOutput.CompileOnly;
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
            settings.WarningsAsErrors = true;
        }

        public static void
        Empty(
            this C.ICommonCompilerSettings settings)
        {
            settings.DisableWarnings = new Bam.Core.StringArray();
            settings.IncludePaths = new Bam.Core.TokenizedStringArray();
            settings.PreprocessorDefines = new PreprocessorDefinitions();
            settings.PreprocessorUndefines = new Bam.Core.StringArray();
            settings.SystemIncludePaths = new Bam.Core.TokenizedStringArray();
        }

        public static void
        Intersect(
            this C.ICommonCompilerSettings shared,
            C.ICommonCompilerSettings other)
        {
            if (shared.Bits != other.Bits)
            {
                shared.Bits = null;
            }
            shared.PreprocessorDefines = new PreprocessorDefinitions(shared.PreprocessorDefines.Intersect(other.PreprocessorDefines));
            shared.IncludePaths = new Bam.Core.TokenizedStringArray(shared.IncludePaths.Intersect(other.IncludePaths));
            shared.SystemIncludePaths = new Bam.Core.TokenizedStringArray(shared.SystemIncludePaths.Intersect(other.SystemIncludePaths));
            if (shared.OutputType != other.OutputType)
            {
                shared.OutputType = null;
            }
            if (shared.DebugSymbols != other.DebugSymbols)
            {
                shared.DebugSymbols = null;
            }
            if (shared.WarningsAsErrors != other.WarningsAsErrors)
            {
                shared.WarningsAsErrors = null;
            }
            if (shared.Optimization != other.Optimization)
            {
                shared.Optimization = null;
            }
            if (shared.TargetLanguage != other.TargetLanguage)
            {
                shared.TargetLanguage = null;
            }
            if (shared.OmitFramePointer != other.OmitFramePointer)
            {
                shared.OmitFramePointer = null;
            }
            shared.DisableWarnings = new Bam.Core.StringArray(shared.DisableWarnings.Intersect(other.DisableWarnings));
            shared.PreprocessorUndefines = new Bam.Core.StringArray(shared.PreprocessorUndefines.Intersect(other.PreprocessorUndefines));
        }

        public static void
        Delta(
            this C.ICommonCompilerSettings delta,
            C.ICommonCompilerSettings lhs,
            C.ICommonCompilerSettings rhs)
        {
            delta.Bits = (lhs.Bits != rhs.Bits) ? lhs.Bits : null;
            delta.PreprocessorDefines = new PreprocessorDefinitions(lhs.PreprocessorDefines.Except(rhs.PreprocessorDefines));
            delta.IncludePaths = new Bam.Core.TokenizedStringArray(lhs.IncludePaths.Except(rhs.IncludePaths));
            delta.SystemIncludePaths = new Bam.Core.TokenizedStringArray(lhs.SystemIncludePaths.Except(rhs.SystemIncludePaths));
            delta.OutputType = (lhs.OutputType != rhs.OutputType) ? lhs.OutputType : null;
            delta.DebugSymbols = (lhs.DebugSymbols != rhs.DebugSymbols) ? lhs.DebugSymbols : null;
            delta.WarningsAsErrors = (lhs.WarningsAsErrors != rhs.WarningsAsErrors) ? lhs.WarningsAsErrors : null;
            delta.Optimization = (lhs.Optimization != rhs.Optimization) ? lhs.Optimization : null;
            delta.TargetLanguage = (lhs.TargetLanguage != rhs.TargetLanguage) ? lhs.TargetLanguage : null;
            delta.OmitFramePointer = (lhs.OmitFramePointer != rhs.OmitFramePointer) ? lhs.OmitFramePointer : null;
            delta.DisableWarnings = new Bam.Core.StringArray(lhs.DisableWarnings.Except(rhs.DisableWarnings));
            delta.PreprocessorUndefines = new Bam.Core.StringArray(lhs.PreprocessorUndefines.Except(rhs.PreprocessorUndefines));
        }

        public static void
        Clone(
            this C.ICommonCompilerSettings settings,
            C.ICommonCompilerSettings other)
        {
            settings.Bits = other.Bits;
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
            settings.OutputType = other.OutputType;
            settings.DebugSymbols = other.DebugSymbols;
            settings.WarningsAsErrors = other.WarningsAsErrors;
            settings.Optimization = other.Optimization;
            settings.TargetLanguage = other.TargetLanguage;
            settings.OmitFramePointer = other.OmitFramePointer;
            foreach (var path in other.DisableWarnings)
            {
                settings.DisableWarnings.AddUnique(path);
            }
            foreach (var path in other.PreprocessorUndefines)
            {
                settings.PreprocessorUndefines.AddUnique(path);
            }
        }
    }
    public static partial class DefaultSettingsExtensions
    {
        public static void
        Defaults(
            this C.IAdditionalSettings settings,
            Bam.Core.Module module)
        {
            if (null == settings.AdditionalSettings)
            {
                settings.AdditionalSettings = new Bam.Core.StringArray();
            }
        }

        public static void
        Empty(
            this C.IAdditionalSettings settings)
        {
            settings.AdditionalSettings = new Bam.Core.StringArray();
        }

        public static void
        Intersect(
            this C.IAdditionalSettings shared,
            C.IAdditionalSettings other)
        {
            shared.AdditionalSettings = new Bam.Core.StringArray(shared.AdditionalSettings.Intersect(other.AdditionalSettings));
        }

        public static void
        Delta(
            this C.IAdditionalSettings delta,
            C.IAdditionalSettings lhs,
            C.IAdditionalSettings rhs)
        {
            delta.AdditionalSettings = new Bam.Core.StringArray(lhs.AdditionalSettings.Except(rhs.AdditionalSettings));
        }

        public static void
        Clone(
            this C.IAdditionalSettings settings,
            C.IAdditionalSettings other)
        {
            foreach (var path in other.AdditionalSettings)
            {
                settings.AdditionalSettings.AddUnique(path);
            }
        }
    }
}
