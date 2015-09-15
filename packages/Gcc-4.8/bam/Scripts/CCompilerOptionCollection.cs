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
using C.ObjC.DefaultSettings;
using GccCommon.DefaultSettings;
using Gcc.DefaultSettings;
using GccCommon; // TODO: for the native implementation
namespace Gcc
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
            if (null != options.Bits)
            {
                if (options.Bits == C.EBit.SixtyFour)
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
                        commandLine.Add(System.String.Format("-o {0}", module.GeneratedPaths[C.ObjectFile.Key].ToString()));
                        break;
                    case C.ECompilerOutput.Preprocess:
                        commandLine.Add(System.String.Format("-E {0}", objectFile.InputPath.ToString()));
                        commandLine.Add(System.String.Format("-o {0}", module.GeneratedPaths[C.ObjectFile.Key].ToString()));
                        break;
                }
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
                        commandLine.Add("-std=c89");
                        break;

                    case C.ELanguageStandard.C99:
                        commandLine.Add("-std=c99");
                        break;

                    default:
                        throw new Bam.Core.Exception("Invalid C language standard, '{0}'", options.LanguageStandard.ToString());
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
            this C.IObjectiveCOnlyCompilerOptions options,
            Bam.Core.Module module,
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
            this C.ICommonArchiverOptions options,
            Bam.Core.Module module,
            Bam.Core.StringArray commandLine)
        {
            switch (options.OutputType)
            {
                case C.EArchiverOutput.StaticLibrary:
                    commandLine.Add(module.GeneratedPaths[C.StaticLibrary.Key].ToString());
                    break;
            }
        }

        public static void
        Convert(
            this IArchiverOptions options,
            Bam.Core.Module module,
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
                case EArchiverCommand.Replace:
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
            this C.ICommonLinkerOptions options,
            Bam.Core.Module module,
            Bam.Core.StringArray commandLine)
        {
            //var applicationFile = module as C.ConsoleApplication;
            switch (options.OutputType)
            {
                case C.ELinkerOutput.Executable:
                    commandLine.Add(System.String.Format("-o {0}", module.GeneratedPaths[C.ConsoleApplication.Key].ToString()));
                    break;

                case C.ELinkerOutput.DynamicLibrary:
                    commandLine.Add("-shared");
                    commandLine.Add(System.String.Format("-o {0}", module.GeneratedPaths[C.ConsoleApplication.Key].ToString()));
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

    namespace DefaultSettings
    {
        public static partial class DefaultSettingsExtensions
        {
            public static void Defaults(this IArchiverOptions settings, Bam.Core.Module module)
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

    [Bam.Core.SettingsExtensions(typeof(Gcc.DefaultSettings.DefaultSettingsExtensions))]
    public interface IArchiverOptions : Bam.Core.ISettingsBase
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
        C.SettingsBase,
        CommandLineProcessor.IConvertToCommandLine,
        C.ICommonCompilerOptions,
        C.ICOnlyCompilerOptions,
        GccCommon.ICommonCompilerOptions
    {
        public CompilerSettings(Bam.Core.Module module)
            : this(module, true)
        {
        }

        public CompilerSettings(Bam.Core.Module module, bool useDefaults)
        {
#if true
            this.InitializeAllInterfaces(module, true, useDefaults);
#else
            var stdCommonCompilerOptions = this as C.ICommonCompilerOptions;
            stdCommonCompilerOptions.Empty();
            if (useDefaults)
            {
                stdCommonCompilerOptions.Defaults(module);
            }

            var commonCompilerOptions = this as GccCommon.ICommonCompilerOptions;
            commonCompilerOptions.Empty();
            if (useDefaults)
            {
                commonCompilerOptions.Defaults(module);
            }
#endif
        }

        void CommandLineProcessor.IConvertToCommandLine.Convert(Bam.Core.Module module, Bam.Core.StringArray commandLine)
        {
            (this as C.ICommonCompilerOptions).Convert(module, commandLine);
            (this as GccCommon.ICommonCompilerOptions).Convert(module, commandLine);
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

        bool? GccCommon.ICommonCompilerOptions.PositionIndependentCode
        {
            get;
            set;
        }
    }

    public sealed class CxxCompilerSettings :
        C.SettingsBase,
        CommandLineProcessor.IConvertToCommandLine,
        C.ICommonCompilerOptions,
        C.ICxxOnlyCompilerOptions,
        GccCommon.ICommonCompilerOptions
    {
        public CxxCompilerSettings(Bam.Core.Module module)
            : this(module, true)
        {
        }

        public CxxCompilerSettings(Bam.Core.Module module, bool useDefaults)
        {
#if true
            this.InitializeAllInterfaces(module, true, useDefaults);
#else
            var stdCommonCompilerOptions = this as C.ICommonCompilerOptions;
            stdCommonCompilerOptions.Empty();
            if (useDefaults)
            {
                stdCommonCompilerOptions.Defaults(module);
            }

            var stdCommonCxxCompilerSettings = this as C.ICxxOnlyCompilerOptions;
            stdCommonCxxCompilerSettings.Empty();
            if (useDefaults)
            {
                stdCommonCxxCompilerSettings.Defaults(module);
            }

            var commonCompilerOptions = this as GccCommon.ICommonCompilerOptions;
            commonCompilerOptions.Empty();
            if (useDefaults)
            {
                commonCompilerOptions.Defaults(module);
            }
#endif
        }

        void CommandLineProcessor.IConvertToCommandLine.Convert(Bam.Core.Module module, Bam.Core.StringArray commandLine)
        {
            (this as C.ICommonCompilerOptions).Convert(module, commandLine);
            (this as C.ICxxOnlyCompilerOptions).Convert(module, commandLine);
            (this as GccCommon.ICommonCompilerOptions).Convert(module, commandLine);
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

        bool? GccCommon.ICommonCompilerOptions.PositionIndependentCode
        {
            get;
            set;
        }
    }

    public class ObjectiveCCompilerSettings :
        C.SettingsBase,
        CommandLineProcessor.IConvertToCommandLine,
        C.ICommonCompilerOptions,
        C.ICOnlyCompilerOptions,
        C.IObjectiveCOnlyCompilerOptions,
        GccCommon.ICommonCompilerOptions
    {
        public ObjectiveCCompilerSettings(Bam.Core.Module module)
            : this(module, true)
        {
        }

        public ObjectiveCCompilerSettings(Bam.Core.Module module, bool useDefaults)
        {
#if true
            this.InitializeAllInterfaces(module, true, useDefaults);
#else
            var stdCommonCompilerOptions = this as C.ICommonCompilerOptions;
            stdCommonCompilerOptions.Empty();
            if (useDefaults)
            {
                stdCommonCompilerOptions.Defaults(module);
            }

            var objCCompilerOptions = this as C.IObjectiveCOnlyCompilerOptions;
            objCCompilerOptions.Empty();
            if (useDefaults)
            {
               objCCompilerOptions.Defaults(module);
            }

            var commonCompilerOptions = this as GccCommon.ICommonCompilerOptions;
            commonCompilerOptions.Empty();
            if (useDefaults)
            {
                commonCompilerOptions.Defaults(module);
            }
#endif
        }

        void CommandLineProcessor.IConvertToCommandLine.Convert(Bam.Core.Module module, Bam.Core.StringArray commandLine)
        {
            (this as C.ICommonCompilerOptions).Convert(module, commandLine);
            (this as C.IObjectiveCOnlyCompilerOptions).Convert(module, commandLine);
            (this as GccCommon.ICommonCompilerOptions).Convert(module, commandLine);
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

        string C.IObjectiveCOnlyCompilerOptions.ConstantStringClass
        {
            get;
            set;
        }

        bool? GccCommon.ICommonCompilerOptions.PositionIndependentCode
        {
            get;
            set;
        }
    }

    public sealed class ObjectiveCxxCompilerSettings :
        C.SettingsBase,
        CommandLineProcessor.IConvertToCommandLine,
        C.ICommonCompilerOptions,
        C.ICxxOnlyCompilerOptions,
        C.IObjectiveCxxOnlyCompilerOptions,
        GccCommon.ICommonCompilerOptions
    {
        public ObjectiveCxxCompilerSettings(Bam.Core.Module module)
            : this(module, true)
        {
        }

        public ObjectiveCxxCompilerSettings(Bam.Core.Module module, bool useDefaults)
        {
#if true
            this.InitializeAllInterfaces(module, true, useDefaults);
#else
            var stdCommonCompilerOptions = this as C.ICommonCompilerOptions;
            stdCommonCompilerOptions.Empty();
            if (useDefaults)
            {
                stdCommonCompilerOptions.Defaults(module);
            }

            var stdCommonCxxCompilerSettings = this as C.ICxxOnlyCompilerOptions;
            stdCommonCxxCompilerSettings.Empty();
            if (useDefaults)
            {
                stdCommonCxxCompilerSettings.Defaults(module);
            }

            var commonCompilerOptions = this as GccCommon.ICommonCompilerOptions;
            commonCompilerOptions.Empty();
            if (useDefaults)
            {
                commonCompilerOptions.Defaults(module);
            }
#endif
        }

        void CommandLineProcessor.IConvertToCommandLine.Convert(Bam.Core.Module module, Bam.Core.StringArray commandLine)
        {
            (this as C.ICommonCompilerOptions).Convert(module, commandLine);
            (this as C.ICxxOnlyCompilerOptions).Convert(module, commandLine);
            (this as GccCommon.ICommonCompilerOptions).Convert(module, commandLine);
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

        bool? GccCommon.ICommonCompilerOptions.PositionIndependentCode
        {
            get;
            set;
        }
    }

    public class LibrarianSettings :
        C.SettingsBase,
        CommandLineProcessor.IConvertToCommandLine,
        C.ICommonArchiverOptions,
        IArchiverOptions
    {
        public LibrarianSettings(Bam.Core.Module module)
        {
#if true
            this.InitializeAllInterfaces(module, false, true);
#else
            (this as C.ICommonArchiverOptions).Defaults(module);
            (this as IArchiverOptions).Defaults(module);
#endif
        }

        void CommandLineProcessor.IConvertToCommandLine.Convert(Bam.Core.Module module, Bam.Core.StringArray commandLine)
        {
            (this as IArchiverOptions).Convert(module, commandLine);
            // output file comes last, before inputs
            (this as C.ICommonArchiverOptions).Convert(module, commandLine);
        }

        C.EArchiverOutput C.ICommonArchiverOptions.OutputType
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
        C.SettingsBase,
        CommandLineProcessor.IConvertToCommandLine,
        C.ICommonLinkerOptions,
        GccCommon.ICommonLinkerOptions
    {
        public LinkerSettings(Bam.Core.Module module)
        {
#if true
            this.InitializeAllInterfaces(module, false, true);
#else
            (this as C.ICommonLinkerOptions).Defaults(module);
            (this as GccCommon.ICommonLinkerOptions).Defaults(module);
#endif
        }

        void CommandLineProcessor.IConvertToCommandLine.Convert(Bam.Core.Module module, Bam.Core.StringArray commandLine)
        {
            (this as C.ICommonLinkerOptions).Convert(module, commandLine);
            (this as GccCommon.ICommonLinkerOptions).Convert(module, commandLine);
        }

        C.ELinkerOutput C.ICommonLinkerOptions.OutputType
        {
            get;
            set;
        }

        Bam.Core.Array<Bam.Core.TokenizedString> C.ICommonLinkerOptions.LibraryPaths
        {
            get;
            set;
        }

        Bam.Core.StringArray C.ICommonLinkerOptions.Libraries
        {
            get;
            set;
        }

        bool? C.ICommonLinkerOptions.DebugSymbols
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
            InstallPath = Bam.Core.TokenizedString.Create(@"/usr/bin", null);
        }

        public static Bam.Core.TokenizedString InstallPath
        {
            get;
            private set;
        }
    }

    [C.RegisterArchiver("GCC", Bam.Core.EPlatform.Linux, C.EBit.ThirtyTwo)]
    [C.RegisterArchiver("GCC", Bam.Core.EPlatform.Linux, C.EBit.SixtyFour)]
    public sealed class Librarian :
        C.LibrarianTool
    {
        public Librarian()
        {
            this.Macros.Add("InstallPath", Configure.InstallPath);
            this.Macros.Add("libprefix", "lib");
            this.Macros.Add("libext", ".a");
            this.Macros.Add("LibrarianPath", Bam.Core.TokenizedString.Create("$(InstallPath)/ar", this));
        }

        public override Bam.Core.Settings CreateDefaultSettings<T>(T module)
        {
            var settings = new LibrarianSettings(module);
            return settings;
        }

        public override Bam.Core.TokenizedString Executable
        {
            get
            {
                return this.Macros["LibrarianPath"];
            }
        }
    }

    public abstract class LinkerBase :
        C.LinkerTool
    {
        public LinkerBase(
            string executable)
        {
            this.EnvironmentVariables.Add("PATH", new Bam.Core.TokenizedStringArray(Bam.Core.TokenizedString.Create(@"$(InstallPath)", this)));

            this.Macros.Add("InstallPath", Configure.InstallPath);
            this.Macros.Add("exeext", string.Empty);
            this.Macros.Add("dynamicprefix", "lib");
            this.Macros.Add("dynamicext", ".so");
            this.Macros.Add("LinkerPath", Bam.Core.TokenizedString.Create("$(InstallPath)/" + executable, this));
        }

        private static string
        GetLPrefixLibraryName(
            string fullLibraryPath)
        {
            var libName = System.IO.Path.GetFileNameWithoutExtension(fullLibraryPath);
            libName = libName.Substring(3); // trim off lib prefix
            return System.String.Format("-l{0}", libName);
        }

        private static Bam.Core.Array<C.CModule>
        FindAllDynamicDependents(
            C.DynamicLibrary dynamicModule)
        {
            var dynamicDeps = new Bam.Core.Array<C.CModule>();
            if (0 == dynamicModule.Dependents.Count)
            {
                return dynamicDeps;
            }

            foreach (var dep in dynamicModule.Dependents)
            {
                if (!(dep is C.DynamicLibrary))
                {
                    continue;
                }
                var dynDep = dep as C.DynamicLibrary;
                dynamicDeps.AddUnique(dynDep);
                dynamicDeps.AddRangeUnique(FindAllDynamicDependents(dynDep));
            }
            return dynamicDeps;
        }

        public override void ProcessLibraryDependency(
            C.CModule executable,
            C.CModule library)
        {
            var linker = executable.Settings as C.ICommonLinkerOptions;
            if (library is C.StaticLibrary)
            {
                var libraryPath = library.GeneratedPaths[C.StaticLibrary.Key].Parse();
                // order matters on libraries - the last occurrence is always the one that matters to resolve all symbols
                var libraryName = GetLPrefixLibraryName(libraryPath);
                if (linker.Libraries.Contains(libraryName))
                {
                    linker.Libraries.Remove(libraryName);
                }
                linker.Libraries.Add(libraryName);

                var libraryDir = Bam.Core.TokenizedString.Create(System.IO.Path.GetDirectoryName(libraryPath), null);
                linker.LibraryPaths.AddUnique(libraryDir);
            }
            else if (library is C.DynamicLibrary)
            {
                var libraryPath = library.GeneratedPaths[C.DynamicLibrary.Key].Parse();
                // order matters on libraries - the last occurrence is always the one that matters to resolve all symbols
                var libraryName = GetLPrefixLibraryName(libraryPath);
                if (linker.Libraries.Contains(libraryName))
                {
                    linker.Libraries.Remove(libraryName);
                }
                linker.Libraries.Add(libraryName);

                var libraryDir = Bam.Core.TokenizedString.Create(System.IO.Path.GetDirectoryName(libraryPath), null);
                linker.LibraryPaths.AddUnique(libraryDir);

                var gccLinker = executable.Settings as GccCommon.ICommonLinkerOptions;
                var allDynamicDependents = FindAllDynamicDependents(library as C.DynamicLibrary);
                foreach (var dep in allDynamicDependents)
                {
                    var depLibraryPath = dep.GeneratedPaths[C.DynamicLibrary.Key].Parse();
                    var depLibraryDir = Bam.Core.TokenizedString.Create(System.IO.Path.GetDirectoryName(depLibraryPath), null);
                    gccLinker.RPathLink.AddUnique(depLibraryDir.Parse());
                }
            }
        }

        public override Bam.Core.Settings CreateDefaultSettings<T>(T module)
        {
            var settings = new LinkerSettings(module);
            return settings;
        }

        public override Bam.Core.TokenizedString Executable
        {
            get
            {
                return this.Macros["LinkerPath"];
            }
        }
    }

    [C.RegisterCLinker("GCC", Bam.Core.EPlatform.Linux, C.EBit.ThirtyTwo)]
    [C.RegisterCLinker("GCC", Bam.Core.EPlatform.Linux, C.EBit.SixtyFour)]
    public sealed class Linker :
        LinkerBase
    {
        public Linker()
            : base("gcc-4.8")
        {}
    }

    [C.RegisterCxxLinker("GCC", Bam.Core.EPlatform.Linux, C.EBit.ThirtyTwo)]
    [C.RegisterCxxLinker("GCC", Bam.Core.EPlatform.Linux, C.EBit.SixtyFour)]
    public sealed class LinkerCxx :
        LinkerBase
    {
        public LinkerCxx()
            : base("g++-4.8")
        {}
    }

    public abstract class CompilerBase :
        C.CompilerTool
    {
        protected CompilerBase()
        {
            this.Macros.Add("InstallPath", Configure.InstallPath);
            this.Macros.Add("objext", ".o");
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
            // NOTE: note that super classes need to be checked last in order to
            // honour the class hierarchy
            if (typeof(C.ObjCxx.ObjectFile).IsInstanceOfType(module) ||
                typeof(C.ObjCxx.ObjectFileCollection).IsInstanceOfType(module))
            {
                var settings = new ObjectiveCxxCompilerSettings(module);
                this.OverrideDefaultSettings(settings);
                return settings;
            }
            else if (typeof(C.ObjC.ObjectFile).IsInstanceOfType(module) ||
                     typeof(C.ObjC.ObjectFileCollection).IsInstanceOfType(module))
            {
                var settings = new ObjectiveCCompilerSettings(module);
                this.OverrideDefaultSettings(settings);
                return settings;
            }
            else if (typeof(C.Cxx.ObjectFile).IsInstanceOfType(module) ||
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

        public override void
        CompileAsShared(
            Bam.Core.Settings settings)
        {
            var gccCompiler = settings as GccCommon.ICommonCompilerOptions;
            gccCompiler.PositionIndependentCode = true;
        }

        protected abstract void OverrideDefaultSettings(Bam.Core.Settings settings);
    }

    [C.RegisterCCompiler("GCC", Bam.Core.EPlatform.Linux, C.EBit.ThirtyTwo)]
    [C.RegisterCCompiler("GCC", Bam.Core.EPlatform.Linux, C.EBit.SixtyFour)]
    public sealed class CCompiler :
        CompilerBase
    {
        public CCompiler()
        {
            this.Macros.Add("CompilerPath", Bam.Core.TokenizedString.Create("$(InstallPath)/gcc-4.8", this));
        }

        protected override void OverrideDefaultSettings(Bam.Core.Settings settings)
        {
            var cSettings = settings as C.ICommonCompilerOptions;
            cSettings.TargetLanguage = C.ETargetLanguage.C;
        }
    }

    [C.RegisterCxxCompiler("GCC", Bam.Core.EPlatform.Linux, C.EBit.ThirtyTwo)]
    [C.RegisterCxxCompiler("GCC", Bam.Core.EPlatform.Linux, C.EBit.SixtyFour)]
    public sealed class CxxCompiler :
        CompilerBase
    {
        public CxxCompiler()
        {
            this.Macros.Add("CompilerPath", Bam.Core.TokenizedString.Create("$(InstallPath)/g++-4.8", this));
        }

        protected override void OverrideDefaultSettings(Bam.Core.Settings settings)
        {
            var cSettings = settings as C.ICommonCompilerOptions;
            cSettings.TargetLanguage = C.ETargetLanguage.Cxx;
        }
    }

    [C.RegisterObjectiveCCompiler("GCC", Bam.Core.EPlatform.Linux, C.EBit.ThirtyTwo)]
    [C.RegisterObjectiveCCompiler("GCC", Bam.Core.EPlatform.Linux, C.EBit.SixtyFour)]
    public sealed class ObjectiveCCompiler :
        CompilerBase
    {
        public ObjectiveCCompiler()
        {
            this.Macros.Add("CompilerPath", Bam.Core.TokenizedString.Create("$(InstallPath)/gcc-4.8", this));
        }

        protected override void OverrideDefaultSettings(Bam.Core.Settings settings)
        {
            var compiler = settings as C.ICommonCompilerOptions;
            compiler.TargetLanguage = C.ETargetLanguage.ObjectiveC;
            var cCompiler = settings as C.ICOnlyCompilerOptions;
            cCompiler.LanguageStandard = C.ELanguageStandard.C99;
        }
    }

    [C.RegisterObjectiveCxxCompiler("GCC", Bam.Core.EPlatform.Linux, C.EBit.ThirtyTwo)]
    [C.RegisterObjectiveCxxCompiler("GCC", Bam.Core.EPlatform.Linux, C.EBit.SixtyFour)]
    public sealed class ObjectiveCxxCompiler :
    CompilerBase
    {
        public ObjectiveCxxCompiler()
        {
            this.Macros.Add("CompilerPath", Bam.Core.TokenizedString.Create("$(InstallPath)/g++-4.8", this));
        }

        protected override void OverrideDefaultSettings(Bam.Core.Settings settings)
        {
            var cSettings = settings as C.ICommonCompilerOptions;
            cSettings.TargetLanguage = C.ETargetLanguage.ObjectiveCxx;
        }
    }
}
