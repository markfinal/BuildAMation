#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
namespace C
{
namespace V2
{
namespace DefaultSettings
{
    public static partial class DefaultSettingsExtensions
    {
        public static void Defaults(this C.V2.ICommonCompilerOptions settings, Bam.Core.V2.Module module)
        {
            settings.Bits = EBit.SixtyFour;
            settings.DebugSymbols = module.BuildEnvironment.Configuration == Bam.Core.EConfiguration.Debug;
            settings.DisableWarnings = new Bam.Core.StringArray();
            settings.IncludePaths = new Bam.Core.Array<Bam.Core.V2.TokenizedString>();
            settings.LanguageStandard = ELanguageStandard.C89;
            settings.OmitFramePointer = false;
            settings.Optimization = EOptimization.Off;
            settings.OutputType = ECompilerOutput.CompileOnly;
            settings.PreprocessorDefines = new C.V2.PreprocessorDefinitions();
            settings.PreprocessorDefines.Add(System.String.Format("D_BAM_CONFIGURATION_{0}", module.BuildEnvironment.Configuration.ToString().ToUpper()));
            settings.PreprocessorUndefines = new Bam.Core.StringArray();
            settings.SystemIncludePaths = new Bam.Core.Array<Bam.Core.V2.TokenizedString>();
            settings.TargetLanguage = ETargetLanguage.C;
            settings.WarningsAsErrors = true;
        }
    }
}
    public enum EBit
    {
        ThirtyTwo = 32,
        SixtyFour = 64
    }

    public interface ICommonCompilerOptions
    {
        EBit Bits
        {
            get;
            set;
        }

        C.V2.PreprocessorDefinitions PreprocessorDefines
        {
            get;
            set;
        }

        Bam.Core.Array<Bam.Core.V2.TokenizedString> IncludePaths
        {
            get;
            set;
        }

        Bam.Core.Array<Bam.Core.V2.TokenizedString> SystemIncludePaths
        {
            get;
            set;
        }

        C.ECompilerOutput OutputType
        {
            get;
            set;
        }

        bool DebugSymbols
        {
            get;
            set;
        }

        bool WarningsAsErrors
        {
            get;
            set;
        }

        C.EOptimization Optimization
        {
            get;
            set;
        }

        C.ETargetLanguage TargetLanguage
        {
            get;
            set;
        }

        C.ELanguageStandard LanguageStandard
        {
            get;
            set;
        }

        bool OmitFramePointer
        {
            get;
            set;
        }

        Bam.Core.StringArray DisableWarnings
        {
            get;
            set;
        }

        Bam.Core.StringArray PreprocessorUndefines
        {
            get;
            set;
        }
    }

    public interface ICOnlyCompilerOptions
    {
        bool C99Specific
        {
            get;
            set;
        }
    }
}
    public interface ICCompilerOptions
    {
        /// <summary>
        /// Preprocessor definitions
        /// </summary>
        C.DefineCollection Defines
        {
            get;
            set;
        }

        /// <summary>
        /// Preprocessor user header search paths: #includes of the style #include "..."
        /// </summary>
        Bam.Core.DirectoryCollection IncludePaths
        {
            get;
            set;
        }

        /// <summary>
        /// Preprocessor system header search paths: #includes of the stylr #include <...>
        /// </summary>
        Bam.Core.DirectoryCollection SystemIncludePaths
        {
            get;
            set;
        }

        /// <summary>
        /// Type of file output from the compiler
        /// </summary>
        C.ECompilerOutput OutputType
        {
            get;
            set;
        }

        /// <summary>
        /// Compiled objects contain debug symbols
        /// </summary>
        bool DebugSymbols
        {
            get;
            set;
        }

        /// <summary>
        /// Treat all warnings as errors
        /// </summary>
        bool WarningsAsErrors
        {
            get;
            set;
        }

        /// <summary>
        /// Compiler ignores all built in standard include paths. User must provide them.
        /// </summary>
        bool IgnoreStandardIncludePaths
        {
            get;
            set;
        }

        /// <summary>
        /// The level of optimization the compiler uses
        /// </summary>
        C.EOptimization Optimization
        {
            get;
            set;
        }

        /// <summary>
        /// Custom optimization settings not provided by the Optimization option.
        /// </summary>
        string CustomOptimization
        {
            get;
            set;
        }

        /// <summary>
        /// The target language of the compiled source code
        /// </summary>
        C.ETargetLanguage TargetLanguage
        {
            get;
            set;
        }

        /// <summary>
        /// Display the paths of the header files included on stdout
        /// </summary>
        bool ShowIncludes
        {
            get;
            set;
        }

        /// <summary>
        /// Explicit compiler options added to the compilation step
        /// </summary>
        string AdditionalOptions
        {
            get;
            set;
        }

        /// <summary>
        /// Omit the frame pointer from the compiled object code for each stack frame
        /// </summary>
        bool OmitFramePointer
        {
            get;
            set;
        }

        /// <summary>
        /// A list of warnings that are disabled
        /// </summary>
        Bam.Core.StringArray DisableWarnings
        {
            get;
            set;
        }

        /// <summary>
        /// The target character set in use by the compiled code
        /// </summary>
        C.ECharacterSet CharacterSet
        {
            get;
            set;
        }

        /// <summary>
        /// The language standard for C/C++ compilation
        /// </summary>
        C.ELanguageStandard LanguageStandard
        {
            get;
            set;
        }

        /// <summary>
        /// Preprocessor definitions to be undefined
        /// </summary>
        C.DefineCollection Undefines
        {
            get;
            set;
        }
    }
}
