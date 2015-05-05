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
using C.V2.DefaultSettings;
namespace Mingw
{
    public static class NativeImplementation
    {
        public static void Convert(this C.V2.ICommonCompilerOptions options, Bam.Core.V2.Module module)
        {
            var commandLine = module.CommandLine;
            var objectFile = module as C.V2.ObjectFile;
            if (options.Bits == C.V2.EBit.ThirtyTwo)
            {
                commandLine.Add("-m32");
            }
            else
            {
                commandLine.Add("-m64");
            }
            if (options.DebugSymbols)
            {
                commandLine.Add("-g");
            }
            foreach (var warning in options.DisableWarnings)
            {
                commandLine.Add(warning);
            }
            foreach (var path in options.IncludePaths)
            {
                commandLine.Add(System.String.Format("-I{0}", path));
            }
            switch (options.LanguageStandard)
            {
                case C.ELanguageStandard.C89:
                    break;
                case C.ELanguageStandard.C99:
                    commandLine.Add("-std=c99");
                    break;
                default:
                    // TODO: Might want to split this across C specific and Cxx specific options
                    throw new Bam.Core.Exception("Invalid language standard");
            }
            if (options.OmitFramePointer)
            {
                commandLine.Add("-fomit-frame-pointer");
            }
            else
            {
                commandLine.Add("-fno-omit-frame-pointer");
            }
            switch (options.Optimization)
            {
                case C.EOptimization.Off:
                    commandLine.Add("-O0");
                    break;
                case C.EOptimization.Size:
                    commandLine.Add("-Os");
                    break;
                case C.EOptimization.Speed:
                    commandLine.Add("-O1");
                    break;
                case C.EOptimization.Full:
                    commandLine.Add("-O3");
                    break;
            }
            foreach (var define in options.PreprocessorDefines)
            {
                if (System.String.IsNullOrEmpty(define.Value))
                {
                    commandLine.Add(System.String.Format("-D{0}={1}"));
                }
                else
                {
                    commandLine.Add(System.String.Format("-D{0}={1}"));
                }
            }
            foreach (var undefine in options.PreprocessorUndefines)
            {
                commandLine.Add(System.String.Format("-U{0}", undefine));
            }
            foreach (var path in options.SystemIncludePaths)
            {
                commandLine.Add(System.String.Format("-isystem{0}", path));
            }
            switch (options.TargetLanguage)
            {
                case C.ETargetLanguage.C:
                    commandLine.Add("-x c");
                    break;
                case C.ETargetLanguage.Cxx:
                    commandLine.Add("-x c++");
                    break;
                default:
                    throw new Bam.Core.Exception("Unsupported target language");
            }
            if (options.WarningsAsErrors)
            {
                commandLine.Add("-Werror");
            }
            switch (options.OutputType)
            {
                case C.ECompilerOutput.CompileOnly:
                    commandLine.Add(System.String.Format("-c {0}", objectFile.InputPath.ToString()));
                    commandLine.Add(System.String.Format("-o {0}", module.GeneratedPaths[C.V2.ObjectFile.Key].ToString()));
                    break;
                case C.ECompilerOutput.Preprocess:
                    commandLine.Add(System.String.Format("-E {0}", objectFile.InputPath.ToString()));
                    commandLine.Add(System.String.Format("-o {0}", module.GeneratedPaths[C.V2.ObjectFile.Key].ToString()));
                    break;
            }
        }

        public static void Convert(this C.V2.ICOnlyCompilerOptions options, Bam.Core.V2.Module module)
        {
        }

        public static void Convert(this MingwCommon.V2.ICommonCompilerOptions options, Bam.Core.V2.Module module)
        {
        }

        public static void Convert(this MingwCommon.V2.ICOnlyCompilerOptions options, Bam.Core.V2.Module module)
        {
        }

        public static void Convert(this Mingw.V2.ICommonCompilerOptions options, Bam.Core.V2.Module module)
        {
        }

        public static void Convert(this Mingw.V2.ICOnlyCompilerOptions options, Bam.Core.V2.Module module)
        {
        }
    }

namespace V2
{
    public class CompilerSettings :
        Bam.Core.V2.Settings,
        C.V2.ICommonCompilerOptions,
        C.V2.ICOnlyCompilerOptions,
        MingwCommon.V2.ICommonCompilerOptions,
        MingwCommon.V2.ICOnlyCompilerOptions,
        Mingw.V2.ICommonCompilerOptions,
        Mingw.V2.ICOnlyCompilerOptions,
        CommandLineProcessor.V2.IConvertToCommandLine
    {
        public CompilerSettings()
        {
            (this as C.V2.ICommonCompilerOptions).Defaults();
        }


        C.V2.EBit C.V2.ICommonCompilerOptions.Bits
        {
            get;
            set;
        }

        C.V2.PreprocessorDefinitions C.V2.ICommonCompilerOptions.PreprocessorDefines
        {
            get;
            set;
        }

        Bam.Core.StringArray C.V2.ICommonCompilerOptions.IncludePaths
        {
            get;
            set;
        }

        Bam.Core.StringArray C.V2.ICommonCompilerOptions.SystemIncludePaths
        {
            get;
            set;
        }

        C.ECompilerOutput C.V2.ICommonCompilerOptions.OutputType
        {
            get;
            set;
        }

        bool C.V2.ICommonCompilerOptions.DebugSymbols
        {
            get;
            set;
        }

        bool C.V2.ICommonCompilerOptions.WarningsAsErrors
        {
            get;
            set;
        }

        C.EOptimization C.V2.ICommonCompilerOptions.Optimization
        {
            get;
            set;
        }

        C.ETargetLanguage C.V2.ICommonCompilerOptions.TargetLanguage
        {
            get;
            set;
        }

        C.ELanguageStandard C.V2.ICommonCompilerOptions.LanguageStandard
        {
            get;
            set;
        }

        bool C.V2.ICommonCompilerOptions.OmitFramePointer
        {
            get;
            set;
        }

        Bam.Core.StringArray C.V2.ICommonCompilerOptions.DisableWarnings
        {
            get;
            set;
        }

        Bam.Core.StringArray C.V2.ICommonCompilerOptions.PreprocessorUndefines
        {
            get;
            set;
        }

        bool C.V2.ICOnlyCompilerOptions.C99Specific
        {
            get;
            set;
        }

        bool MingwCommon.V2.ICommonCompilerOptions.MCommonCommon
        {
            get;
            set;
        }

        bool MingwCommon.V2.ICOnlyCompilerOptions.MCommonCOnly
        {
            get;
            set;
        }

        bool ICommonCompilerOptions.M48Common
        {
            get;
            set;
        }

        bool ICOnlyCompilerOptions.M48COnly
        {
            get;
            set;
        }

        void CommandLineProcessor.V2.IConvertToCommandLine.Convert(Bam.Core.V2.Module module)
        {
            (this as C.V2.ICommonCompilerOptions).Convert(module);
            (this as C.V2.ICOnlyCompilerOptions).Convert(module);
            (this as MingwCommon.V2.ICommonCompilerOptions).Convert(module);
            (this as MingwCommon.V2.ICOnlyCompilerOptions).Convert(module);
            (this as Mingw.V2.ICommonCompilerOptions).Convert(module);
            (this as Mingw.V2.ICOnlyCompilerOptions).Convert(module);
        }
    }

    public sealed class CxxCompilerSettings :
        Bam.Core.V2.Settings,
        C.V2.ICommonCompilerOptions,
        C.V2.ICxxOnlyCompilerOptions,
        MingwCommon.V2.ICommonCompilerOptions,
        MingwCommon.V2.ICxxOnlyCompilerOptions,
        Mingw.V2.ICommonCompilerOptions,
        Mingw.V2.ICxxOnlyCompilerOptions
    {
        C.V2.EBit C.V2.ICommonCompilerOptions.Bits
        {
            get;
            set;
        }

        C.V2.PreprocessorDefinitions C.V2.ICommonCompilerOptions.PreprocessorDefines
        {
            get;
            set;
        }

        Bam.Core.StringArray C.V2.ICommonCompilerOptions.IncludePaths
        {
            get;
            set;
        }

        Bam.Core.StringArray C.V2.ICommonCompilerOptions.SystemIncludePaths
        {
            get;
            set;
        }

        C.ECompilerOutput C.V2.ICommonCompilerOptions.OutputType
        {
            get;
            set;
        }

        bool C.V2.ICommonCompilerOptions.DebugSymbols
        {
            get;
            set;
        }

        bool C.V2.ICommonCompilerOptions.WarningsAsErrors
        {
            get;
            set;
        }

        C.EOptimization C.V2.ICommonCompilerOptions.Optimization
        {
            get;
            set;
        }

        C.ETargetLanguage C.V2.ICommonCompilerOptions.TargetLanguage
        {
            get;
            set;
        }

        C.ELanguageStandard C.V2.ICommonCompilerOptions.LanguageStandard
        {
            get;
            set;
        }

        bool C.V2.ICommonCompilerOptions.OmitFramePointer
        {
            get;
            set;
        }

        Bam.Core.StringArray C.V2.ICommonCompilerOptions.DisableWarnings
        {
            get;
            set;
        }

        Bam.Core.StringArray C.V2.ICommonCompilerOptions.PreprocessorUndefines
        {
            get;
            set;
        }

        C.Cxx.EExceptionHandler C.V2.ICxxOnlyCompilerOptions.ExceptionHandler
        {
            get;
            set;
        }

        bool MingwCommon.V2.ICommonCompilerOptions.MCommonCommon
        {
            get;
            set;
        }

        bool MingwCommon.V2.ICxxOnlyCompilerOptions.MCommonCxxOnly
        {
            get;
            set;
        }

        bool ICommonCompilerOptions.M48Common
        {
            get;
            set;
        }

        int ICxxOnlyCompilerOptions.M48CxxOnly
        {
            get;
            set;
        }
    }

    public abstract class CompilerBase :
        C.V2.CompilerTool
    {
        public CompilerBase()
        {
            this.InheritedEnvironmentVariables.Add("TEMP");
            this.EnvironmentVariables.Add("PATH", @"C:\MinGW\bin");
        }

        public override string Executable
        {
            get
            {
                return @"C:\MinGW\bin\mingw32-gcc-4.8.1.exe";
            }
        }

        public override Bam.Core.V2.Settings CreateDefaultSettings<T>(T module)
        {
            if (typeof(C.Cxx.V2.ObjectFile).IsInstanceOfType(module))
            {
                var settings = new CxxCompilerSettings();
                this.OverrideDefaultSettings(settings);
                return settings;
            }
            else if (typeof(C.V2.ObjectFile).IsInstanceOfType(module))
            {
                var settings = new CompilerSettings();
                this.OverrideDefaultSettings(settings);
                return settings;
            }
            else
            {
                throw new Bam.Core.Exception("Could not determine type of module {0}", typeof(T).ToString());
            }
        }

        protected abstract void OverrideDefaultSettings(Bam.Core.V2.Settings settings);
    }

    public sealed class Compiler32 :
        CompilerBase
    {
        protected override void OverrideDefaultSettings(Bam.Core.V2.Settings settings)
        {
            var cSettings = settings as C.V2.ICommonCompilerOptions;
            cSettings.Bits = C.V2.EBit.ThirtyTwo;
        }
    }

    /*
    public sealed class Compiler64 :
        CompilerBase
    {
        protected override void OverrideDefaultSettings(Bam.Core.V2.Settings settings)
        {
            var cSettings = settings as C.V2.ICommonCompilerOptions;
            cSettings.Bits = C.V2.EBit.SixtyFour;
        }
    }
     */
}
    public partial class CCompilerOptionCollection :
        MingwCommon.CCompilerOptionCollection,
        ICCompilerOptions
    {
        public
        CCompilerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}

        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);

            // requires gcc 4.0, and only works on ELFs, but doesn't seem to do any harm
            (this as ICCompilerOptions).Visibility = EVisibility.Hidden;
        }
    }
}
