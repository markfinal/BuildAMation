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
using C.V2.DefaultSettings;
using C.Cxx.V2.DefaultSettings;
namespace VisualC
{
    public static partial class VSSolutionImplementation
    {
        public static void
        Convert(
            this C.V2.ICommonCompilerOptions options,
            Bam.Core.V2.Module module,
            VSSolutionBuilder.V2.VSSettingsGroup settingsGroup,
            string condition)
        {
            // write nothing for disabled debug symbols, otherwise the source files rebuild continually
            // and reports a warning that the pdb file does not exist
            // the IDE can write None into the .vcxproj, but this has the same behaviour
            // https://connect.microsoft.com/VisualStudio/feedback/details/833494/project-with-debug-information-disabled-always-rebuilds
            if (options.DebugSymbols.GetValueOrDefault(false))
            {
                settingsGroup.AddSetting("DebugInformationFormat", "OldStyle", condition);
            }

            if (options.DisableWarnings.Count > 0)
            {
                settingsGroup.AddSetting("DisableSpecificWarnings", options.DisableWarnings, condition, inheritExisting: true);
            }

            if (options.IncludePaths.Count > 0)
            {
                settingsGroup.AddSetting("AdditionalIncludeDirectories", options.IncludePaths, condition, inheritExisting: true);
            }

            if (options.OmitFramePointer.HasValue)
            {
                settingsGroup.AddSetting("OmitFramePointers", options.OmitFramePointer.Value, condition);
            }

            if (options.Optimization.HasValue)
            {
                System.Func<string> optimization = () =>
                    {
                        switch (options.Optimization.Value)
                        {
                            case C.EOptimization.Off:
                                return "Disabled";

                            case C.EOptimization.Size:
                                return "MinSpace";

                            case C.EOptimization.Speed:
                                return "MaxSpeed";

                            case C.EOptimization.Full:
                                return "Full";

                            default:
                                throw new Bam.Core.Exception("Unknown optimization type, {0}", options.Optimization.Value.ToString());
                        }
                    };
                settingsGroup.AddSetting("Optimization", optimization(), condition);
            }

            if (options.PreprocessorDefines.Count > 0)
            {
                settingsGroup.AddSetting("PreprocessorDefinitions", options.PreprocessorDefines, condition, inheritExisting: true);
            }

            if (options.PreprocessorUndefines.Count > 0)
            {
                settingsGroup.AddSetting("UndefinePreprocessorDefinitions", options.PreprocessorUndefines, condition, inheritExisting: true);
            }

            if (options.TargetLanguage.HasValue)
            {
                System.Func<string> targetLanguage = () =>
                {
                    switch (options.TargetLanguage.Value)
                    {
                        case C.ETargetLanguage.C:
                            return "CompileAsC";

                        case C.ETargetLanguage.Cxx:
                            return "CompileAsCpp";

                        case C.ETargetLanguage.Default:
                            return "Default";

                        default:
                            throw new Bam.Core.Exception("Unknown target language, {0}", options.TargetLanguage.Value.ToString());
                    }
                };
                settingsGroup.AddSetting("CompileAs", targetLanguage(), condition);
            }

            if (options.WarningsAsErrors.HasValue)
            {
                settingsGroup.AddSetting("TreatWarningAsError", options.WarningsAsErrors.Value, condition);
            }

            if (options.OutputType.HasValue)
            {
                settingsGroup.AddSetting("PreprocessToFile", options.OutputType.Value == C.ECompilerOutput.Preprocess, condition);
                if (module is C.V2.ObjectFile)
                {
                    settingsGroup.AddSetting("ObjectFileName", module.GeneratedPaths[C.V2.ObjectFile.Key], condition);
                }
            }

        }

        public static void
        Convert(
            this C.V2.ICOnlyCompilerOptions options,
            Bam.Core.V2.Module module,
            VSSolutionBuilder.V2.VSSettingsGroup settingsGroup,
            string condition)
        {
        }

        public static void
        Convert(
            this C.V2.ICxxOnlyCompilerOptions options,
            Bam.Core.V2.Module module,
            VSSolutionBuilder.V2.VSSettingsGroup settingsGroup,
            string condition)
        {
            if (options.ExceptionHandler.HasValue)
            {
                System.Func<string> exceptionHandler = () =>
                {
                    switch (options.ExceptionHandler.Value)
                    {
                        case C.Cxx.EExceptionHandler.Disabled:
                            return "false";

                        case C.Cxx.EExceptionHandler.Asynchronous:
                            return "Async";

                        case C.Cxx.EExceptionHandler.Synchronous:
                            return "Sync";

                        case C.Cxx.EExceptionHandler.SyncWithCExternFunctions:
                            return "SyncCThrow";

                        default:
                            throw new Bam.Core.Exception("Unknown exception handler, {0}", options.ExceptionHandler.Value.ToString());
                    }
                };
                settingsGroup.AddSetting("ExceptionHandling", exceptionHandler(), condition);
            }
        }

        public static void
        Convert(
            this VisualCCommon.V2.ICommonCompilerOptions options,
            Bam.Core.V2.Module module,
            VSSolutionBuilder.V2.VSSettingsGroup settingsGroup,
            string condition)
        {
            if (options.NoLogo.GetValueOrDefault(false))
            {
                settingsGroup.AddSetting("SuppressStartupBanner", options.NoLogo.Value, condition);
            }
        }
    }

    public static partial class NativeImplementation
    {
        public static void
        Convert(
            this C.V2.ICommonCompilerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
            var objectFile = module as C.V2.ObjectFile;
            if (true == options.DebugSymbols)
            {
                commandLine.Add("-Z7");
            }
            foreach (var warning in options.DisableWarnings)
            {
                commandLine.Add(System.String.Format("-wd{0}", warning));
            }
            foreach (var path in options.IncludePaths)
            {
                var formatString = path.ContainsSpace ? "-I\"{0}\"" : "-I{0}";
                commandLine.Add(System.String.Format(formatString, path));
            }
            if (true == options.OmitFramePointer)
            {
                commandLine.Add("-Oy");
            }
            else
            {
                commandLine.Add("-Oy-");
            }
            switch (options.Optimization)
            {
                case C.EOptimization.Off:
                    commandLine.Add("-Od");
                    break;
                case C.EOptimization.Size:
                    commandLine.Add("-Os");
                    break;
                case C.EOptimization.Speed:
                    commandLine.Add("-O1");
                    break;
                case C.EOptimization.Full:
                    commandLine.Add("-Ox");
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
                    commandLine.Add(System.String.Format("-Tc {0}", objectFile.InputPath.ToString()));
                    break;
                case C.ETargetLanguage.Cxx:
                    commandLine.Add(System.String.Format("-Tp {0}", objectFile.InputPath.ToString()));
                    break;
                default:
                    throw new Bam.Core.Exception("Unsupported target language");
            }
            if (true == options.WarningsAsErrors)
            {
                commandLine.Add("-WX");
            }
            switch (options.OutputType)
            {
                case C.ECompilerOutput.CompileOnly:
                    commandLine.Add("-c");
                    commandLine.Add(System.String.Format("-Fo{0}", module.GeneratedPaths[C.V2.ObjectFile.Key].ToString()));
                    break;
                case C.ECompilerOutput.Preprocess:
                    commandLine.Add("-E");
                    commandLine.Add(System.String.Format("-Fo{0}", module.GeneratedPaths[C.V2.ObjectFile.Key].ToString()));
                    break;
            }
        }

        public static void
        Convert(
            this C.V2.ICOnlyCompilerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
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
                        // nothing
                        break;
                    case C.Cxx.EExceptionHandler.Asynchronous:
                        commandLine.Add("-EHa");
                        break;
                    case C.Cxx.EExceptionHandler.Synchronous:
                        commandLine.Add("-EHsc");
                        break;
                    case C.Cxx.EExceptionHandler.SyncWithCExternFunctions:
                        commandLine.Add("-EHs");
                        break;
                    default:
                        throw new Bam.Core.Exception("Unrecognized exception handler option");
                }
            }
        }

        public static void
        Convert(
            this VisualCCommon.V2.ICommonCompilerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
            if (null != options.NoLogo)
            {
                if (options.NoLogo == true)
                {
                    commandLine.Add("-nologo");
                }
            }
        }

        public static void
        Convert(
            this VisualCCommon.V2.ICOnlyCompilerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
        }

        public static void
        Convert(
            this VisualCCommon.V2.ICxxOnlyCompilerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
        }

        public static void
        Convert(
            this VisualC.V2.ICommonCompilerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
        }

        public static void
        Convert(
            this VisualC.V2.ICOnlyCompilerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
        }

        public static void
        Convert(
            this VisualC.V2.ICxxOnlyCompilerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
        }
    }

namespace V2
{
    [Bam.Core.V2.SettingsExtensions(typeof(C.V2.DefaultSettings.DefaultSettingsExtensions))]
    public interface ICommonCompilerOptions : Bam.Core.V2.ISettingsBase
    {
        bool VC12Common
        {
            get;
            set;
        }
    }

    [Bam.Core.V2.SettingsExtensions(typeof(C.V2.DefaultSettings.DefaultSettingsExtensions))]
    public interface ICOnlyCompilerOptions : Bam.Core.V2.ISettingsBase
    {
        int VC12COnly
        {
            get;
            set;
        }
    }

    [Bam.Core.V2.SettingsExtensions(typeof(C.V2.DefaultSettings.DefaultSettingsExtensions))]
    public interface ICxxOnlyCompilerOptions : Bam.Core.V2.ISettingsBase
    {
        Bam.Core.EPlatform VC12CxxOnly
        {
            get;
            set;
        }
    }
}

    // V2
    public class CompilerSettings :
        C.V2.SettingsBase,
        C.V2.ICommonCompilerOptions,
        C.V2.ICOnlyCompilerOptions,
        VisualCCommon.V2.ICommonCompilerOptions,
        //VisualCCommon.V2.ICOnlyCompilerOptions,
        //VisualC.V2.ICommonCompilerOptions,
        //VisualC.V2.ICOnlyCompilerOptions,
        CommandLineProcessor.V2.IConvertToCommandLine,
        VisualStudioProcessor.V2.IConvertToProject
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
            (this as C.V2.ICommonCompilerOptions).Empty();
            if (useDefaults)
            {
                (this as C.V2.ICommonCompilerOptions).Defaults(module);
            }
#endif
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

        bool? VisualCCommon.V2.ICommonCompilerOptions.NoLogo
        {
            get;
            set;
        }

#if false
        int VisualCCommon.V2.ICOnlyCompilerOptions.VCCommonCOnly
        {
            get;
            set;
        }

        bool V2.ICommonCompilerOptions.VC12Common
        {
            get;
            set;
        }

        int V2.ICOnlyCompilerOptions.VC12COnly
        {
            get;
            set;
        }
#endif

        void
        CommandLineProcessor.V2.IConvertToCommandLine.Convert(
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
            (this as C.V2.ICommonCompilerOptions).Convert(module, commandLine);
            (this as C.V2.ICOnlyCompilerOptions).Convert(module, commandLine);
            (this as VisualCCommon.V2.ICommonCompilerOptions).Convert(module, commandLine);
            //(this as VisualCCommon.V2.ICOnlyCompilerOptions).Convert(module, commandLine);
            //(this as VisualC.V2.ICommonCompilerOptions).Convert(module, commandLine);
            //(this as VisualC.V2.ICOnlyCompilerOptions).Convert(module, commandLine);
        }

        void
        VisualStudioProcessor.V2.IConvertToProject.Convert(
            Bam.Core.V2.Module module,
            VSSolutionBuilder.V2.VSSettingsGroup settings,
            string condition)
        {
            (this as C.V2.ICommonCompilerOptions).Convert(module, settings, condition);
            (this as C.V2.ICOnlyCompilerOptions).Convert(module, settings, condition);
            (this as VisualCCommon.V2.ICommonCompilerOptions).Convert(module, settings, condition);
        }
    }

    public sealed class CxxCompilerSettings :
        C.V2.SettingsBase,
        CommandLineProcessor.V2.IConvertToCommandLine,
        VisualStudioProcessor.V2.IConvertToProject,
        C.V2.ICommonCompilerOptions,
        C.V2.ICxxOnlyCompilerOptions,
        VisualCCommon.V2.ICommonCompilerOptions
        //VisualCCommon.V2.ICxxOnlyCompilerOptions,
        //VisualC.V2.ICommonCompilerOptions,
        //VisualC.V2.ICxxOnlyCompilerOptions
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
            // TODO: iterate in reflection, in a well defined static class
            (this as C.V2.ICommonCompilerOptions).Convert(module, commandLine);
            (this as C.V2.ICxxOnlyCompilerOptions).Convert(module, commandLine);
            (this as VisualCCommon.V2.ICommonCompilerOptions).Convert(module, commandLine);
#if false
            (this as VisualCCommon.V2.ICxxOnlyCompilerOptions).Convert(module, commandLine);
            (this as VisualC.V2.ICommonCompilerOptions).Convert(module, commandLine);
            (this as VisualC.V2.ICxxOnlyCompilerOptions).Convert(module, commandLine);
#endif
        }

        void
        VisualStudioProcessor.V2.IConvertToProject.Convert(
            Bam.Core.V2.Module module,
            VSSolutionBuilder.V2.VSSettingsGroup settings,
            string condition)
        {
            (this as C.V2.ICommonCompilerOptions).Convert(module, settings, condition);
            (this as C.V2.ICxxOnlyCompilerOptions).Convert(module, settings, condition);
            (this as VisualCCommon.V2.ICommonCompilerOptions).Convert(module, settings, condition);
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

        bool? VisualCCommon.V2.ICommonCompilerOptions.NoLogo
        {
            get;
            set;
        }

#if false
        string VisualCCommon.V2.ICxxOnlyCompilerOptions.VCCommonCxxOnly
        {
            get;
            set;
        }

        bool V2.ICommonCompilerOptions.VC12Common
        {
            get;
            set;
        }

        Bam.Core.EPlatform V2.ICxxOnlyCompilerOptions.VC12CxxOnly
        {
            get;
            set;
        }
#endif
    }

    public static class Configure
    {
        static Configure()
        {
            InstallPath = Bam.Core.V2.TokenizedString.Create(@"C:\Program Files (x86)\Microsoft Visual Studio 12.0", null);
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
        protected CompilerBase()
        {
            this.InheritedEnvironmentVariables.Add("SystemRoot");
            // temp environment variables avoid generation of _CL_<hex> temporary files in the current directory
            this.InheritedEnvironmentVariables.Add("TEMP");
            this.InheritedEnvironmentVariables.Add("TMP");

            this.Macros.Add("InstallPath", Configure.InstallPath);
            this.Macros.Add("BinPath", Bam.Core.V2.TokenizedString.Create(@"$(InstallPath)\VC\bin", this));
            this.Macros.Add("objext", ".obj");

            this.EnvironmentVariables.Add("PATH", new Bam.Core.V2.TokenizedStringArray(Bam.Core.V2.TokenizedString.Create(@"$(InstallPath)\Common7\IDE", this)));

            this.PublicPatch((settings, appliedTo) =>
                {
                    var compilation = settings as C.V2.ICommonCompilerOptions;
                    compilation.SystemIncludePaths.Add(Bam.Core.V2.TokenizedString.Create(@"$(InstallPath)\VC\include", this));
                });
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

    [C.V2.RegisterCCompiler("VisualC", Bam.Core.EPlatform.Windows, C.V2.EBit.ThirtyTwo)]
    public class Compiler32 :
        CompilerBase
    {
        public Compiler32()
        {
            this.Macros.Add("CompilerPath", Bam.Core.V2.TokenizedString.Create(@"$(BinPath)\cl.exe", this));
        }

        protected override void OverrideDefaultSettings(Bam.Core.V2.Settings settings)
        {
            var cSettings = settings as C.V2.ICommonCompilerOptions;
            cSettings.Bits = C.V2.EBit.ThirtyTwo;
        }
    }

    [C.V2.RegisterCxxCompiler("VisualC", Bam.Core.EPlatform.Windows, C.V2.EBit.ThirtyTwo)]
    public sealed class CxxCompiler32 :
        Compiler32
    {
        public CxxCompiler32()
            : base()
        {
        }

        protected override void OverrideDefaultSettings(Bam.Core.V2.Settings settings)
        {
            base.OverrideDefaultSettings(settings);
            var cSettings = settings as C.V2.ICommonCompilerOptions;
            cSettings.TargetLanguage = C.ETargetLanguage.Cxx;
        }
    }

    [C.V2.RegisterCCompiler("VisualC", Bam.Core.EPlatform.Windows, C.V2.EBit.SixtyFour)]
    public class Compiler64 :
        CompilerBase
    {
        public Compiler64()
            : base()
        {
            this.Macros.Add("CompilerPath", Bam.Core.V2.TokenizedString.Create(@"$(BinPath)\x86_amd64\cl.exe", this));
            // some DLLs exist only in the 32-bit bin folder
            this.EnvironmentVariables["PATH"].Add(this.Macros["BinPath"]);
        }

        protected override void OverrideDefaultSettings(Bam.Core.V2.Settings settings)
        {
            var cSettings = settings as C.V2.ICommonCompilerOptions;
            cSettings.Bits = C.V2.EBit.SixtyFour;
        }
    }

    [C.V2.RegisterCxxCompiler("VisualC", Bam.Core.EPlatform.Windows, C.V2.EBit.SixtyFour)]
    public sealed class CxxCompiler64 :
        Compiler64
    {
        public CxxCompiler64()
            : base()
        {
        }

        protected override void OverrideDefaultSettings(Bam.Core.V2.Settings settings)
        {
            base.OverrideDefaultSettings(settings);
            var cSettings = settings as C.V2.ICommonCompilerOptions;
            cSettings.TargetLanguage = C.ETargetLanguage.Cxx;
        }
    }
    // -V2
    // Not sealed since the C++ compiler inherits from it
    public partial class CCompilerOptionCollection :
        VisualCCommon.CCompilerOptionCollection
    {
        public
        CCompilerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}
    }
}
