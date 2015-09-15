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
using C.DefaultSettings;
using C.Cxx.DefaultSettings;
namespace Mingw
{
    public static partial class NativeImplementation
    {
        public static void
        Convert(
            this C.ICommonCompilerOptions options,
            Bam.Core.Module module,
            Bam.Core.StringArray commandLine)
        {
            var objectFile = module as C.ObjectFile;
            if (options.Bits == C.EBit.ThirtyTwo)
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
                    commandLine.Add(System.String.Format("-o {0}", module.GeneratedPaths[C.ObjectFile.Key].ToString()));
                    break;
                case C.ECompilerOutput.Preprocess:
                    commandLine.Add(System.String.Format("-E {0}", objectFile.InputPath.ToString()));
                    commandLine.Add(System.String.Format("-o {0}", module.GeneratedPaths[C.ObjectFile.Key].ToString()));
                    break;
            }
        }

        public static void
        Convert(
            this C.ICOnlyCompilerOptions options,
            Bam.Core.Module module,
            Bam.Core.StringArray commandLine)
        {
            if (null != options.LanguageStandard)
            {
                switch (options.LanguageStandard)
                {
                    case C.ELanguageStandard.C89:
                        break;
                    case C.ELanguageStandard.C99:
                        commandLine.Add("-std=c99");
                        break;
                    default:
                        throw new Bam.Core.Exception("Invalid C language standard {0}", options.LanguageStandard.ToString());
                }
            }
        }

        public static void
        Convert(
            this C.ICxxOnlyCompilerOptions options,
            Bam.Core.Module module,
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
            this C.ICCompilerOptionsWin options,
            Bam.Core.Module module,
            Bam.Core.StringArray commandLine)
        {
            if (options.CharacterSet.HasValue)
            {
                switch (options.CharacterSet.Value)
                {
                    case C.ECharacterSet.NotSet:
                        break;

                    case C.ECharacterSet.Unicode:
                        {
                            var compiler = options as C.ICommonCompilerOptions;
                            compiler.PreprocessorDefines.Add("_UNICODE");
                        }
                        break;

                    case C.ECharacterSet.MultiByte:
                        {
                            var compiler = options as C.ICommonCompilerOptions;
                            compiler.PreprocessorDefines.Add("_MBCS");
                        }
                        break;
                }
            }
        }

        public static void
        Convert(
            this MingwCommon.ICommonCompilerOptions options,
            Bam.Core.Module module,
            Bam.Core.StringArray commandLine)
        {
        }

        public static void
        Convert(
            this MingwCommon.ICOnlyCompilerOptions options,
            Bam.Core.Module module,
            Bam.Core.StringArray commandLine)
        {
        }

        public static void
        Convert(
            this MingwCommon.ICxxOnlyCompilerOptions options,
            Bam.Core.Module module,
            Bam.Core.StringArray commandLine)
        {
        }

        public static void
        Convert(
            this Mingw.ICommonCompilerOptions options,
            Bam.Core.Module module,
            Bam.Core.StringArray commandLine)
        {
        }

        public static void
        Convert(
            this Mingw.ICOnlyCompilerOptions options,
            Bam.Core.Module module,
            Bam.Core.StringArray commandLine)
        {
        }

        public static void
        Convert(
            this Mingw.ICxxOnlyCompilerOptions options,
            Bam.Core.Module module,
            Bam.Core.StringArray commandLine)
        {
        }
    }

    public class CompilerSettings :
        C.SettingsBase,
        CommandLineProcessor.IConvertToCommandLine,
        C.ICCompilerOptionsWin,
        C.ICommonCompilerOptions,
        C.ICOnlyCompilerOptions,
        MingwCommon.ICommonCompilerOptions,
        MingwCommon.ICOnlyCompilerOptions,
        Mingw.ICommonCompilerOptions,
        Mingw.ICOnlyCompilerOptions
    {
        public CompilerSettings(Bam.Core.Module module)
            : this(module, useDefaults:true)
        {
        }

        public CompilerSettings(Bam.Core.Module module, bool useDefaults)
        {
#if true
            this.InitializeAllInterfaces(module, true, useDefaults);
#else
            (this as C.ICommonCompilerOptions).Empty();
            if (useDefaults)
            {
                (this as C.ICommonCompilerOptions).Defaults(module);
            }
#endif
        }

        void
        CommandLineProcessor.IConvertToCommandLine.Convert(
            Bam.Core.Module module,
            Bam.Core.StringArray commandLine)
        {
            (this as C.ICCompilerOptionsWin).Convert(module, commandLine);
            (this as C.ICommonCompilerOptions).Convert(module, commandLine);
            (this as C.ICOnlyCompilerOptions).Convert(module, commandLine);
            (this as MingwCommon.ICommonCompilerOptions).Convert(module, commandLine);
            (this as MingwCommon.ICOnlyCompilerOptions).Convert(module, commandLine);
            (this as Mingw.ICommonCompilerOptions).Convert(module, commandLine);
            (this as Mingw.ICOnlyCompilerOptions).Convert(module, commandLine);
        }

        C.ECharacterSet? C.ICCompilerOptionsWin.CharacterSet
        {
            get;
            set;
        }

        C.EBit? C.ICommonCompilerOptions.Bits
        {
            get;
            set;
        }

        C.PreprocessorDefinitions C.ICommonCompilerOptions.PreprocessorDefines
        {
            get;
            set;
        }

        Bam.Core.Array<Bam.Core.TokenizedString> C.ICommonCompilerOptions.IncludePaths
        {
            get;
            set;
        }

        Bam.Core.Array<Bam.Core.TokenizedString> C.ICommonCompilerOptions.SystemIncludePaths
        {
            get;
            set;
        }

        C.ECompilerOutput? C.ICommonCompilerOptions.OutputType
        {
            get;
            set;
        }

        bool? C.ICommonCompilerOptions.DebugSymbols
        {
            get;
            set;
        }

        bool? C.ICommonCompilerOptions.WarningsAsErrors
        {
            get;
            set;
        }

        C.EOptimization? C.ICommonCompilerOptions.Optimization
        {
            get;
            set;
        }

        C.ETargetLanguage? C.ICommonCompilerOptions.TargetLanguage
        {
            get;
            set;
        }

        bool? C.ICommonCompilerOptions.OmitFramePointer
        {
            get;
            set;
        }

        Bam.Core.StringArray C.ICommonCompilerOptions.DisableWarnings
        {
            get;
            set;
        }

        Bam.Core.StringArray C.ICommonCompilerOptions.PreprocessorUndefines
        {
            get;
            set;
        }

        C.ELanguageStandard? C.ICOnlyCompilerOptions.LanguageStandard
        {
            get;
            set;
        }

        bool MingwCommon.ICommonCompilerOptions.MCommonCommon
        {
            get;
            set;
        }

        bool MingwCommon.ICOnlyCompilerOptions.MCommonCOnly
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
        C.SettingsBase,
        CommandLineProcessor.IConvertToCommandLine,
        C.ICCompilerOptionsWin,
        C.ICommonCompilerOptions,
        C.ICxxOnlyCompilerOptions,
        MingwCommon.ICommonCompilerOptions,
        MingwCommon.ICxxOnlyCompilerOptions,
        Mingw.ICommonCompilerOptions,
        Mingw.ICxxOnlyCompilerOptions
    {
        public CxxCompilerSettings(Bam.Core.Module module)
            : this(module, useDefaults:true)
        {
        }

        public CxxCompilerSettings(Bam.Core.Module module, bool useDefaults)
        {
#if true
            this.InitializeAllInterfaces(module, true, useDefaults);
#else
            (this as C.ICommonCompilerOptions).Empty();
            (this as C.ICxxOnlyCompilerOptions).Empty();
            if (useDefaults)
            {
                (this as C.ICommonCompilerOptions).Defaults(module);
                (this as C.ICxxOnlyCompilerOptions).Defaults(module);
            }
#endif
        }

        void
        CommandLineProcessor.IConvertToCommandLine.Convert(
            Bam.Core.Module module,
            Bam.Core.StringArray commandLine)
        {
            (this as C.ICCompilerOptionsWin).Convert(module, commandLine);
            (this as C.ICommonCompilerOptions).Convert(module, commandLine);
            (this as C.ICxxOnlyCompilerOptions).Convert(module, commandLine);
            (this as MingwCommon.ICommonCompilerOptions).Convert(module, commandLine);
            (this as MingwCommon.ICxxOnlyCompilerOptions).Convert(module, commandLine);
            (this as Mingw.ICommonCompilerOptions).Convert(module, commandLine);
            (this as Mingw.ICxxOnlyCompilerOptions).Convert(module, commandLine);
        }

        C.ECharacterSet? C.ICCompilerOptionsWin.CharacterSet
        {
            get;
            set;
        }

        C.EBit? C.ICommonCompilerOptions.Bits
        {
            get;
            set;
        }

        C.PreprocessorDefinitions C.ICommonCompilerOptions.PreprocessorDefines
        {
            get;
            set;
        }

        Bam.Core.Array<Bam.Core.TokenizedString> C.ICommonCompilerOptions.IncludePaths
        {
            get;
            set;
        }

        Bam.Core.Array<Bam.Core.TokenizedString> C.ICommonCompilerOptions.SystemIncludePaths
        {
            get;
            set;
        }

        C.ECompilerOutput? C.ICommonCompilerOptions.OutputType
        {
            get;
            set;
        }

        bool? C.ICommonCompilerOptions.DebugSymbols
        {
            get;
            set;
        }

        bool? C.ICommonCompilerOptions.WarningsAsErrors
        {
            get;
            set;
        }

        C.EOptimization? C.ICommonCompilerOptions.Optimization
        {
            get;
            set;
        }

        C.ETargetLanguage? C.ICommonCompilerOptions.TargetLanguage
        {
            get;
            set;
        }

        bool? C.ICommonCompilerOptions.OmitFramePointer
        {
            get;
            set;
        }

        Bam.Core.StringArray C.ICommonCompilerOptions.DisableWarnings
        {
            get;
            set;
        }

        Bam.Core.StringArray C.ICommonCompilerOptions.PreprocessorUndefines
        {
            get;
            set;
        }

        C.Cxx.EExceptionHandler? C.ICxxOnlyCompilerOptions.ExceptionHandler
        {
            get;
            set;
        }

        C.Cxx.ELanguageStandard? C.ICxxOnlyCompilerOptions.LanguageStandard
        {
            get;
            set;
        }

        C.Cxx.EStandardLibrary? C.ICxxOnlyCompilerOptions.StandardLibrary
        {
            get;
            set;
        }

        bool MingwCommon.ICommonCompilerOptions.MCommonCommon
        {
            get;
            set;
        }

        bool MingwCommon.ICxxOnlyCompilerOptions.MCommonCxxOnly
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
            InstallPath = Bam.Core.TokenizedString.Create(@"C:\MinGW", null);
        }

        public static Bam.Core.TokenizedString InstallPath
        {
            get;
            private set;
        }
    }

    public abstract class CompilerBase :
        C.CompilerTool
    {
        public CompilerBase()
        {
            this.InheritedEnvironmentVariables.Add("TEMP");

            this.Macros.Add("InstallPath", Configure.InstallPath);
            this.Macros.Add("BinPath", Bam.Core.TokenizedString.Create(@"$(InstallPath)\bin", this));
            this.Macros.Add("CompilerPath", Bam.Core.TokenizedString.Create(@"$(BinPath)\mingw32-gcc-4.8.1.exe", this));
            this.Macros.Add("objext", ".o");

            this.EnvironmentVariables.Add("PATH", new Bam.Core.TokenizedStringArray(this.Macros["BinPath"]));
        }

        public override Bam.Core.TokenizedString Executable
        {
            get
            {
                return this.Macros["CompilerPath"];
            }
        }

        public override Bam.Core.Settings CreateDefaultSettings<T>(T module)
        {
            if (typeof(C.Cxx.ObjectFile).IsInstanceOfType(module) ||
                typeof(C.Cxx.ObjectFileCollection).IsInstanceOfType(module))
            {
                var settings = new CxxCompilerSettings(module);
                this.OverrideDefaultSettings(settings);
                return settings;
            }
            else if (typeof(C.ObjectFile).IsInstanceOfType(module) ||
                     typeof(C.CObjectFileCollection).IsInstanceOfType(module))
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

        protected abstract void OverrideDefaultSettings(Bam.Core.Settings settings);
    }

    [C.RegisterCCompiler("Mingw", Bam.Core.EPlatform.Windows, C.EBit.ThirtyTwo)]
    public class Compiler32 :
        CompilerBase
    {
        protected override void OverrideDefaultSettings(Bam.Core.Settings settings)
        {
            var cSettings = settings as C.ICommonCompilerOptions;
            cSettings.Bits = C.EBit.ThirtyTwo;
        }
    }

    [C.RegisterCxxCompiler("Mingw", Bam.Core.EPlatform.Windows, C.EBit.ThirtyTwo)]
    public sealed class Compiler32Cxx :
        Compiler32
    {
        public Compiler32Cxx()
        {
            this.Macros.Add("CompilerPath", Bam.Core.TokenizedString.Create(@"$(BinPath)\mingw32-g++.exe", this));
        }

        protected override void OverrideDefaultSettings(Bam.Core.Settings settings)
        {
            base.OverrideDefaultSettings(settings);
            var cSettings = settings as C.ICommonCompilerOptions;
            cSettings.TargetLanguage = C.ETargetLanguage.Cxx;
        }
    }

    /*
    public sealed class Compiler64 :
        CompilerBase
    {
        protected override void OverrideDefaultSettings(Bam.Core.Settings settings)
        {
            var cSettings = settings as C.ICommonCompilerOptions;
            cSettings.Bits = C.EBit.SixtyFour;
        }
    }
     */
}
