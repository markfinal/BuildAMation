#region License
// Copyright 2010-2014 Mark Final
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
#region BamOptionGenerator
// Automatically generated file from BamOptionGenerator.
// Command line arguments:
//     -i=../../../C/dev/Scripts/ICCompilerOptions.cs&ICCompilerOptions.cs
//     -n=ClangCommon
//     -c=CCompilerOptionCollection
//     -p
//     -d
//     -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs&../../../XcodeProjectProcessor/dev/Scripts/Delegate.cs
//     -pv=PrivateData
#endregion // BamOptionGenerator
namespace ClangCommon
{
    public partial class CCompilerOptionCollection
    {
        #region C.ICCompilerOptions Option delegates
        private static void
        DefinesCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var definesOption = option as Bam.Core.ReferenceTypeOption<C.DefineCollection>;
            foreach (var define in definesOption.Value)
            {
                commandLineBuilder.Add(System.String.Format("-D{0}", define));
            }
        }
        private static void
        DefinesXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var definesOption = option as Bam.Core.ReferenceTypeOption<C.DefineCollection>;
            configuration.Options["GCC_PREPROCESSOR_DEFINITIONS"].AddRangeUnique(definesOption.Value.ToStringArray());
        }
        private static void
        IncludePathsCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var toolset = target.Toolset;
            var compiler = toolset.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;
            var switchPrefix = compiler.IncludePathCompilerSwitches[0];
            var includePathsOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.DirectoryCollection>;
            // TODO: convert to 'var'
            foreach (string includePath in includePathsOption.Value)
            {
                if (includePath.Contains(" "))
                {
                    commandLineBuilder.Add(System.String.Format("{0}\"{1}\"", switchPrefix, includePath));
                }
                else
                {
                    commandLineBuilder.Add(System.String.Format("{0}{1}", switchPrefix, includePath));
                }
            }
        }
        private static void
        IncludePathsXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var includePathsOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.DirectoryCollection>;
            configuration.Options["HEADER_SEARCH_PATHS"].AddRangeUnique(includePathsOption.Value.ToStringArray());
        }
        private static void
        SystemIncludePathsCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            IncludePathsCommandLineProcessor(sender, commandLineBuilder, option, target);
        }
        private static void
        SystemIncludePathsXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            IncludePathsXcodeProjectProcessor(sender, project, currentObject, configuration, option, target);
        }
        private static void
        OutputTypeCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var options = sender as CCompilerOptionCollection;
            commandLineBuilder.Add("-c");
            commandLineBuilder.Add("-o");
            var outputPath = options.OwningNode.Module.Locations[C.ObjectFile.OutputFile].GetSinglePath();
            commandLineBuilder.Add(outputPath);
        }
        private static void
        OutputTypeXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            // TODO: not sure what this should do to preprocess files only
        }
        private static void
        DebugSymbolsCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var debugSymbolsOption = option as Bam.Core.ValueTypeOption<bool>;
            if (debugSymbolsOption.Value)
            {
                commandLineBuilder.Add("-g");
            }
            else
            {
                commandLineBuilder.Add("-g0");
            }
        }
        private static void
        DebugSymbolsXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var debugSymbols = option as Bam.Core.ValueTypeOption<bool>;
            var debugSymbolsOption = configuration.Options["GCC_GENERATE_DEBUGGING_SYMBOLS"];
            if (debugSymbols.Value)
            {
                debugSymbolsOption.AddUnique("YES");
            }
            else
            {
                debugSymbolsOption.AddUnique("NO");
            }
            if (debugSymbolsOption.Count != 1)
            {
                throw new Bam.Core.Exception("More than one debug symbol generation option has been set");
            }
        }
        private static void
        WarningsAsErrorsCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var warningsAsErrorsOption = option as Bam.Core.ValueTypeOption<bool>;
            if (warningsAsErrorsOption.Value)
            {
                commandLineBuilder.Add("-Werror");
            }
        }
        private static void
        WarningsAsErrorsXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var warningsAsErrors = option as Bam.Core.ValueTypeOption<bool>;
            var warningsAsErrorsOption = configuration.Options["GCC_TREAT_WARNINGS_AS_ERRORS"];
            if (warningsAsErrors.Value)
            {
                warningsAsErrorsOption.AddUnique("YES");
            }
            else
            {
                warningsAsErrorsOption.AddUnique("NO");
            }
            if (warningsAsErrorsOption.Count != 1)
            {
                throw new Bam.Core.Exception("More than one warnings as errors option has been set");
            }
        }
        private static void
        IgnoreStandardIncludePathsCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var ignoreStandardIncludePathsOption = option as Bam.Core.ValueTypeOption<bool>;
            if (ignoreStandardIncludePathsOption.Value)
            {
                commandLineBuilder.Add("-nostdinc");
                var options = sender as C.ICCompilerOptions;
                if (options.TargetLanguage == C.ETargetLanguage.Cxx)
                {
                    commandLineBuilder.Add("-nostdinc++");
                }
            }
        }
        private static void
        IgnoreStandardIncludePathsXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var ignoreStandardIncludePaths = option as Bam.Core.ValueTypeOption<bool>;
            var otherCFlagsOption = configuration.Options["OTHER_CFLAGS"];
            if (ignoreStandardIncludePaths.Value)
            {
                otherCFlagsOption.AddUnique("-nostdinc");
                var cOptions = sender as C.ICCompilerOptions;
                if (cOptions.TargetLanguage == C.ETargetLanguage.Cxx)
                {
                    otherCFlagsOption.AddUnique("-nostdinc++");
                }
            }
        }
        private static void
        OptimizationCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var optimizationOption = option as Bam.Core.ValueTypeOption<C.EOptimization>;
            switch (optimizationOption.Value)
            {
                case C.EOptimization.Off:
                    commandLineBuilder.Add("-O0");
                    break;
                case C.EOptimization.Size:
                    commandLineBuilder.Add("-Os");
                    break;
                case C.EOptimization.Speed:
                    commandLineBuilder.Add("-O1");
                    break;
                case C.EOptimization.Full:
                    commandLineBuilder.Add("-O3");
                    break;
                case C.EOptimization.Custom:
                    // do nothing
                    break;
                default:
                    throw new Bam.Core.Exception("Unrecognized optimization option");
            }
        }
        private static void
        OptimizationXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var optimization = option as Bam.Core.ValueTypeOption<C.EOptimization>;
            var optimizationOption = configuration.Options["GCC_OPTIMIZATION_LEVEL"];
            switch (optimization.Value)
            {
            case C.EOptimization.Off:
                optimizationOption.AddUnique("0");
                break;
            case C.EOptimization.Size:
                optimizationOption.AddUnique("s");
                break;
            case C.EOptimization.Speed:
                optimizationOption.AddUnique("1");
                break;
            case C.EOptimization.Full:
                optimizationOption.AddUnique("3");
                break;
            case C.EOptimization.Custom:
                // nothing
                break;
            default:
                throw new Bam.Core.Exception("Unrecognized optimization option");
            }
            if (optimizationOption.Count != 1)
            {
                throw new Bam.Core.Exception("More than one optimization option has been set");
            }
        }
        private static void
        CustomOptimizationCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var customOptimizationOption = option as Bam.Core.ReferenceTypeOption<string>;
            commandLineBuilder.Add(customOptimizationOption.Value);
        }
        private static void
        CustomOptimizationXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var customOptimizations = option as Bam.Core.ReferenceTypeOption<string>;
            var otherCFlagsOption = configuration.Options["OTHER_CFLAGS"];
            otherCFlagsOption.AddUnique(customOptimizations.Value);
        }
        private static void
        TargetLanguageCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var targetLanguageOption = option as Bam.Core.ValueTypeOption<C.ETargetLanguage>;
            switch (targetLanguageOption.Value)
            {
            case C.ETargetLanguage.Default:
                // do nothing
                break;
            case C.ETargetLanguage.C:
                commandLineBuilder.Add("-x c");
                break;
            case C.ETargetLanguage.Cxx:
                commandLineBuilder.Add("-x c++");
                break;
            case C.ETargetLanguage.ObjectiveC:
                commandLineBuilder.Add("-x objective-c");
                break;
            case C.ETargetLanguage.ObjectiveCxx:
                commandLineBuilder.Add("-x objective-c++");
                break;
            default:
                throw new Bam.Core.Exception("Unrecognized target language option");
            }
        }
        private static void
        TargetLanguageXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var targetLanguageOption = option as Bam.Core.ValueTypeOption<C.ETargetLanguage>;
            var inputFileType = configuration.Options["GCC_INPUT_FILETYPE"];
            switch (targetLanguageOption.Value)
            {
            case C.ETargetLanguage.Default:
                inputFileType.AddUnique("automatic");
                break;
            case C.ETargetLanguage.C:
                inputFileType.AddUnique("sourcecode.c.c");
                break;
            case C.ETargetLanguage.Cxx:
                inputFileType.AddUnique("sourcecode.cpp.cpp");
                break;
            case C.ETargetLanguage.ObjectiveC:
                inputFileType.AddUnique("sourcecode.c.objc");
                break;
            case C.ETargetLanguage.ObjectiveCxx:
                inputFileType.AddUnique("sourcecode.cpp.objcpp");
                break;
            default:
                throw new Bam.Core.Exception("Unrecognized target language option");
            }
            if (inputFileType.Count != 1)
            {
                throw new Bam.Core.Exception("More than one target language option has been set");
            }
        }
        private static void
        ShowIncludesCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-H");
            }
        }
        private static void
        ShowIncludesXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var showIncludes = option as Bam.Core.ValueTypeOption<bool>;
            var otherCFlagsOption = configuration.Options["OTHER_CFLAGS"];
            if (showIncludes.Value)
            {
                otherCFlagsOption.AddUnique("-H");
            }
        }
        private static void
        AdditionalOptionsCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var stringOption = option as Bam.Core.ReferenceTypeOption<string>;
            var arguments = stringOption.Value.Split(' ');
            foreach (var argument in arguments)
            {
                commandLineBuilder.Add(argument);
            }
        }
        private static void
        AdditionalOptionsXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var additionalOptions = option as Bam.Core.ReferenceTypeOption<string>;
            var splitArguments = additionalOptions.Value.Split(' ');
            var otherCFlagsOption = configuration.Options["OTHER_CFLAGS"];
            foreach (var argument in splitArguments)
            {
                otherCFlagsOption.AddUnique(argument);
            }
        }
        private static void
        OmitFramePointerCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-fomit-frame-pointer");
            }
            else
            {
                commandLineBuilder.Add("-fno-omit-frame-pointer");
            }
        }
        private static void
        OmitFramePointerXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var omitFramePointer = option as Bam.Core.ValueTypeOption<bool>;
            var otherCFlagsOption = configuration.Options["OTHER_CFLAGS"];
            if (omitFramePointer.Value)
            {
                otherCFlagsOption.AddUnique("-fomit-frame-pointer");
            }
            else
            {
                otherCFlagsOption.AddUnique("-fno-omit-frame-pointer");
            }
        }
        private static void
        DisableWarningsCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var disableWarningsOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.StringArray>;
            foreach (var warning in disableWarningsOption.Value)
            {
                commandLineBuilder.Add(System.String.Format("-Wno-{0}", warning));
            }
        }
        private static void
        DisableWarningsXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var disableWarnings = option as Bam.Core.ReferenceTypeOption<Bam.Core.StringArray>;
            var warningCFlagsOption = configuration.Options["WARNING_CFLAGS"];
            foreach (var warning in disableWarnings.Value)
            {
                // TODO: there are some named warnings, e.g.
                // -Wno-shadow = GCC_WARN_SHADOW = NO
                warningCFlagsOption.AddUnique(System.String.Format("-Wno-{0}", warning));
            }
        }
        private static void
        CharacterSetCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var enumOption = option as Bam.Core.ValueTypeOption<C.ECharacterSet>;
            var cOptions = sender as C.ICCompilerOptions;
            switch (enumOption.Value)
            {
                case C.ECharacterSet.NotSet:
                    break;
                case C.ECharacterSet.Unicode:
                    cOptions.Defines.Add("_UNICODE");
                    cOptions.Defines.Add("UNICODE");
                    break;
                case C.ECharacterSet.MultiByte:
                    cOptions.Defines.Add("_MBCS");
                    break;
            }
        }
        private static void
        CharacterSetXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var characterSet = option as Bam.Core.ValueTypeOption<C.ECharacterSet>;
            var cOptions = sender as C.ICCompilerOptions;
            switch (characterSet.Value)
            {
            case C.ECharacterSet.NotSet:
                break;
            case C.ECharacterSet.Unicode:
                cOptions.Defines.Add("_UNICODE");
                cOptions.Defines.Add("UNICODE");
                break;
            case C.ECharacterSet.MultiByte:
                cOptions.Defines.Add("_MBCS");
                break;
            }
        }
        private static void
        LanguageStandardCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var languageStandard = option as Bam.Core.ValueTypeOption<C.ELanguageStandard>;
            switch (languageStandard.Value)
            {
            case C.ELanguageStandard.NotSet:
                break;
            case C.ELanguageStandard.C89:
                commandLineBuilder.Add("-std=c89");
                break;
            case C.ELanguageStandard.C99:
                commandLineBuilder.Add("-std=c99");
                break;
            case C.ELanguageStandard.Cxx98:
                commandLineBuilder.Add("-std=c++98");
                break;
            case C.ELanguageStandard.Cxx11:
                commandLineBuilder.Add("-std=c++11");
                break;
            default:
                throw new Bam.Core.Exception("Unknown language standard");
            }
        }
        private static void
        LanguageStandardXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var languageStandard = option as Bam.Core.ValueTypeOption<C.ELanguageStandard>;
            var languageStandardOption = configuration.Options["GCC_C_LANGUAGE_STANDARD"];
            switch (languageStandard.Value)
            {
            case C.ELanguageStandard.NotSet:
                break;
            case C.ELanguageStandard.C89:
                languageStandardOption.AddUnique("c89");
                break;
            case C.ELanguageStandard.C99:
                languageStandardOption.AddUnique("c99");
                break;
            case C.ELanguageStandard.Cxx98:
                // nothing corresponding
                break;
            case C.ELanguageStandard.Cxx11:
                // nothing corresponding
                break;
            default:
                throw new Bam.Core.Exception("Unknown language standard");
            }
        }
        private static void
        UndefinesCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var undefinesOption = option as Bam.Core.ReferenceTypeOption<C.DefineCollection>;
            foreach (var undefine in undefinesOption.Value)
            {
                commandLineBuilder.Add(System.String.Format("-U{0}", undefine));
            }
        }
        private static void
        UndefinesXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            // TODO
        }
        #endregion
        #region ICCompilerOptions Option delegates
        private static void
        PositionIndependentCodeCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var pic = option as Bam.Core.ValueTypeOption<bool>;
            if (pic.Value)
            {
                commandLineBuilder.Add("-fPIC");
            }
        }
        private static void
        PositionIndependentCodeXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var pic = option as Bam.Core.ValueTypeOption<bool>;
            // note that the logic is reversed here
            var noPICOption = configuration.Options["GCC_DYNAMIC_NO_PIC"];
            if (pic.Value)
            {
                noPICOption.AddUnique("NO");
            }
            else
            {
                noPICOption.AddUnique("YES");
            }
            if (noPICOption.Count != 1)
            {
                throw new Bam.Core.Exception("More than one no position independent code option has been set");
            }
        }
        private static void
        SixtyFourBitCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var sixtyFourBit = option as Bam.Core.ValueTypeOption<bool>;
            if (sixtyFourBit.Value)
            {
                commandLineBuilder.Add("-arch x86_64");
            }
            else
            {
                commandLineBuilder.Add("-arch i386");
            }
        }
        private static void
        SixtyFourBitXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var sixtyFourBit = option as Bam.Core.ValueTypeOption<bool>;
            var archsOption = configuration.Options["ARCHS"];
            var validArchsOption = configuration.Options["VALID_ARCHS"];
            if (sixtyFourBit.Value)
            {
                var toolset = target.Toolset;
                var xcodeDetails = toolset as XcodeBuilder.IXcodeDetails;
                if (null != xcodeDetails && xcodeDetails.SupportedVersion == XcodeBuilder.EXcodeVersion.V4dot6)
                {
                    // Note; this option only required in Xcode 4 - gives a warning in Xcode 5
                    if (0 == archsOption.Count)
                    {
                        archsOption.AddUnique("$(ARCHS_STANDARD_64_BIT)");
                    }
                    else if (1 == archsOption.Count)
                    {
                        archsOption.Remove("$(ARCHS_STANDARD_32_BIT)");
                        archsOption.AddUnique("$(ARCHS_STANDARD_32_64_BIT)");
                    }
                }
                validArchsOption.AddUnique("x86_64");
            }
            else
            {
                // there is no equivalent check for Xcode 4 here
                // even though this will generate a warning on Xcode 5+ for 32-bit builds, I don't see
                // another way of being able to actually compile for 32-bit
                if (0 == archsOption.Count)
                {
                    archsOption.AddUnique("$(ARCHS_STANDARD_32_BIT)");
                }
                else if (1 == archsOption.Count)
                {
                    archsOption.Remove("$(ARCHS_STANDARD_64_BIT)");
                    archsOption.AddUnique("$(ARCHS_STANDARD_32_64_BIT)");
                }
                validArchsOption.AddUnique("i386");
            }
        }
        #endregion
        protected override void
        SetDelegates(
            Bam.Core.DependencyNode node)
        {
            this["Defines"].PrivateData = new PrivateData(DefinesCommandLineProcessor,DefinesXcodeProjectProcessor);
            this["IncludePaths"].PrivateData = new PrivateData(IncludePathsCommandLineProcessor,IncludePathsXcodeProjectProcessor);
            this["SystemIncludePaths"].PrivateData = new PrivateData(SystemIncludePathsCommandLineProcessor,SystemIncludePathsXcodeProjectProcessor);
            this["OutputType"].PrivateData = new PrivateData(OutputTypeCommandLineProcessor,OutputTypeXcodeProjectProcessor);
            this["DebugSymbols"].PrivateData = new PrivateData(DebugSymbolsCommandLineProcessor,DebugSymbolsXcodeProjectProcessor);
            this["WarningsAsErrors"].PrivateData = new PrivateData(WarningsAsErrorsCommandLineProcessor,WarningsAsErrorsXcodeProjectProcessor);
            this["IgnoreStandardIncludePaths"].PrivateData = new PrivateData(IgnoreStandardIncludePathsCommandLineProcessor,IgnoreStandardIncludePathsXcodeProjectProcessor);
            this["Optimization"].PrivateData = new PrivateData(OptimizationCommandLineProcessor,OptimizationXcodeProjectProcessor);
            this["CustomOptimization"].PrivateData = new PrivateData(CustomOptimizationCommandLineProcessor,CustomOptimizationXcodeProjectProcessor);
            this["TargetLanguage"].PrivateData = new PrivateData(TargetLanguageCommandLineProcessor,TargetLanguageXcodeProjectProcessor);
            this["ShowIncludes"].PrivateData = new PrivateData(ShowIncludesCommandLineProcessor,ShowIncludesXcodeProjectProcessor);
            this["AdditionalOptions"].PrivateData = new PrivateData(AdditionalOptionsCommandLineProcessor,AdditionalOptionsXcodeProjectProcessor);
            this["OmitFramePointer"].PrivateData = new PrivateData(OmitFramePointerCommandLineProcessor,OmitFramePointerXcodeProjectProcessor);
            this["DisableWarnings"].PrivateData = new PrivateData(DisableWarningsCommandLineProcessor,DisableWarningsXcodeProjectProcessor);
            this["CharacterSet"].PrivateData = new PrivateData(CharacterSetCommandLineProcessor,CharacterSetXcodeProjectProcessor);
            this["LanguageStandard"].PrivateData = new PrivateData(LanguageStandardCommandLineProcessor,LanguageStandardXcodeProjectProcessor);
            this["Undefines"].PrivateData = new PrivateData(UndefinesCommandLineProcessor,UndefinesXcodeProjectProcessor);
            this["PositionIndependentCode"].PrivateData = new PrivateData(PositionIndependentCodeCommandLineProcessor,PositionIndependentCodeXcodeProjectProcessor);
            this["SixtyFourBit"].PrivateData = new PrivateData(SixtyFourBitCommandLineProcessor,SixtyFourBitXcodeProjectProcessor);
        }
    }
}
