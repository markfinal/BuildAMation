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

namespace MingwCommon
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
        IncludePathsCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var toolset = target.Toolset;
            var compiler = toolset.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;
            var switchPrefix = compiler.IncludePathCompilerSwitches[1];
            var includePathsOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.DirectoryCollection>;
            // TODO: convert to var
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
        SystemIncludePathsCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var optionCollection = sender as C.ICCompilerOptions;
            if (!optionCollection.IgnoreStandardIncludePaths)
            {
                Bam.Core.Log.Full("System include paths not explicitly added to the build");
                return;
            }
            var toolset = target.Toolset;
            var compiler = toolset.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;
            var switchPrefix = compiler.IncludePathCompilerSwitches[0];
            var includePathsOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.DirectoryCollection>;
            // TODO: convert to var
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
        OutputTypeCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var options = sender as CCompilerOptionCollection;
            var enumOption = option as Bam.Core.ValueTypeOption<C.ECompilerOutput>;
            switch (enumOption.Value)
            {
                case C.ECompilerOutput.CompileOnly:
                    {
                        commandLineBuilder.Add("-c");
                        var outputPath = options.OwningNode.Module.Locations[C.ObjectFile.OutputFile].GetSinglePath();
                        commandLineBuilder.Add(System.String.Format("-o {0}", outputPath));
                    }
                    break;
                case C.ECompilerOutput.Preprocess:
                    {
                        commandLineBuilder.Add("-E");
                        var outputPath = options.OwningNode.Module.Locations[C.ObjectFile.OutputFile].GetSinglePath();
                        commandLineBuilder.Add(System.String.Format("-o {0}", outputPath));
                    }
                    break;
                default:
                    throw new Bam.Core.Exception("Unrecognized option for C.ECompilerOutput");
            }
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
                default:
                    throw new Bam.Core.Exception("Unrecognized target language option");
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
        LanguageStandardCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
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
        #endregion
        #region ICCompilerOptions Option delegates
        private static void
        AllWarningsCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-Wall");
            }
        }
        private static void
        ExtraWarningsCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-Wextra");
            }
        }
        private static void
        StrictAliasingCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-fstrict-aliasing");
            }
            else
            {
                commandLineBuilder.Add("-fno-strict-aliasing");
            }
        }
        private static void
        InlineFunctionsCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-finline-functions");
            }
            else
            {
                commandLineBuilder.Add("-fno-inline-functions");
            }
        }
        private static void
        PedanticCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-pedantic");
            }
        }
        private static void
        SixtyFourBitCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var sixtyFourBitOption = option as Bam.Core.ValueTypeOption<bool>;
            if (sixtyFourBitOption.Value)
            {
                commandLineBuilder.Add("-m64");
            }
            else
            {
                commandLineBuilder.Add("-m32");
            }
        }
        #endregion
        protected override void
        SetDelegates(
            Bam.Core.DependencyNode node)
        {
            this["Defines"].PrivateData = new PrivateData(DefinesCommandLineProcessor);
            this["IncludePaths"].PrivateData = new PrivateData(IncludePathsCommandLineProcessor);
            this["SystemIncludePaths"].PrivateData = new PrivateData(SystemIncludePathsCommandLineProcessor);
            this["OutputType"].PrivateData = new PrivateData(OutputTypeCommandLineProcessor);
            this["DebugSymbols"].PrivateData = new PrivateData(DebugSymbolsCommandLineProcessor);
            this["WarningsAsErrors"].PrivateData = new PrivateData(WarningsAsErrorsCommandLineProcessor);
            this["IgnoreStandardIncludePaths"].PrivateData = new PrivateData(IgnoreStandardIncludePathsCommandLineProcessor);
            this["Optimization"].PrivateData = new PrivateData(OptimizationCommandLineProcessor);
            this["CustomOptimization"].PrivateData = new PrivateData(CustomOptimizationCommandLineProcessor);
            this["TargetLanguage"].PrivateData = new PrivateData(TargetLanguageCommandLineProcessor);
            this["ShowIncludes"].PrivateData = new PrivateData(ShowIncludesCommandLineProcessor);
            this["AdditionalOptions"].PrivateData = new PrivateData(AdditionalOptionsCommandLineProcessor);
            this["OmitFramePointer"].PrivateData = new PrivateData(OmitFramePointerCommandLineProcessor);
            this["DisableWarnings"].PrivateData = new PrivateData(DisableWarningsCommandLineProcessor);
            this["CharacterSet"].PrivateData = new PrivateData(CharacterSetCommandLineProcessor);
            this["LanguageStandard"].PrivateData = new PrivateData(LanguageStandardCommandLineProcessor);
            this["Undefines"].PrivateData = new PrivateData(UndefinesCommandLineProcessor);
            this["AllWarnings"].PrivateData = new PrivateData(AllWarningsCommandLineProcessor);
            this["ExtraWarnings"].PrivateData = new PrivateData(ExtraWarningsCommandLineProcessor);
            this["StrictAliasing"].PrivateData = new PrivateData(StrictAliasingCommandLineProcessor);
            this["InlineFunctions"].PrivateData = new PrivateData(InlineFunctionsCommandLineProcessor);
            this["Pedantic"].PrivateData = new PrivateData(PedanticCommandLineProcessor);
            this["SixtyFourBit"].PrivateData = new PrivateData(SixtyFourBitCommandLineProcessor);
        }
    }
}
