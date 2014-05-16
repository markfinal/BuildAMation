// Automatically generated file from OpusOptionCodeGenerator.
// Command line:
// -i=../../../C/dev/Scripts/ICCompilerOptions.cs;ICCompilerOptions.cs -n=VisualCCommon -c=CCompilerOptionCollection -p -d -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs;../../../VisualStudioProcessor/dev/Scripts/VisualStudioDelegate.cs -pv=PrivateData

namespace VisualCCommon
{
    public partial class CCompilerOptionCollection
    {
        #region C.ICCompilerOptions Option delegates
        private static void DefinesCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var definesOption = option as Opus.Core.ReferenceTypeOption<C.DefineCollection>;
            foreach (var define in definesOption.Value)
            {
                commandLineBuilder.Add(System.String.Format("-D{0}", define));
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary DefinesVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var definesOption = option as Opus.Core.ReferenceTypeOption<C.DefineCollection>;
            var defines = new System.Text.StringBuilder();
            foreach (var define in definesOption.Value)
            {
                defines.AppendFormat("{0};", define);
            }
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("PreprocessorDefinitions", defines.ToString());
            return returnVal;
        }
        private static void IncludePathsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var compilerTool = target.Toolset.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;
            var switchPrefix = compilerTool.IncludePathCompilerSwitches[0];
            var includePathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            // TODO: convert to var
            // TODO: obtain Locations from this instead of strings- also removes need to check for spaces
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
        private static VisualStudioProcessor.ToolAttributeDictionary IncludePathsVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var includePathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            var includePaths = new System.Text.StringBuilder();
            // TODO: convert to var, return Locations instead of paths, remove need to check for space
            foreach (string includePath in includePathsOption.Value)
            {
                if (includePath.Contains(" "))
                {
                    includePaths.AppendFormat("\"{0}\";", includePath);
                }
                else
                {
                    includePaths.AppendFormat("{0};", includePath);
                }
            }
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("AdditionalIncludeDirectories", includePaths.ToString());
            return returnVal;
        }
        private static void SystemIncludePathsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var optionCollection = sender as C.ICCompilerOptions;
            if (!optionCollection.IgnoreStandardIncludePaths)
            {
                Opus.Core.Log.Full("System include paths not explicitly added to the build");
                return;
            }
            var compilerTool = target.Toolset.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;
            var switchPrefix = compilerTool.IncludePathCompilerSwitches[0];
            var includePathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            // TODO: convert to var, convert to return Location
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
        private static VisualStudioProcessor.ToolAttributeDictionary SystemIncludePathsVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            var optionCollection = sender as C.ICCompilerOptions;
            if (!optionCollection.IgnoreStandardIncludePaths)
            {
                Opus.Core.Log.Full("System include paths not explicitly added to the build");
                return returnVal;
            }
            var includePathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            var includePaths = new System.Text.StringBuilder();
            // TODO: convert to var, convert to returning Locations
            foreach (string includePath in includePathsOption.Value)
            {
                if (includePath.Contains(" "))
                {
                    includePaths.AppendFormat("\"{0}\";", includePath);
                }
                else
                {
                    includePaths.AppendFormat("{0};", includePath);
                }
            }
            returnVal.Add("AdditionalIncludeDirectories", includePaths.ToString());
            return returnVal;
        }
        private static void OutputTypeCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
#if true
            var outputFileLoc = (sender as Opus.Core.BaseOptionCollection).OwningNode.Module.Locations[C.ObjectFile.OutputFile];
            var enumOption = option as Opus.Core.ValueTypeOption<C.ECompilerOutput>;
            switch (enumOption.Value)
            {
                case C.ECompilerOutput.CompileOnly:
                    {
                        commandLineBuilder.Add("-c");
                        var objPathName = outputFileLoc.GetSinglePath();
                        commandLineBuilder.Add(System.String.Format("-Fo{0}", objPathName));
                    }
                    break;
                case C.ECompilerOutput.Preprocess: // with line numbers
                    {
                        commandLineBuilder.Add("-P");
                        var objPathName = outputFileLoc.GetSinglePath();
                        commandLineBuilder.Add(System.String.Format("-Fo{0}", objPathName));
                    }
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized option for C.ECompilerOutput");
            }
#else
            var options = sender as CCompilerOptionCollection;
            if (null == options.ObjectFilePath)
            {
                return;
            }
            Opus.Core.ValueTypeOption<C.ECompilerOutput> enumOption = option as Opus.Core.ValueTypeOption<C.ECompilerOutput>;
            switch (enumOption.Value)
            {
                case C.ECompilerOutput.CompileOnly:
                    {
                        commandLineBuilder.Add("-c");
                        string objPathName = options.ObjectFilePath;
                        if (objPathName.Contains(" "))
                        {
                            commandLineBuilder.Add(System.String.Format("-Fo\"{0}\"", objPathName));
                        }
                        else
                        {
                            commandLineBuilder.Add(System.String.Format("-Fo{0}", objPathName));
                        }
                    }
                    break;
                case C.ECompilerOutput.Preprocess: // with line numbers
                    {
                        commandLineBuilder.Add("-P");
                        string objPathName = options.PreprocessedFilePath;
                        if (objPathName.Contains(" "))
                        {
                            commandLineBuilder.Add(System.String.Format("-Fo\"{0}\"", objPathName));
                        }
                        else
                        {
                            commandLineBuilder.Add(System.String.Format("-Fo{0}", objPathName));
                        }
                    }
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized option for C.ECompilerOutput");
            }
#endif
        }
        private static VisualStudioProcessor.ToolAttributeDictionary OutputTypeVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
#if true
            var outputFileLoc = (sender as Opus.Core.BaseOptionCollection).OwningNode.Module.Locations[C.ObjectFile.OutputFile];
            if (!outputFileLoc.IsValid)
            {
                return null;
            }
            var processOption = option as Opus.Core.ValueTypeOption<C.ECompilerOutput>;
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
                returnVal.Add ("GeneratePreprocessedFile", processOption.Value.ToString ("D"));
                switch (processOption.Value)
                {
                case C.ECompilerOutput.CompileOnly:
                    {
                        var objPathName = outputFileLoc.GetSinglePath();
                        returnVal.Add("ObjectFile", objPathName);
                        return returnVal;
                    }
                case C.ECompilerOutput.Preprocess:
                    {
                        var objPathName = outputFileLoc.GetSinglePath();
                        returnVal.Add("ObjectFile", objPathName);
                        return returnVal;
                    }
                default:
                    throw new Opus.Core.Exception("Unrecognized option for C.ECompilerOutput");
                }
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
                switch (processOption.Value)
                {
                    case C.ECompilerOutput.CompileOnly:
                        {
                            returnVal.Add("PreprocessToFile", "false");
                            var objPathName = outputFileLoc.GetSinglePath();
                            returnVal.Add("ObjectFileName", objPathName);
                        }
                        break;
                    case C.ECompilerOutput.Preprocess:
                        {
                            returnVal.Add("PreprocessToFile", "true");
                            var objPathName = outputFileLoc.GetSinglePath();
                            returnVal.Add("ObjectFileName", objPathName);
                        }
                        break;
                    default:
                        throw new Opus.Core.Exception("Unrecognized option for C.ECompilerOutput");
                }
                return returnVal;
            }
            return null;
#else
            var processOption = option as Opus.Core.ValueTypeOption<C.ECompilerOutput>;
            var options = sender as CCompilerOptionCollection;
            if (null == options.ObjectFilePath)
            {
                return null;
            }
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                VisualStudioProcessor.ToolAttributeDictionary returnVal = new VisualStudioProcessor.ToolAttributeDictionary ();
                returnVal.Add ("GeneratePreprocessedFile", processOption.Value.ToString ("D"));
                switch (processOption.Value)
                {
                case C.ECompilerOutput.CompileOnly:
                    {
                        returnVal.Add ("ObjectFile", options.ObjectFilePath);
                        return returnVal;
                    }
                case C.ECompilerOutput.Preprocess:
                    {
                        returnVal.Add ("ObjectFile", options.PreprocessedFilePath);
                        return returnVal;
                    }
                default:
                    throw new Opus.Core.Exception("Unrecognized option for C.ECompilerOutput");
                }
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                VisualStudioProcessor.ToolAttributeDictionary returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
                switch (processOption.Value)
                {
                    case C.ECompilerOutput.CompileOnly:
                        {
                            returnVal.Add("PreprocessToFile", "false");
                            returnVal.Add("ObjectFileName", options.ObjectFilePath);
                        }
                        break;
                    case C.ECompilerOutput.Preprocess:
                        {
                            returnVal.Add("PreprocessToFile", "true");
                            returnVal.Add("ObjectFileName", options.PreprocessedFilePath);
                        }
                        break;
                    default:
                        throw new Opus.Core.Exception("Unrecognized option for C.ECompilerOutput");
                }
                return returnVal;
            }
            return null;
#endif
        }
        private static void DebugSymbolsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            // do nothing
        }
        private static VisualStudioProcessor.ToolAttributeDictionary DebugSymbolsVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            // do nothing
            return returnVal;
        }
        private static void WarningsAsErrorsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var warningsAsErrorsOption = option as Opus.Core.ValueTypeOption<bool>;
            if (warningsAsErrorsOption.Value)
            {
                commandLineBuilder.Add("-WX");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary WarningsAsErrorsVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var warningsAsErrorsOption = option as Opus.Core.ValueTypeOption<bool>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                returnVal.Add("WarnAsError", warningsAsErrorsOption.Value.ToString().ToLower());
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                returnVal.Add("TreatWarningAsError", warningsAsErrorsOption.Value.ToString().ToLower());
            }
            return returnVal;
        }
        private static void IgnoreStandardIncludePathsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var includeStandardIncludePathsOption = option as Opus.Core.ValueTypeOption<bool>;
            if (includeStandardIncludePathsOption.Value)
            {
                commandLineBuilder.Add("-X");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary IgnoreStandardIncludePathsVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var includeStandardIncludePathsOption = option as Opus.Core.ValueTypeOption<bool>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("IgnoreStandardIncludePath", includeStandardIncludePathsOption.Value.ToString().ToLower());
            return returnVal;
        }
        private static void OptimizationCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var optimizationOption = option as Opus.Core.ValueTypeOption<C.EOptimization>;
            switch (optimizationOption.Value)
            {
                case C.EOptimization.Off:
                    commandLineBuilder.Add("-Od");
                    break;
                case C.EOptimization.Size:
                    commandLineBuilder.Add("-Os");
                    break;
                case C.EOptimization.Speed:
                    commandLineBuilder.Add("-O1");
                    break;
                case C.EOptimization.Full:
                    commandLineBuilder.Add("-Ox");
                    break;
                case C.EOptimization.Custom:
                    // do nothing
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized optimization option");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary OptimizationVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var optimizationOption = option as Opus.Core.ValueTypeOption<C.EOptimization>;
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                switch (optimizationOption.Value)
                {
                    case C.EOptimization.Off:
                    case C.EOptimization.Size:
                    case C.EOptimization.Speed:
                    case C.EOptimization.Full:
                    case C.EOptimization.Custom:
                        {
                            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
                            returnVal.Add("Optimization", optimizationOption.Value.ToString("D"));
                            return returnVal;
                        }
                    default:
                        throw new Opus.Core.Exception("Unrecognized optimization option");
                }
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
                switch (optimizationOption.Value)
                {
                    case C.EOptimization.Off:
                        returnVal.Add("Optimization", "Disabled");
                        break;
                    case C.EOptimization.Size:
                        returnVal.Add("Optimization", "MinSpace");
                        break;
                    case C.EOptimization.Speed:
                        returnVal.Add("Optimization", "MaxSpeed");
                        break;
                    case C.EOptimization.Full:
                        returnVal.Add("Optimization", "Full");
                        break;
                    case C.EOptimization.Custom:
                        // TODO: does this need something?
                        break;
                    default:
                        throw new Opus.Core.Exception("Unrecognized optimization option");
                }
                return returnVal;
            }
            return null;
        }
        private static void CustomOptimizationCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var customOptimizationOption = option as Opus.Core.ReferenceTypeOption<string>;
            if (!System.String.IsNullOrEmpty(customOptimizationOption.Value))
            {
                commandLineBuilder.Add(customOptimizationOption.Value);
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary CustomOptimizationVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            // TODO
            return returnVal;
        }
        private static void TargetLanguageCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var targetLanguageOption = option as Opus.Core.ValueTypeOption<C.ETargetLanguage>;
            switch (targetLanguageOption.Value)
            {
                case C.ETargetLanguage.Default:
                    // do nothing
                    break;
                case C.ETargetLanguage.C:
                    commandLineBuilder.Add("-TC");
                    break;
                case C.ETargetLanguage.Cxx:
                    commandLineBuilder.Add("-TP");
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized target language option");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary TargetLanguageVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var targetLanguageOption = option as Opus.Core.ValueTypeOption<C.ETargetLanguage>;
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                switch (targetLanguageOption.Value)
                {
                    case C.ETargetLanguage.Default:
                    case C.ETargetLanguage.C:
                    case C.ETargetLanguage.Cxx:
                        {
                            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
                            returnVal.Add("CompileAs", targetLanguageOption.Value.ToString("D"));
                            return returnVal;
                        }
                    default:
                        throw new Opus.Core.Exception("Unrecognized target language option");
                }
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
                switch (targetLanguageOption.Value)
                {
                    case C.ETargetLanguage.Default:
                        returnVal.Add("CompileAs", "Default");
                        break;
                    case C.ETargetLanguage.C:
                        returnVal.Add("CompileAs", "CompileAsC");
                        break;
                    case C.ETargetLanguage.Cxx:
                        returnVal.Add("CompileAs", "CompileAsCpp");
                        break;
                    default:
                        throw new Opus.Core.Exception("Unrecognized target language option");
                }
                return returnVal;
            }
            return null;
        }
        private static void ShowIncludesCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-showIncludes");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary ShowIncludesVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("ShowIncludes", boolOption.Value.ToString().ToLower());
            return returnVal;
        }
        private static void AdditionalOptionsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            var arguments = stringOption.Value.Split(' ');
            foreach (var argument in arguments)
            {
                commandLineBuilder.Add(argument);
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary AdditionalOptionsVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            if (!System.String.IsNullOrEmpty(stringOption.Value))
            {
                returnVal.Add("AdditionalOptions", stringOption.Value);
            }
            return returnVal;
        }
        private static void OmitFramePointerCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-Oy");
            }
            else
            {
                commandLineBuilder.Add("-Oy-");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary OmitFramePointerVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                returnVal.Add("OmitFramePointers", boolOption.Value.ToString());
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                returnVal.Add("OmitFramePointers", boolOption.Value.ToString());
            }
            return returnVal;
        }
        private static void DisableWarningsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var disableWarningsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.StringArray>;
            // TODO: does this need converting to var?
            foreach (string warning in disableWarningsOption.Value)
            {
                commandLineBuilder.Add(System.String.Format("-wd{0}", warning));
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary DisableWarningsVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var disableWarningsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.StringArray>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            var disableWarnings = new System.Text.StringBuilder();
            // TODO: convert to var?
            foreach (string warning in disableWarningsOption.Value)
            {
                disableWarnings.AppendFormat("{0};", warning);
            }
            returnVal.Add("DisableSpecificWarnings", disableWarnings.ToString());
            return returnVal;
        }
        private static void CharacterSetCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var enumOption = option as Opus.Core.ValueTypeOption<C.ECharacterSet>;
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
        private static VisualStudioProcessor.ToolAttributeDictionary CharacterSetVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var enumOption = option as Opus.Core.ValueTypeOption<C.ECharacterSet>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            switch (enumOption.Value)
            {
                case C.ECharacterSet.NotSet:
                    break;
                case C.ECharacterSet.Unicode:
                    break;
                case C.ECharacterSet.MultiByte:
                    break;
            }
            return returnVal;
        }
        private static void LanguageStandardCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static VisualStudioProcessor.ToolAttributeDictionary LanguageStandardVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            return returnVal;
        }
        private static void UndefinesCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var undefinesOption = option as Opus.Core.ReferenceTypeOption<C.DefineCollection>;
            // TODO: convert to var?
            foreach (string undefine in undefinesOption.Value)
            {
                commandLineBuilder.Add(System.String.Format("-U{0}", undefine));
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary UndefinesVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var undefinesOption = option as Opus.Core.ReferenceTypeOption<C.DefineCollection>;
            var undefines = new System.Text.StringBuilder();
            // TODO: convert to var?
            foreach (string define in undefinesOption.Value)
            {
                undefines.AppendFormat("{0};", define);
            }
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("UndefinePreprocessorDefinitions", undefines.ToString());
            return returnVal;
        }
        #endregion
        #region ICCompilerOptions Option delegates
        private static void NoLogoCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var noLogoOption = option as Opus.Core.ValueTypeOption<bool>;
            if (noLogoOption.Value)
            {
                commandLineBuilder.Add("-nologo");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary NoLogoVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var noLogoOption = option as Opus.Core.ValueTypeOption<bool>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("SuppressStartupBanner", noLogoOption.Value.ToString().ToLower());
            return returnVal;
        }
        private static void MinimalRebuildCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var optionCollection = sender as C.ICCompilerOptions;
            var vcOptionCollection = sender as ICCompilerOptions;
            var minimalRebuildOption = option as Opus.Core.ValueTypeOption<bool>;
            if (minimalRebuildOption.Value &&
                optionCollection.DebugSymbols &&
                (EManagedCompilation.NoCLR == vcOptionCollection.CompileAsManaged) &&
                ((EDebugType.ProgramDatabase == vcOptionCollection.DebugType) || (EDebugType.ProgramDatabaseEditAndContinue == vcOptionCollection.DebugType)))
            {
                commandLineBuilder.Add("-Gm");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary MinimalRebuildVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var optionCollection = sender as C.ICCompilerOptions;
            var vcOptionCollection = sender as ICCompilerOptions;
            var minimalRebuildOption = option as Opus.Core.ValueTypeOption<bool>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            var attributeName = "MinimalRebuild";
            if (minimalRebuildOption.Value &&
                optionCollection.DebugSymbols &&
                (EManagedCompilation.NoCLR == vcOptionCollection.CompileAsManaged) &&
                ((EDebugType.ProgramDatabase == vcOptionCollection.DebugType) || (EDebugType.ProgramDatabaseEditAndContinue == vcOptionCollection.DebugType)))
            {
                returnVal.Add(attributeName, "true");
            }
            else
            {
                returnVal.Add(attributeName, "false");
            }
            return returnVal;
        }
        private static void WarningLevelCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var enumOption = option as Opus.Core.ValueTypeOption<EWarningLevel>;
            commandLineBuilder.Add(System.String.Format("-W{0}", (int)enumOption.Value));
        }
        private static VisualStudioProcessor.ToolAttributeDictionary WarningLevelVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var enumOption = option as Opus.Core.ValueTypeOption<EWarningLevel>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                returnVal.Add("WarningLevel", enumOption.Value.ToString("D"));
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                if (enumOption.Value == EWarningLevel.Level0)
                {
                    returnVal.Add("WarningLevel", "TurnOffAllWarnings");
                }
                else
                {
                    returnVal.Add("WarningLevel", System.String.Format("Level{0}", enumOption.Value.ToString("D")));
                }
            }
            return returnVal;
        }
        private static void DebugTypeCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
#if true
            var options = sender as C.ICCompilerOptions;
            if (options.DebugSymbols)
            {
                var localOptions = sender as ICCompilerOptions;
                switch (localOptions.DebugType)
                {
                    case EDebugType.Embedded:
                        commandLineBuilder.Add("-Z7");
                        break;

                    // TODO: more case statements with PDBs...

                    default:
                        throw new Opus.Core.Exception("Unrecognized value for VisualC.EDebugType");
                }
            }
#else
            var options = sender as CCompilerOptionCollection;
            if ((sender as C.ICCompilerOptions).DebugSymbols)
            {
                switch ((sender as ICCompilerOptions).DebugType)
                {
                    case EDebugType.Embedded:
                        commandLineBuilder.Add("-Z7");
                        break;
                    case EDebugType.ProgramDatabase:
                        {
                            commandLineBuilder.Add("-Zi");
                            string pdbPathName = options.ProgramDatabaseFilePath;
                            if (null == pdbPathName)
                            {
                                throw new Opus.Core.Exception("PDB file path has not been set");
                            }
                            if (pdbPathName.Contains(" "))
                            {
                                commandLineBuilder.Add(System.String.Format("-Fd\"{0}\"", pdbPathName));
                            }
                            else
                            {
                                commandLineBuilder.Add(System.String.Format("-Fd{0}", pdbPathName));
                            }
                        }
                        break;
                    case EDebugType.ProgramDatabaseEditAndContinue:
                        {
                            commandLineBuilder.Add("-ZI");
                            string pdbPathName = options.ProgramDatabaseFilePath;
                            if (null == pdbPathName)
                            {
                                throw new Opus.Core.Exception("PDB file path has not been set");
                            }
                            if (pdbPathName.Contains(" "))
                            {
                                commandLineBuilder.Add(System.String.Format("-Fd\"{0}\"", pdbPathName));
                            }
                            else
                            {
                                commandLineBuilder.Add(System.String.Format("-Fd{0}", pdbPathName));
                            }
                        }
                        break;
                    default:
                        throw new Opus.Core.Exception("Unrecognized value for VisualC.EDebugType");
                }
            }
#endif
        }
        private static VisualStudioProcessor.ToolAttributeDictionary DebugTypeVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
#if true
            var attributeName = "DebugInformationFormat";
            if (!(sender as C.ICCompilerOptions).DebugSymbols)
            {
                return null;
            }

            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                switch ((sender as ICCompilerOptions).DebugType)
                {
                    case EDebugType.Embedded:
                        returnVal.Add(attributeName, (sender as ICCompilerOptions).DebugType.ToString("D"));
                        break;

#if true
                        // TODO: handle PDBs
#else
                    case EDebugType.ProgramDatabase:
                    case EDebugType.ProgramDatabaseEditAndContinue:
                        returnVal.Add(attributeName, (sender as ICCompilerOptions).DebugType.ToString("D"));
                        returnVal.Add("ProgramDataBaseFileName", options.ProgramDatabaseFilePath);
                        break;
#endif

                    default:
                        throw new Opus.Core.Exception("Unrecognized value for VisualC.EDebugType");
                }
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                switch ((sender as ICCompilerOptions).DebugType)
                {
                    case EDebugType.Embedded:
                        returnVal.Add("DebugInformationFormat", "OldStyle");
                        break;
#if true
                        // TODO: handle PDBs
#else
                    case EDebugType.ProgramDatabase:
                        returnVal.Add("DebugInformationFormat", "ProgramDatabase");
                        returnVal.Add("ProgramDataBaseFileName", options.ProgramDatabaseFilePath);
                        break;
                    case EDebugType.ProgramDatabaseEditAndContinue:
                        returnVal.Add("DebugInformationFormat", "EditAndContinue");
                        returnVal.Add("ProgramDataBaseFileName", options.ProgramDatabaseFilePath);
                        break;
#endif
                    default:
                        throw new Opus.Core.Exception("Unrecognized value for VisualC.EDebugType");
                }
            }
            return returnVal;
#else
            var options = sender as CCompilerOptionCollection;
            var attributeName = "DebugInformationFormat";
            if ((sender as C.ICCompilerOptions).DebugSymbols)
            {
                var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
                if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
                {
                    switch ((sender as ICCompilerOptions).DebugType)
                    {
                        case EDebugType.Embedded:
                            returnVal.Add(attributeName, (sender as ICCompilerOptions).DebugType.ToString("D"));
                            break;
                        case EDebugType.ProgramDatabase:
                        case EDebugType.ProgramDatabaseEditAndContinue:
                            returnVal.Add(attributeName, (sender as ICCompilerOptions).DebugType.ToString("D"));
                            returnVal.Add("ProgramDataBaseFileName", options.ProgramDatabaseFilePath);
                            break;
                        default:
                            throw new Opus.Core.Exception("Unrecognized value for VisualC.EDebugType");
                    }
                }
                else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
                {
                    switch ((sender as ICCompilerOptions).DebugType)
                    {
                        case EDebugType.Embedded:
                            returnVal.Add("DebugInformationFormat", "OldStyle");
                            break;
                        case EDebugType.ProgramDatabase:
                            returnVal.Add("DebugInformationFormat", "ProgramDatabase");
                            returnVal.Add("ProgramDataBaseFileName", options.ProgramDatabaseFilePath);
                            break;
                        case EDebugType.ProgramDatabaseEditAndContinue:
                            returnVal.Add("DebugInformationFormat", "EditAndContinue");
                            returnVal.Add("ProgramDataBaseFileName", options.ProgramDatabaseFilePath);
                            break;
                        default:
                            throw new Opus.Core.Exception("Unrecognized value for VisualC.EDebugType");
                    }
                }
                return returnVal;
            }
            else
            {
                return null;
            }
#endif
        }
        private static void BrowseInformationCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var enumOption = option as Opus.Core.ValueTypeOption<EBrowseInformation>;
            var browseDir = (sender as Opus.Core.BaseOptionCollection).OwningNode.Module.Locations[C.ObjectFile.OutputDir].GetSinglePath();
            switch (enumOption.Value)
            {
                case EBrowseInformation.None:
                    // do nothing
                    break;
                case EBrowseInformation.Full:
                    commandLineBuilder.Add(System.String.Format("-FR{0}", browseDir));
                    break;
                case EBrowseInformation.NoLocalSymbols:
                    commandLineBuilder.Add(System.String.Format("-Fr{0}", browseDir));
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized EBrowseInformation option");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary BrowseInformationVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
#if true
            var enumOption = option as Opus.Core.ValueTypeOption<EBrowseInformation>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            var browseLocDir = (sender as Opus.Core.BaseOptionCollection).OwningNode.Module.Locations[C.ObjectFile.OutputDir];
            // the trailing directory separator is important, or unexpected rebuilds occur
            var browseDir = browseLocDir.IsValid ? browseLocDir.GetSinglePath() + "\\" : string.Empty;
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                returnVal.Add("BrowseInformation", enumOption.Value.ToString("D"));
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                switch (enumOption.Value)
                {
                    case EBrowseInformation.None:
                        returnVal.Add("BrowseInformation", "false");
                        break;
                    // TODO: there does not appear to be a different set of values in MSBUILD
                    case EBrowseInformation.Full:
                    case EBrowseInformation.NoLocalSymbols:
                        returnVal.Add("BrowseInformation", "true");
                        break;
                    default:
                        throw new Opus.Core.Exception("Unrecognized EBrowseInformation option");
                }
            }
            returnVal.Add("BrowseInformationFile", browseDir);
            return returnVal;
#else
            var enumOption = option as Opus.Core.ValueTypeOption<EBrowseInformation>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            var options = sender as C.CompilerOptionCollection;
            // the trailing directory separator is important, or unexpected rebuilds occur
            var browseDir = !string.IsNullOrEmpty(options.OutputDirectoryPath) ? options.OutputDirectoryPath + "\\" : string.Empty;
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                returnVal.Add("BrowseInformation", enumOption.Value.ToString("D"));
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                switch (enumOption.Value)
                {
                    case EBrowseInformation.None:
                        returnVal.Add("BrowseInformation", "false");
                        break;
                    // TODO: there does not appear to be a different set of values in MSBUILD
                    case EBrowseInformation.Full:
                    case EBrowseInformation.NoLocalSymbols:
                        returnVal.Add("BrowseInformation", "true");
                        break;
                    default:
                        throw new Opus.Core.Exception("Unrecognized EBrowseInformation option");
                }
            }
            returnVal.Add("BrowseInformationFile", browseDir);
            return returnVal;
#endif
        }
        private static void StringPoolingCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-GF");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary StringPoolingVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("StringPooling", boolOption.Value.ToString().ToLower());
            return returnVal;
        }
        private static void DisableLanguageExtensionsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-Za");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary DisableLanguageExtensionsVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("DisableLanguageExtensions", boolOption.Value.ToString().ToLower());
            return returnVal;
        }
        private static void ForceConformanceInForLoopScopeCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-Zc:forScope");
            }
            else
            {
                commandLineBuilder.Add("-Zc:forScope-");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary ForceConformanceInForLoopScopeVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("ForceConformanceInForLoopScope", boolOption.Value.ToString().ToLower());
            return returnVal;
        }
        private static void UseFullPathsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-FC");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary UseFullPathsVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("UseFullPaths", boolOption.Value.ToString().ToLower());
            return returnVal;
        }
        private static void CompileAsManagedCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var enumOption = option as Opus.Core.ValueTypeOption<EManagedCompilation>;
            switch (enumOption.Value)
            {
                case EManagedCompilation.NoCLR:
                    break;
                case EManagedCompilation.CLR:
                    commandLineBuilder.Add("-clr");
                    break;
                case EManagedCompilation.PureCLR:
                    commandLineBuilder.Add("-clr:pure");
                    break;
                case EManagedCompilation.SafeCLR:
                    commandLineBuilder.Add("-clr:safe");
                    break;
                case EManagedCompilation.OldSyntaxCLR:
                    commandLineBuilder.Add("-clr:oldsyntax");
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized EManagedCompilation option");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary CompileAsManagedVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var enumOption = option as Opus.Core.ValueTypeOption<EManagedCompilation>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                returnVal.Add("CompileAsManaged", enumOption.Value.ToString("D"));
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                switch (enumOption.Value)
                {
                    case EManagedCompilation.NoCLR:
                        returnVal.Add("CompileAsManaged", "false");
                        break;
                    case EManagedCompilation.CLR:
                        returnVal.Add("CompileAsManaged", "true");
                        break;
                    case EManagedCompilation.PureCLR:
                        returnVal.Add("CompileAsManaged", "Pure");
                        break;
                    case EManagedCompilation.SafeCLR:
                        returnVal.Add("CompileAsManaged", "Safe");
                        break;
                    case EManagedCompilation.OldSyntaxCLR:
                        returnVal.Add("CompileAsManaged", "OldSyntax");
                        break;
                    default:
                        throw new Opus.Core.Exception("Unrecognized EManagedCompilation option");
                }
            }
            return returnVal;
        }
        private static void BasicRuntimeChecksCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var optionCollection = sender as ICCompilerOptions;
            if (EManagedCompilation.NoCLR != optionCollection.CompileAsManaged)
            {
                return;
            }
            var enumOption = option as Opus.Core.ValueTypeOption<EBasicRuntimeChecks>;
            switch (enumOption.Value)
            {
                case EBasicRuntimeChecks.None:
                    break;
                case EBasicRuntimeChecks.StackFrame:
                    commandLineBuilder.Add("-RTCs");
                    break;
                case EBasicRuntimeChecks.UninitializedVariables:
                    commandLineBuilder.Add("-RTCu");
                    break;
                case EBasicRuntimeChecks.StackFrameAndUninitializedVariables:
                    commandLineBuilder.Add("-RTC1");
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized value for VisualC.EBasicRuntimeChecks");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary BasicRuntimeChecksVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            var optionCollection = sender as ICCompilerOptions;
            if (EManagedCompilation.NoCLR != optionCollection.CompileAsManaged)
            {
                return returnVal;
            }
            var enumOption = option as Opus.Core.ValueTypeOption<EBasicRuntimeChecks>;
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                switch (enumOption.Value)
                {
                    case EBasicRuntimeChecks.None:
                    case EBasicRuntimeChecks.StackFrame:
                    case EBasicRuntimeChecks.UninitializedVariables:
                    case EBasicRuntimeChecks.StackFrameAndUninitializedVariables:
                        returnVal.Add("BasicRuntimeChecks", enumOption.Value.ToString("D"));
                        break;
                    default:
                        throw new Opus.Core.Exception("Unrecognized value for VisualC.EBasicRuntimeChecks");
                }
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                switch (enumOption.Value)
                {
                    case EBasicRuntimeChecks.None:
                        returnVal.Add("BasicRuntimeChecks", "Default");
                        break;
                    case EBasicRuntimeChecks.StackFrame:
                        returnVal.Add("BasicRuntimeChecks", "StackFrameRuntimeCheck");
                        break;
                    case EBasicRuntimeChecks.UninitializedVariables:
                        returnVal.Add("BasicRuntimeChecks", "UninitializedLocalUsageCheck");
                        break;
                    case EBasicRuntimeChecks.StackFrameAndUninitializedVariables:
                        returnVal.Add("BasicRuntimeChecks", "EnableFastChecks");
                        break;
                    default:
                        throw new Opus.Core.Exception("Unrecognized value for VisualC.EBasicRuntimeChecks");
                }
            }
            return returnVal;
        }
        private static void SmallerTypeConversionRuntimeCheckCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var optionCollection = sender as ICCompilerOptions;
            if (EManagedCompilation.NoCLR != optionCollection.CompileAsManaged)
            {
                return;
            }
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-RTCc");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary SmallerTypeConversionRuntimeCheckVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            var optionCollection = sender as ICCompilerOptions;
            if (EManagedCompilation.NoCLR != optionCollection.CompileAsManaged)
            {
                return returnVal;
            }
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                returnVal.Add("SmallerTypeCheck", boolOption.Value.ToString());
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                returnVal.Add("SmallerTypeCheck", boolOption.Value.ToString());
            }
            return returnVal;
        }
        private static void InlineFunctionExpansionCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var enumOption = option as Opus.Core.ValueTypeOption<EInlineFunctionExpansion>;
            switch (enumOption.Value)
            {
                case EInlineFunctionExpansion.None:
                    commandLineBuilder.Add("-Ob0");
                    break;
                case EInlineFunctionExpansion.OnlyInline:
                    commandLineBuilder.Add("-Ob1");
                    break;
                case EInlineFunctionExpansion.AnySuitable:
                    commandLineBuilder.Add("-Ob2");
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized value for VisualC.EInlineFunctionExpansion");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary InlineFunctionExpansionVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var enumOption = option as Opus.Core.ValueTypeOption<EInlineFunctionExpansion>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                switch (enumOption.Value)
                {
                    case EInlineFunctionExpansion.None:
                    case EInlineFunctionExpansion.OnlyInline:
                    case EInlineFunctionExpansion.AnySuitable:
                        returnVal.Add("InlineFunctionExpansion", enumOption.Value.ToString("D"));
                        break;
                    default:
                        throw new Opus.Core.Exception("Unrecognized value for VisualC.EInlineFunctionExpansion");
                }
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                switch (enumOption.Value)
                {
                    case EInlineFunctionExpansion.None:
                        returnVal.Add("InlineFunctionExpansion", "Disabled");
                        break;
                    case EInlineFunctionExpansion.OnlyInline:
                        returnVal.Add("InlineFunctionExpansion", "OnlyExplicitInline");
                        break;
                    case EInlineFunctionExpansion.AnySuitable:
                        returnVal.Add("InlineFunctionExpansion", "AnySuitable");
                        break;
                    default:
                        throw new Opus.Core.Exception("Unrecognized value for VisualC.EInlineFunctionExpansion");
                }
            }
            return returnVal;
        }
        private static void EnableIntrinsicFunctionsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-Oi");
            }
            else
            {
                commandLineBuilder.Add("-Oi-");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary EnableIntrinsicFunctionsVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                returnVal.Add("EnableIntrinsicFunctions", boolOption.Value.ToString());
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                returnVal.Add("IntrinsicFunctions", boolOption.Value.ToString());
            }
            return returnVal;
        }
        private static void RuntimeLibraryCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var runtimeLibraryOption = option as Opus.Core.ValueTypeOption<ERuntimeLibrary>;
            switch (runtimeLibraryOption.Value)
            {
                case ERuntimeLibrary.MultiThreaded:
                    commandLineBuilder.Add("-MT");
                    break;
                case ERuntimeLibrary.MultiThreadedDebug:
                    commandLineBuilder.Add("-MTd");
                    break;
                case ERuntimeLibrary.MultiThreadedDLL:
                    commandLineBuilder.Add("-MD");
                    break;
                case ERuntimeLibrary.MultiThreadedDebugDLL:
                    commandLineBuilder.Add("-MDd");
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized runtime library option");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary RuntimeLibraryVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            var runtimeLibraryOption = option as Opus.Core.ValueTypeOption<ERuntimeLibrary>;
            switch (runtimeLibraryOption.Value)
            {
                case ERuntimeLibrary.MultiThreaded:
                case ERuntimeLibrary.MultiThreadedDebug:
                case ERuntimeLibrary.MultiThreadedDLL:
                case ERuntimeLibrary.MultiThreadedDebugDLL:
                    {
                        if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
                        {
                            returnVal.Add("RuntimeLibrary", runtimeLibraryOption.Value.ToString("D"));
                        }
                        else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
                        {
                            returnVal.Add("RuntimeLibrary", runtimeLibraryOption.Value.ToString());
                        }
                        return returnVal;
                    }
                default:
                    throw new Opus.Core.Exception("Unrecognized runtime library option");
            }
        }
        #endregion
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            this["Defines"].PrivateData = new PrivateData(DefinesCommandLineProcessor,DefinesVisualStudioProcessor);
            this["IncludePaths"].PrivateData = new PrivateData(IncludePathsCommandLineProcessor,IncludePathsVisualStudioProcessor);
            this["SystemIncludePaths"].PrivateData = new PrivateData(SystemIncludePathsCommandLineProcessor,SystemIncludePathsVisualStudioProcessor);
            this["OutputType"].PrivateData = new PrivateData(OutputTypeCommandLineProcessor,OutputTypeVisualStudioProcessor);
            this["DebugSymbols"].PrivateData = new PrivateData(DebugSymbolsCommandLineProcessor,DebugSymbolsVisualStudioProcessor);
            this["WarningsAsErrors"].PrivateData = new PrivateData(WarningsAsErrorsCommandLineProcessor,WarningsAsErrorsVisualStudioProcessor);
            this["IgnoreStandardIncludePaths"].PrivateData = new PrivateData(IgnoreStandardIncludePathsCommandLineProcessor,IgnoreStandardIncludePathsVisualStudioProcessor);
            this["Optimization"].PrivateData = new PrivateData(OptimizationCommandLineProcessor,OptimizationVisualStudioProcessor);
            this["CustomOptimization"].PrivateData = new PrivateData(CustomOptimizationCommandLineProcessor,CustomOptimizationVisualStudioProcessor);
            this["TargetLanguage"].PrivateData = new PrivateData(TargetLanguageCommandLineProcessor,TargetLanguageVisualStudioProcessor);
            this["ShowIncludes"].PrivateData = new PrivateData(ShowIncludesCommandLineProcessor,ShowIncludesVisualStudioProcessor);
            this["AdditionalOptions"].PrivateData = new PrivateData(AdditionalOptionsCommandLineProcessor,AdditionalOptionsVisualStudioProcessor);
            this["OmitFramePointer"].PrivateData = new PrivateData(OmitFramePointerCommandLineProcessor,OmitFramePointerVisualStudioProcessor);
            this["DisableWarnings"].PrivateData = new PrivateData(DisableWarningsCommandLineProcessor,DisableWarningsVisualStudioProcessor);
            this["CharacterSet"].PrivateData = new PrivateData(CharacterSetCommandLineProcessor,CharacterSetVisualStudioProcessor);
            this["LanguageStandard"].PrivateData = new PrivateData(LanguageStandardCommandLineProcessor,LanguageStandardVisualStudioProcessor);
            this["Undefines"].PrivateData = new PrivateData(UndefinesCommandLineProcessor,UndefinesVisualStudioProcessor);
            this["NoLogo"].PrivateData = new PrivateData(NoLogoCommandLineProcessor,NoLogoVisualStudioProcessor);
            this["MinimalRebuild"].PrivateData = new PrivateData(MinimalRebuildCommandLineProcessor,MinimalRebuildVisualStudioProcessor);
            this["WarningLevel"].PrivateData = new PrivateData(WarningLevelCommandLineProcessor,WarningLevelVisualStudioProcessor);
            this["DebugType"].PrivateData = new PrivateData(DebugTypeCommandLineProcessor,DebugTypeVisualStudioProcessor);
            this["BrowseInformation"].PrivateData = new PrivateData(BrowseInformationCommandLineProcessor,BrowseInformationVisualStudioProcessor);
            this["StringPooling"].PrivateData = new PrivateData(StringPoolingCommandLineProcessor,StringPoolingVisualStudioProcessor);
            this["DisableLanguageExtensions"].PrivateData = new PrivateData(DisableLanguageExtensionsCommandLineProcessor,DisableLanguageExtensionsVisualStudioProcessor);
            this["ForceConformanceInForLoopScope"].PrivateData = new PrivateData(ForceConformanceInForLoopScopeCommandLineProcessor,ForceConformanceInForLoopScopeVisualStudioProcessor);
            this["UseFullPaths"].PrivateData = new PrivateData(UseFullPathsCommandLineProcessor,UseFullPathsVisualStudioProcessor);
            this["CompileAsManaged"].PrivateData = new PrivateData(CompileAsManagedCommandLineProcessor,CompileAsManagedVisualStudioProcessor);
            this["BasicRuntimeChecks"].PrivateData = new PrivateData(BasicRuntimeChecksCommandLineProcessor,BasicRuntimeChecksVisualStudioProcessor);
            this["SmallerTypeConversionRuntimeCheck"].PrivateData = new PrivateData(SmallerTypeConversionRuntimeCheckCommandLineProcessor,SmallerTypeConversionRuntimeCheckVisualStudioProcessor);
            this["InlineFunctionExpansion"].PrivateData = new PrivateData(InlineFunctionExpansionCommandLineProcessor,InlineFunctionExpansionVisualStudioProcessor);
            this["EnableIntrinsicFunctions"].PrivateData = new PrivateData(EnableIntrinsicFunctionsCommandLineProcessor,EnableIntrinsicFunctionsVisualStudioProcessor);
            this["RuntimeLibrary"].PrivateData = new PrivateData(RuntimeLibraryCommandLineProcessor,RuntimeLibraryVisualStudioProcessor);
        }
    }
}
