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
using C.Cxx.V2.DefaultSettings;
namespace Mingw
{
    public static partial class NativeImplementation
    {
        public static void
        Convert(
            this C.V2.ICommonCompilerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
            var objectFile = module as C.V2.ObjectFile;
            if (options.Bits == C.V2.EBit.ThirtyTwo)
            {
                commandLine.Add("-m32");
            }
            else
            {
                commandLine.Add("-m64");
            }
            if (true == options.DebugSymbols)
            {
                commandLine.Add("-g");
            }
            foreach (var warning in options.DisableWarnings)
            {
                commandLine.Add(warning);
            }
            foreach (var path in options.IncludePaths)
            {
                var formatString = path.ContainsSpace ? "-I\"{0}\"" : "-I{0}";
                commandLine.Add(System.String.Format(formatString, path));
            }
            if (true == options.OmitFramePointer)
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
                    commandLine.Add(System.String.Format("-D{0}", define.Key));
                }
                else
                {
                    commandLine.Add(System.String.Format("-D{0}={1}", define.Key, define.Value));
                }
            }
            foreach (var undefine in options.PreprocessorUndefines)
            {
                commandLine.Add(System.String.Format("-U{0}", undefine));
            }
            foreach (var path in options.SystemIncludePaths)
            {
                var formatString = path.ContainsSpace ? "-I\"{0}\"" : "-I{0}";
                commandLine.Add(System.String.Format(formatString, path));
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
            if (true == options.WarningsAsErrors)
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

        public static void
        Convert(
            this C.V2.ICOnlyCompilerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
            switch (options.LanguageStandard)
            {
                case C.ECLanguageStandard.C89:
                    break;
                case C.ECLanguageStandard.C99:
                    commandLine.Add("-std=c99");
                    break;
                default:
                    throw new Bam.Core.Exception("Invalid C language standard {0}", options.LanguageStandard.ToString());
            }
        }

        public static void
        Convert(
            this C.V2.ICxxOnlyCompilerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
            if (null != options.ExceptionHandler)
            {
                switch (options.ExceptionHandler)
                {
                    case C.Cxx.EExceptionHandler.Disabled:
                        commandLine.Add("-fno-exceptions");
                        break;

                    case C.Cxx.EExceptionHandler.Asynchronous:
                    case C.Cxx.EExceptionHandler.Synchronous:
                        commandLine.Add("-fexceptions");
                        break;

                    default:
                        throw new Bam.Core.Exception("Unrecognized exception handler option");
                }
            }
            switch (options.LanguageStandard)
            {
                case C.Cxx.ELanguageStandard.Cxx98:
                    commandLine.Add("-std=c++98");
                    break;
                case C.Cxx.ELanguageStandard.Cxx11:
                    commandLine.Add("-std=c++11");
                    break;
                default:
                    throw new Bam.Core.Exception("Invalid C++ language standard {0}", options.LanguageStandard.ToString());
            }
        }

        public static void
        Convert(
            this MingwCommon.V2.ICommonCompilerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
        }

        public static void
        Convert(
            this MingwCommon.V2.ICOnlyCompilerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
        }

        public static void
        Convert(
            this MingwCommon.V2.ICxxOnlyCompilerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
        }

        public static void
        Convert(
            this Mingw.V2.ICommonCompilerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
        }

        public static void
        Convert(
            this Mingw.V2.ICOnlyCompilerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
        }

        public static void
        Convert(
            this Mingw.V2.ICxxOnlyCompilerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
        }
    }

namespace V2
{
    public class CompilerSettings :
        C.V2.SettingsBase,
        CommandLineProcessor.V2.IConvertToCommandLine,
        C.V2.ICommonCompilerOptions,
        C.V2.ICOnlyCompilerOptions,
        MingwCommon.V2.ICommonCompilerOptions,
        MingwCommon.V2.ICOnlyCompilerOptions,
        Mingw.V2.ICommonCompilerOptions,
        Mingw.V2.ICOnlyCompilerOptions
    {
        public CompilerSettings(Bam.Core.V2.Module module)
            : this(module, useDefaults:true)
        {
        }

        public CompilerSettings(Bam.Core.V2.Module module, bool useDefaults)
        {
#if true
            this.InitializeAllInterfaces(module, true, useDefaults);
#else
            (this as C.V2.ICommonCompilerOptions).Empty();
            if (useDefaults)
            {
                (this as C.V2.ICommonCompilerOptions).Defaults(module);
            }
#endif
        }

        void
        CommandLineProcessor.V2.IConvertToCommandLine.Convert(
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
            (this as C.V2.ICommonCompilerOptions).Convert(module, commandLine);
            (this as C.V2.ICOnlyCompilerOptions).Convert(module, commandLine);
            (this as MingwCommon.V2.ICommonCompilerOptions).Convert(module, commandLine);
            (this as MingwCommon.V2.ICOnlyCompilerOptions).Convert(module, commandLine);
            (this as Mingw.V2.ICommonCompilerOptions).Convert(module, commandLine);
            (this as Mingw.V2.ICOnlyCompilerOptions).Convert(module, commandLine);
        }

        C.V2.EBit? C.V2.ICommonCompilerOptions.Bits
        {
            get;
            set;
        }

        C.V2.PreprocessorDefinitions C.V2.ICommonCompilerOptions.PreprocessorDefines
        {
            get;
            set;
        }

        Bam.Core.Array<Bam.Core.V2.TokenizedString> C.V2.ICommonCompilerOptions.IncludePaths
        {
            get;
            set;
        }

        Bam.Core.Array<Bam.Core.V2.TokenizedString> C.V2.ICommonCompilerOptions.SystemIncludePaths
        {
            get;
            set;
        }

        C.ECompilerOutput? C.V2.ICommonCompilerOptions.OutputType
        {
            get;
            set;
        }

        bool? C.V2.ICommonCompilerOptions.DebugSymbols
        {
            get;
            set;
        }

        bool? C.V2.ICommonCompilerOptions.WarningsAsErrors
        {
            get;
            set;
        }

        C.EOptimization? C.V2.ICommonCompilerOptions.Optimization
        {
            get;
            set;
        }

        C.ETargetLanguage? C.V2.ICommonCompilerOptions.TargetLanguage
        {
            get;
            set;
        }

        bool? C.V2.ICommonCompilerOptions.OmitFramePointer
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

        C.ECLanguageStandard? C.V2.ICOnlyCompilerOptions.LanguageStandard
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
    }

    public sealed class CxxCompilerSettings :
        C.V2.SettingsBase,
        CommandLineProcessor.V2.IConvertToCommandLine,
        C.V2.ICommonCompilerOptions,
        C.V2.ICxxOnlyCompilerOptions,
        MingwCommon.V2.ICommonCompilerOptions,
        MingwCommon.V2.ICxxOnlyCompilerOptions,
        Mingw.V2.ICommonCompilerOptions,
        Mingw.V2.ICxxOnlyCompilerOptions
    {
        public CxxCompilerSettings(Bam.Core.V2.Module module)
            : this(module, useDefaults:true)
        {
        }

        public CxxCompilerSettings(Bam.Core.V2.Module module, bool useDefaults)
        {
#if true
            this.InitializeAllInterfaces(module, true, useDefaults);
#else
            (this as C.V2.ICommonCompilerOptions).Empty();
            (this as C.V2.ICxxOnlyCompilerOptions).Empty();
            if (useDefaults)
            {
                (this as C.V2.ICommonCompilerOptions).Defaults(module);
                (this as C.V2.ICxxOnlyCompilerOptions).Defaults(module);
            }
#endif
        }

        void
        CommandLineProcessor.V2.IConvertToCommandLine.Convert(
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
            (this as C.V2.ICommonCompilerOptions).Convert(module, commandLine);
            (this as C.V2.ICxxOnlyCompilerOptions).Convert(module, commandLine);
            (this as MingwCommon.V2.ICommonCompilerOptions).Convert(module, commandLine);
            (this as MingwCommon.V2.ICxxOnlyCompilerOptions).Convert(module, commandLine);
            (this as Mingw.V2.ICommonCompilerOptions).Convert(module, commandLine);
            (this as Mingw.V2.ICxxOnlyCompilerOptions).Convert(module, commandLine);
        }

        C.V2.EBit? C.V2.ICommonCompilerOptions.Bits
        {
            get;
            set;
        }

        C.V2.PreprocessorDefinitions C.V2.ICommonCompilerOptions.PreprocessorDefines
        {
            get;
            set;
        }

        Bam.Core.Array<Bam.Core.V2.TokenizedString> C.V2.ICommonCompilerOptions.IncludePaths
        {
            get;
            set;
        }

        Bam.Core.Array<Bam.Core.V2.TokenizedString> C.V2.ICommonCompilerOptions.SystemIncludePaths
        {
            get;
            set;
        }

        C.ECompilerOutput? C.V2.ICommonCompilerOptions.OutputType
        {
            get;
            set;
        }

        bool? C.V2.ICommonCompilerOptions.DebugSymbols
        {
            get;
            set;
        }

        bool? C.V2.ICommonCompilerOptions.WarningsAsErrors
        {
            get;
            set;
        }

        C.EOptimization? C.V2.ICommonCompilerOptions.Optimization
        {
            get;
            set;
        }

        C.ETargetLanguage? C.V2.ICommonCompilerOptions.TargetLanguage
        {
            get;
            set;
        }

        bool? C.V2.ICommonCompilerOptions.OmitFramePointer
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

        C.Cxx.EExceptionHandler? C.V2.ICxxOnlyCompilerOptions.ExceptionHandler
        {
            get;
            set;
        }

        C.Cxx.ELanguageStandard? C.V2.ICxxOnlyCompilerOptions.LanguageStandard
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
    }

    public static class Configure
    {
        static Configure()
        {
            InstallPath = Bam.Core.V2.TokenizedString.Create(@"C:\MinGW", null);
        }

        public static Bam.Core.V2.TokenizedString InstallPath
        {
            get;
            private set;
        }
    }

    public abstract class CompilerBase :
        C.V2.CompilerTool
    {
        public CompilerBase()
        {
            this.InheritedEnvironmentVariables.Add("TEMP");

            this.Macros.Add("InstallPath", Configure.InstallPath);
            this.Macros.Add("BinPath", Bam.Core.V2.TokenizedString.Create(@"$(InstallPath)\bin", this));
            this.Macros.Add("CompilerPath", Bam.Core.V2.TokenizedString.Create(@"$(BinPath)\mingw32-gcc-4.8.1.exe", this));
            this.Macros.Add("objext", ".o");

            this.EnvironmentVariables.Add("PATH", new Bam.Core.V2.TokenizedStringArray(this.Macros["BinPath"]));
        }

        public override Bam.Core.V2.TokenizedString Executable
        {
            get
            {
                return this.Macros["CompilerPath"];
            }
        }

        public override Bam.Core.V2.Settings CreateDefaultSettings<T>(T module)
        {
            if (typeof(C.Cxx.V2.ObjectFile).IsInstanceOfType(module) ||
                typeof(C.Cxx.V2.ObjectFileCollection).IsInstanceOfType(module))
            {
                var settings = new CxxCompilerSettings(module);
                this.OverrideDefaultSettings(settings);
                return settings;
            }
            else if (typeof(C.V2.ObjectFile).IsInstanceOfType(module) ||
                     typeof(C.V2.CObjectFileCollection).IsInstanceOfType(module))
            {
                var settings = new CompilerSettings(module);
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

    [C.V2.RegisterCCompiler("Mingw", Bam.Core.EPlatform.Windows, C.V2.EBit.ThirtyTwo)]
    public class Compiler32 :
        CompilerBase
    {
        protected override void OverrideDefaultSettings(Bam.Core.V2.Settings settings)
        {
            var cSettings = settings as C.V2.ICommonCompilerOptions;
            cSettings.Bits = C.V2.EBit.ThirtyTwo;
        }
    }

    [C.V2.RegisterCxxCompiler("Mingw", Bam.Core.EPlatform.Windows, C.V2.EBit.ThirtyTwo)]
    public sealed class Compiler32Cxx :
        Compiler32
    {
        public Compiler32Cxx()
        {
            this.Macros.Add("CompilerPath", Bam.Core.V2.TokenizedString.Create(@"$(BinPath)\mingw32-g++.exe", this));
        }

        protected override void OverrideDefaultSettings(Bam.Core.V2.Settings settings)
        {
            base.OverrideDefaultSettings(settings);
            var cSettings = settings as C.V2.ICommonCompilerOptions;
            cSettings.TargetLanguage = C.ETargetLanguage.Cxx;
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
