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
using C.V2.DefaultSettings;
using C.Cxx.V2.DefaultSettings;
using C.ObjC.V2.DefaultSettings;
using GccCommon.V2.DefaultSettings;
using Gcc.V2.DefaultSettings;
using GccCommon.V2; // TODO: for the native implementation
namespace Gcc
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
            if (null != options.Bits)
            {
                if (options.Bits == C.V2.EBit.SixtyFour)
                {
                    commandLine.Add("-m64");
                }
                else
                {
                    commandLine.Add("-m32");
                }
            }
            if (null != options.DebugSymbols)
            {
                if (true == options.DebugSymbols)
                {
                    commandLine.Add("-g");
                }
            }
            foreach (var warning in options.DisableWarnings)
            {
                commandLine.Add(System.String.Format("-Wno-{0}", warning));
            }
            foreach (var path in options.IncludePaths)
            {
                var formatString = path.ContainsSpace ? "-I\"{0}\"" : "-I{0}";
                commandLine.Add(System.String.Format(formatString, path));
            }
            if (null != options.OmitFramePointer)
            {
                if (true == options.OmitFramePointer)
                {
                    commandLine.Add("-fomit-frame-pointer");
                }
                else
                {
                    commandLine.Add("-fno-omit-frame-pointer");
                }
            }
            if (null != options.Optimization)
            {
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
            if (null != options.TargetLanguage)
            {
                switch (options.TargetLanguage)
                {
                    case C.ETargetLanguage.C:
                        commandLine.Add("-x c");
                        break;
                    case C.ETargetLanguage.Cxx:
                        commandLine.Add("-x c++");
                        break;
                    case C.ETargetLanguage.ObjectiveC:
                        commandLine.Add("-x objective-c");
                        break;
                    case C.ETargetLanguage.ObjectiveCxx:
                        commandLine.Add("-x objective-c++");
                        break;
                    default:
                        throw new Bam.Core.Exception("Unsupported target language");
                }
            }
            if (null != options.WarningsAsErrors)
            {
                if (true == options.WarningsAsErrors)
                {
                    commandLine.Add("-Werror");
                }
            }
            if (null != options.OutputType)
            {
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
        }

        public static void
        Convert(
            this C.V2.ICOnlyCompilerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
            if (null != options.LanguageStandard)
            {
                switch (options.LanguageStandard)
                {
                    case C.ECLanguageStandard.C89:
                        commandLine.Add("-std=c89");
                        break;

                    case C.ECLanguageStandard.C99:
                        commandLine.Add("-std=c99");
                        break;

                    default:
                        throw new Bam.Core.Exception("Invalid C language standard, '{0}'", options.LanguageStandard.ToString());
                }
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
            if (null != options.LanguageStandard)
            {
                switch (options.LanguageStandard)
                {
                    case C.Cxx.ELanguageStandard.Cxx98:
                        commandLine.Add("-std=c++98");
                        break;

                    case C.Cxx.ELanguageStandard.GnuCxx98:
                        commandLine.Add("-std=gnu++98");
                        break;

                    case C.Cxx.ELanguageStandard.Cxx11:
                        commandLine.Add("-std=c++11");
                        break;

                    default:
                        throw new Bam.Core.Exception("Invalid C++ language standard, '{0}'", options.LanguageStandard.ToString());
                }
            }
        }

        public static void
        Convert(
            this C.V2.IObjectiveCOnlyCompilerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
            if (options.ConstantStringClass != null)
            {
                commandLine.Add(System.String.Format("-fconstant-string-class={0}", options.ConstantStringClass));
            }
        }
    }

    public static partial class NativeImplementation
    {
        public static void
        Convert(
            this C.V2.ICommonArchiverOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
            switch (options.OutputType)
            {
                case C.EArchiverOutput.StaticLibrary:
                    commandLine.Add(module.GeneratedPaths[C.V2.StaticLibrary.Key].ToString());
                    break;
            }
        }

        public static void
        Convert(
            this V2.IArchiverOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
            if (options.Ranlib)
            {
                commandLine.Add("-s");
            }
            if (options.DoNotWarnIfLibraryCreated)
            {
                commandLine.Add("-c");
            }
            switch (options.Command)
            {
                case V2.EArchiverCommand.Replace:
                    commandLine.Add("-r");
                    break;

                default:
                    throw new Bam.Core.Exception("No such archiver command");
            }
        }
    }

    public static partial class NativeImplementation
    {
        public static void
        Convert(
            this C.V2.ICommonLinkerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
            //var applicationFile = module as C.V2.ConsoleApplication;
            switch (options.OutputType)
            {
                case C.ELinkerOutput.Executable:
                    commandLine.Add(System.String.Format("-o {0}", module.GeneratedPaths[C.V2.ConsoleApplication.Key].ToString()));
                    break;

                case C.ELinkerOutput.DynamicLibrary:
                    commandLine.Add("-shared");
                    commandLine.Add(System.String.Format("-o {0}", module.GeneratedPaths[C.V2.ConsoleApplication.Key].ToString()));
                    break;
            }
            foreach (var path in options.LibraryPaths)
            {
                var format = path.ContainsSpace ? "-L\"{0}\"" : "-L{0}";
                commandLine.Add(System.String.Format(format, path.ToString()));
            }
            foreach (var path in options.Libraries)
            {
                commandLine.Add(path);
            }
            if (options.DebugSymbols.GetValueOrDefault())
            {
                commandLine.Add("-g");
            }
        }
    }

namespace V2
{
    namespace DefaultSettings
    {
        public static partial class DefaultSettingsExtensions
        {
            public static void Defaults(this IArchiverOptions settings, Bam.Core.V2.Module module)
            {
                settings.Ranlib = true;
                settings.DoNotWarnIfLibraryCreated = true;
                settings.Command = EArchiverCommand.Replace;
            }
        }
    }

    public enum EArchiverCommand
    {
        Replace
    }

    [Bam.Core.V2.SettingsExtensions(typeof(Gcc.V2.DefaultSettings.DefaultSettingsExtensions))]
    public interface IArchiverOptions : Bam.Core.V2.ISettingsBase
    {
        bool Ranlib
        {
            get;
            set;
        }

        bool DoNotWarnIfLibraryCreated
        {
            get;
            set;
        }

        EArchiverCommand Command
        {
            get;
            set;
        }
    }

    public class CompilerSettings :
        C.V2.SettingsBase,
        CommandLineProcessor.V2.IConvertToCommandLine,
        C.V2.ICommonCompilerOptions,
        C.V2.ICOnlyCompilerOptions,
        GccCommon.V2.ICommonCompilerOptions
    {
        public CompilerSettings(Bam.Core.V2.Module module)
            : this(module, true)
        {
        }

        public CompilerSettings(Bam.Core.V2.Module module, bool useDefaults)
        {
#if true
            this.InitializeAllInterfaces(module, true, useDefaults);
#else
            var stdCommonCompilerOptions = this as C.V2.ICommonCompilerOptions;
            stdCommonCompilerOptions.Empty();
            if (useDefaults)
            {
                stdCommonCompilerOptions.Defaults(module);
            }

            var commonCompilerOptions = this as GccCommon.V2.ICommonCompilerOptions;
            commonCompilerOptions.Empty();
            if (useDefaults)
            {
                commonCompilerOptions.Defaults(module);
            }
#endif
        }

        void CommandLineProcessor.V2.IConvertToCommandLine.Convert(Bam.Core.V2.Module module, Bam.Core.StringArray commandLine)
        {
            (this as C.V2.ICommonCompilerOptions).Convert(module, commandLine);
            (this as GccCommon.V2.ICommonCompilerOptions).Convert(module, commandLine);
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

        bool? GccCommon.V2.ICommonCompilerOptions.PositionIndependentCode
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
        GccCommon.V2.ICommonCompilerOptions
    {
        public CxxCompilerSettings(Bam.Core.V2.Module module)
            : this(module, true)
        {
        }

        public CxxCompilerSettings(Bam.Core.V2.Module module, bool useDefaults)
        {
#if true
            this.InitializeAllInterfaces(module, true, useDefaults);
#else
            var stdCommonCompilerOptions = this as C.V2.ICommonCompilerOptions;
            stdCommonCompilerOptions.Empty();
            if (useDefaults)
            {
                stdCommonCompilerOptions.Defaults(module);
            }

            var stdCommonCxxCompilerSettings = this as C.V2.ICxxOnlyCompilerOptions;
            stdCommonCxxCompilerSettings.Empty();
            if (useDefaults)
            {
                stdCommonCxxCompilerSettings.Defaults(module);
            }

            var commonCompilerOptions = this as GccCommon.V2.ICommonCompilerOptions;
            commonCompilerOptions.Empty();
            if (useDefaults)
            {
                commonCompilerOptions.Defaults(module);
            }
#endif
        }

        void CommandLineProcessor.V2.IConvertToCommandLine.Convert(Bam.Core.V2.Module module, Bam.Core.StringArray commandLine)
        {
            (this as C.V2.ICommonCompilerOptions).Convert(module, commandLine);
            (this as C.V2.ICxxOnlyCompilerOptions).Convert(module, commandLine);
            (this as GccCommon.V2.ICommonCompilerOptions).Convert(module, commandLine);
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

        C.Cxx.EStandardLibrary? C.V2.ICxxOnlyCompilerOptions.StandardLibrary
        {
            get;
            set;
        }

        bool? GccCommon.V2.ICommonCompilerOptions.PositionIndependentCode
        {
            get;
            set;
        }
    }

    public class ObjectiveCCompilerSettings :
        C.V2.SettingsBase,
        CommandLineProcessor.V2.IConvertToCommandLine,
        C.V2.ICommonCompilerOptions,
        C.V2.ICOnlyCompilerOptions,
        C.V2.IObjectiveCOnlyCompilerOptions,
        GccCommon.V2.ICommonCompilerOptions
    {
        public ObjectiveCCompilerSettings(Bam.Core.V2.Module module)
            : this(module, true)
        {
        }

        public ObjectiveCCompilerSettings(Bam.Core.V2.Module module, bool useDefaults)
        {
#if true
            this.InitializeAllInterfaces(module, true, useDefaults);
#else
            var stdCommonCompilerOptions = this as C.V2.ICommonCompilerOptions;
            stdCommonCompilerOptions.Empty();
            if (useDefaults)
            {
                stdCommonCompilerOptions.Defaults(module);
            }

            var objCCompilerOptions = this as C.V2.IObjectiveCOnlyCompilerOptions;
            objCCompilerOptions.Empty();
            if (useDefaults)
            {
               objCCompilerOptions.Defaults(module);
            }

            var commonCompilerOptions = this as GccCommon.V2.ICommonCompilerOptions;
            commonCompilerOptions.Empty();
            if (useDefaults)
            {
                commonCompilerOptions.Defaults(module);
            }
#endif
        }

        void CommandLineProcessor.V2.IConvertToCommandLine.Convert(Bam.Core.V2.Module module, Bam.Core.StringArray commandLine)
        {
            (this as C.V2.ICommonCompilerOptions).Convert(module, commandLine);
            (this as C.V2.IObjectiveCOnlyCompilerOptions).Convert(module, commandLine);
            (this as GccCommon.V2.ICommonCompilerOptions).Convert(module, commandLine);
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

        string C.V2.IObjectiveCOnlyCompilerOptions.ConstantStringClass
        {
            get;
            set;
        }

        bool? GccCommon.V2.ICommonCompilerOptions.PositionIndependentCode
        {
            get;
            set;
        }
    }

    public sealed class ObjectiveCxxCompilerSettings :
        C.V2.SettingsBase,
        CommandLineProcessor.V2.IConvertToCommandLine,
        C.V2.ICommonCompilerOptions,
        C.V2.ICxxOnlyCompilerOptions,
        C.V2.IObjectiveCxxOnlyCompilerOptions,
        GccCommon.V2.ICommonCompilerOptions
    {
        public ObjectiveCxxCompilerSettings(Bam.Core.V2.Module module)
            : this(module, true)
        {
        }

        public ObjectiveCxxCompilerSettings(Bam.Core.V2.Module module, bool useDefaults)
        {
#if true
            this.InitializeAllInterfaces(module, true, useDefaults);
#else
            var stdCommonCompilerOptions = this as C.V2.ICommonCompilerOptions;
            stdCommonCompilerOptions.Empty();
            if (useDefaults)
            {
                stdCommonCompilerOptions.Defaults(module);
            }

            var stdCommonCxxCompilerSettings = this as C.V2.ICxxOnlyCompilerOptions;
            stdCommonCxxCompilerSettings.Empty();
            if (useDefaults)
            {
                stdCommonCxxCompilerSettings.Defaults(module);
            }

            var commonCompilerOptions = this as GccCommon.V2.ICommonCompilerOptions;
            commonCompilerOptions.Empty();
            if (useDefaults)
            {
                commonCompilerOptions.Defaults(module);
            }
#endif
        }

        void CommandLineProcessor.V2.IConvertToCommandLine.Convert(Bam.Core.V2.Module module, Bam.Core.StringArray commandLine)
        {
            (this as C.V2.ICommonCompilerOptions).Convert(module, commandLine);
            (this as C.V2.ICxxOnlyCompilerOptions).Convert(module, commandLine);
            (this as GccCommon.V2.ICommonCompilerOptions).Convert(module, commandLine);
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

        C.Cxx.EStandardLibrary? C.V2.ICxxOnlyCompilerOptions.StandardLibrary
        {
            get;
            set;
        }

        bool? GccCommon.V2.ICommonCompilerOptions.PositionIndependentCode
        {
            get;
            set;
        }
    }

    public class LibrarianSettings :
        C.V2.SettingsBase,
        CommandLineProcessor.V2.IConvertToCommandLine,
        C.V2.ICommonArchiverOptions,
        IArchiverOptions
    {
        public LibrarianSettings(Bam.Core.V2.Module module)
        {
#if true
            this.InitializeAllInterfaces(module, false, true);
#else
            (this as C.V2.ICommonArchiverOptions).Defaults(module);
            (this as IArchiverOptions).Defaults(module);
#endif
        }

        void CommandLineProcessor.V2.IConvertToCommandLine.Convert(Bam.Core.V2.Module module, Bam.Core.StringArray commandLine)
        {
            (this as IArchiverOptions).Convert(module, commandLine);
            // output file comes last, before inputs
            (this as C.V2.ICommonArchiverOptions).Convert(module, commandLine);
        }

        C.EArchiverOutput C.V2.ICommonArchiverOptions.OutputType
        {
            get;
            set;
        }

        bool IArchiverOptions.Ranlib
        {
            get;
            set;
        }

        bool IArchiverOptions.DoNotWarnIfLibraryCreated
        {
            get;
            set;
        }

        EArchiverCommand IArchiverOptions.Command
        {
            get;
            set;
        }
    }

    public class LinkerSettings :
        C.V2.SettingsBase,
        CommandLineProcessor.V2.IConvertToCommandLine,
        C.V2.ICommonLinkerOptions,
        GccCommon.V2.ICommonLinkerOptions
    {
        public LinkerSettings(Bam.Core.V2.Module module)
        {
#if true
            this.InitializeAllInterfaces(module, false, true);
#else
            (this as C.V2.ICommonLinkerOptions).Defaults(module);
            (this as GccCommon.V2.ICommonLinkerOptions).Defaults(module);
#endif
        }

        void CommandLineProcessor.V2.IConvertToCommandLine.Convert(Bam.Core.V2.Module module, Bam.Core.StringArray commandLine)
        {
            (this as C.V2.ICommonLinkerOptions).Convert(module, commandLine);
            (this as GccCommon.V2.ICommonLinkerOptions).Convert(module, commandLine);
        }

        C.ELinkerOutput C.V2.ICommonLinkerOptions.OutputType
        {
            get;
            set;
        }

        Bam.Core.Array<Bam.Core.V2.TokenizedString> C.V2.ICommonLinkerOptions.LibraryPaths
        {
            get;
            set;
        }

        Bam.Core.StringArray C.V2.ICommonLinkerOptions.Libraries
        {
            get;
            set;
        }

        bool? C.V2.ICommonLinkerOptions.DebugSymbols
        {
            get;
            set;
        }

        bool? ICommonLinkerOptions.CanUseOrigin
        {
            get;
            set;
        }

        Bam.Core.StringArray ICommonLinkerOptions.RPath
        {
            get;
            set;
        }

        Bam.Core.StringArray ICommonLinkerOptions.RPathLink
        {
            get;
            set;
        }
    }

    public static class Configure
    {
        static Configure()
        {
            InstallPath = Bam.Core.V2.TokenizedString.Create(@"/usr/bin", null);
        }

        public static Bam.Core.V2.TokenizedString InstallPath
        {
            get;
            private set;
        }
    }

    [C.V2.RegisterArchiver("GCC", Bam.Core.EPlatform.Linux, C.V2.EBit.ThirtyTwo)]
    [C.V2.RegisterArchiver("GCC", Bam.Core.EPlatform.Linux, C.V2.EBit.SixtyFour)]
    public sealed class Librarian :
        C.V2.LibrarianTool
    {
        public Librarian()
        {
            this.Macros.Add("InstallPath", Configure.InstallPath);
            this.Macros.Add("libprefix", "lib");
            this.Macros.Add("libext", ".a");
            this.Macros.Add("LibrarianPath", Bam.Core.V2.TokenizedString.Create("$(InstallPath)/ar", this));
        }

        public override Bam.Core.V2.Settings CreateDefaultSettings<T>(T module)
        {
            var settings = new LibrarianSettings(module);
            return settings;
        }

        public override Bam.Core.V2.TokenizedString Executable
        {
            get
            {
                return this.Macros["LibrarianPath"];
            }
        }
    }

    public abstract class LinkerBase :
        C.V2.LinkerTool
    {
        public LinkerBase(
            string executable)
        {
            this.EnvironmentVariables.Add("PATH", new Bam.Core.V2.TokenizedStringArray(Bam.Core.V2.TokenizedString.Create(@"$(InstallPath)", this)));

            this.Macros.Add("InstallPath", Configure.InstallPath);
            this.Macros.Add("exeext", string.Empty);
            this.Macros.Add("dynamicprefix", "lib");
            this.Macros.Add("dynamicext", ".so");
            this.Macros.Add("LinkerPath", Bam.Core.V2.TokenizedString.Create("$(InstallPath)/" + executable, this));
        }

        private static string
        GetLPrefixLibraryName(
            string fullLibraryPath)
        {
            var libName = System.IO.Path.GetFileNameWithoutExtension(fullLibraryPath);
            libName = libName.Substring(3); // trim off lib prefix
            return System.String.Format("-l{0}", libName);
        }

        private static Bam.Core.Array<C.V2.CModule>
        FindAllDynamicDependents(
            C.V2.DynamicLibrary dynamicModule)
        {
            var dynamicDeps = new Bam.Core.Array<C.V2.CModule>();
            if (0 == dynamicModule.Dependents.Count)
            {
                return dynamicDeps;
            }

            foreach (var dep in dynamicModule.Dependents)
            {
                if (!(dep is C.V2.DynamicLibrary))
                {
                    continue;
                }
                var dynDep = dep as C.V2.DynamicLibrary;
                dynamicDeps.AddUnique(dynDep);
                dynamicDeps.AddRangeUnique(FindAllDynamicDependents(dynDep));
            }
            return dynamicDeps;
        }

        public override void ProcessLibraryDependency(
            C.V2.CModule executable,
            C.V2.CModule library)
        {
            var linker = executable.Settings as C.V2.ICommonLinkerOptions;
            if (library is C.V2.StaticLibrary)
            {
                var libraryPath = library.GeneratedPaths[C.V2.StaticLibrary.Key].Parse();
                // order matters on libraries - the last occurrence is always the one that matters to resolve all symbols
                var libraryName = GetLPrefixLibraryName(libraryPath);
                if (linker.Libraries.Contains(libraryName))
                {
                    linker.Libraries.Remove(libraryName);
                }
                linker.Libraries.Add(libraryName);

                var libraryDir = Bam.Core.V2.TokenizedString.Create(System.IO.Path.GetDirectoryName(libraryPath), null);
                linker.LibraryPaths.AddUnique(libraryDir);
            }
            else if (library is C.V2.DynamicLibrary)
            {
                var libraryPath = library.GeneratedPaths[C.V2.DynamicLibrary.Key].Parse();
                // order matters on libraries - the last occurrence is always the one that matters to resolve all symbols
                var libraryName = GetLPrefixLibraryName(libraryPath);
                if (linker.Libraries.Contains(libraryName))
                {
                    linker.Libraries.Remove(libraryName);
                }
                linker.Libraries.Add(libraryName);

                var libraryDir = Bam.Core.V2.TokenizedString.Create(System.IO.Path.GetDirectoryName(libraryPath), null);
                linker.LibraryPaths.AddUnique(libraryDir);

                var gccLinker = executable.Settings as GccCommon.V2.ICommonLinkerOptions;
                var allDynamicDependents = FindAllDynamicDependents(library as C.V2.DynamicLibrary);
                foreach (var dep in allDynamicDependents)
                {
                    var depLibraryPath = dep.GeneratedPaths[C.V2.DynamicLibrary.Key].Parse();
                    var depLibraryDir = Bam.Core.V2.TokenizedString.Create(System.IO.Path.GetDirectoryName(depLibraryPath), null);
                    gccLinker.RPathLink.AddUnique(depLibraryDir.Parse());
                }
            }
        }

        public override Bam.Core.V2.Settings CreateDefaultSettings<T>(T module)
        {
            var settings = new LinkerSettings(module);
            return settings;
        }

        public override Bam.Core.V2.TokenizedString Executable
        {
            get
            {
                return this.Macros["LinkerPath"];
            }
        }
    }

    [C.V2.RegisterCLinker("GCC", Bam.Core.EPlatform.Linux, C.V2.EBit.ThirtyTwo)]
    [C.V2.RegisterCLinker("GCC", Bam.Core.EPlatform.Linux, C.V2.EBit.SixtyFour)]
    public sealed class Linker :
        LinkerBase
    {
        public Linker()
            : base("gcc-4.8")
        {}
    }

    [C.V2.RegisterCxxLinker("GCC", Bam.Core.EPlatform.Linux, C.V2.EBit.ThirtyTwo)]
    [C.V2.RegisterCxxLinker("GCC", Bam.Core.EPlatform.Linux, C.V2.EBit.SixtyFour)]
    public sealed class LinkerCxx :
        LinkerBase
    {
        public LinkerCxx()
            : base("g++-4.8")
        {}
    }

    public abstract class CompilerBase :
        C.V2.CompilerTool
    {
        protected CompilerBase()
        {
            this.Macros.Add("InstallPath", Configure.InstallPath);
            this.Macros.Add("objext", ".o");
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
            // NOTE: note that super classes need to be checked last in order to
            // honour the class hierarchy
            if (typeof(C.ObjCxx.V2.ObjectFile).IsInstanceOfType(module) ||
                typeof(C.ObjCxx.V2.ObjectFileCollection).IsInstanceOfType(module))
            {
                var settings = new ObjectiveCxxCompilerSettings(module);
                this.OverrideDefaultSettings(settings);
                return settings;
            }
            else if (typeof(C.ObjC.V2.ObjectFile).IsInstanceOfType(module) ||
                     typeof(C.ObjC.V2.ObjectFileCollection).IsInstanceOfType(module))
            {
                var settings = new ObjectiveCCompilerSettings(module);
                this.OverrideDefaultSettings(settings);
                return settings;
            }
            else if (typeof(C.Cxx.V2.ObjectFile).IsInstanceOfType(module) ||
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

        public override void
        CompileAsShared(
            Bam.Core.V2.Settings settings)
        {
            var gccCompiler = settings as GccCommon.V2.ICommonCompilerOptions;
            gccCompiler.PositionIndependentCode = true;
        }

        protected abstract void OverrideDefaultSettings(Bam.Core.V2.Settings settings);
    }

    [C.V2.RegisterCCompiler("GCC", Bam.Core.EPlatform.Linux, C.V2.EBit.ThirtyTwo)]
    [C.V2.RegisterCCompiler("GCC", Bam.Core.EPlatform.Linux, C.V2.EBit.SixtyFour)]
    public sealed class CCompiler :
        CompilerBase
    {
        public CCompiler()
        {
            this.Macros.Add("CompilerPath", Bam.Core.V2.TokenizedString.Create("$(InstallPath)/gcc-4.8", this));
        }

        protected override void OverrideDefaultSettings(Bam.Core.V2.Settings settings)
        {
            var cSettings = settings as C.V2.ICommonCompilerOptions;
            cSettings.TargetLanguage = C.ETargetLanguage.C;
        }
    }

    [C.V2.RegisterCxxCompiler("GCC", Bam.Core.EPlatform.Linux, C.V2.EBit.ThirtyTwo)]
    [C.V2.RegisterCxxCompiler("GCC", Bam.Core.EPlatform.Linux, C.V2.EBit.SixtyFour)]
    public sealed class CxxCompiler :
        CompilerBase
    {
        public CxxCompiler()
        {
            this.Macros.Add("CompilerPath", Bam.Core.V2.TokenizedString.Create("$(InstallPath)/g++-4.8", this));
        }

        protected override void OverrideDefaultSettings(Bam.Core.V2.Settings settings)
        {
            var cSettings = settings as C.V2.ICommonCompilerOptions;
            cSettings.TargetLanguage = C.ETargetLanguage.Cxx;
        }
    }

    [C.V2.RegisterObjectiveCCompiler("GCC", Bam.Core.EPlatform.Linux, C.V2.EBit.ThirtyTwo)]
    [C.V2.RegisterObjectiveCCompiler("GCC", Bam.Core.EPlatform.Linux, C.V2.EBit.SixtyFour)]
    public sealed class ObjectiveCCompiler :
        CompilerBase
    {
        public ObjectiveCCompiler()
        {
            this.Macros.Add("CompilerPath", Bam.Core.V2.TokenizedString.Create("$(InstallPath)/gcc-4.8", this));
        }

        protected override void OverrideDefaultSettings(Bam.Core.V2.Settings settings)
        {
            var compiler = settings as C.V2.ICommonCompilerOptions;
            compiler.TargetLanguage = C.ETargetLanguage.ObjectiveC;
            var cCompiler = settings as C.V2.ICOnlyCompilerOptions;
            cCompiler.LanguageStandard = C.ECLanguageStandard.C99;
        }
    }

    [C.V2.RegisterObjectiveCxxCompiler("GCC", Bam.Core.EPlatform.Linux, C.V2.EBit.ThirtyTwo)]
    [C.V2.RegisterObjectiveCxxCompiler("GCC", Bam.Core.EPlatform.Linux, C.V2.EBit.SixtyFour)]
    public sealed class ObjectiveCxxCompiler :
    CompilerBase
    {
        public ObjectiveCxxCompiler()
        {
            this.Macros.Add("CompilerPath", Bam.Core.V2.TokenizedString.Create("$(InstallPath)/g++-4.8", this));
        }

        protected override void OverrideDefaultSettings(Bam.Core.V2.Settings settings)
        {
            var cSettings = settings as C.V2.ICommonCompilerOptions;
            cSettings.TargetLanguage = C.ETargetLanguage.ObjectiveCxx;
        }
    }
}
    // Not sealed since the C++ compiler inherits from it
    public partial class CCompilerOptionCollection :
        GccCommon.CCompilerOptionCollection,
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

            // requires gcc 4.0
            (this as ICCompilerOptions).Visibility = EVisibility.Hidden;
        }
    }
}
