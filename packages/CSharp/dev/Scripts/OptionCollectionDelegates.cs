// Automatically generated file from OpusOptionCodeGenerator.
// Command line arguments:
//     -i=IOptions.cs
//     -n=CSharp
//     -c=OptionCollection
//     -p
//     -d
//     -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs&../../../VisualStudioProcessor/dev/Scripts/VisualStudioDelegate.cs
//     -pv=PrivateData

namespace CSharp
{
    public partial class OptionCollection
    {
        #region IOptions Option delegates
        private static void
        TargetCommandLineProcessor(
             object sender,
             Opus.Core.StringArray commandLineBuilder,
             Opus.Core.Option option,
             Opus.Core.Target target)
        {
            var options = sender as OptionCollection;
            var enumOption = option as Opus.Core.ValueTypeOption<ETarget>;
            switch (enumOption.Value)
            {
                case ETarget.Executable:
                    commandLineBuilder.Add("/target:exe");
                    break;
                case ETarget.Library:
                    commandLineBuilder.Add("/target:library");
                    break;
                case ETarget.Module:
                    commandLineBuilder.Add("/target:module");
                    break;
                case ETarget.WindowsExecutable:
                    commandLineBuilder.Add("/target:winexe");
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized CSharp.ETarget value");
            }
#if true
            var outputPath = options.OwningNode.Module.Locations[CSharp.Assembly.OutputFile].GetSinglePath();
            commandLineBuilder.Add(System.String.Format("/out:{0}", outputPath));
#else
            var outputPathName = options.OutputFilePath;
            if (outputPathName.Contains(" "))
            {
                commandLineBuilder.Add(System.String.Format("/out:\"{0}\"", outputPathName));
            }
            else
            {
                commandLineBuilder.Add(System.String.Format("/out:{0}", outputPathName));
            }
#endif
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        TargetVisualStudioProcessor(
             object sender,
             Opus.Core.Option option,
             Opus.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            var enumOption = option as Opus.Core.ValueTypeOption<ETarget>;
            switch (enumOption.Value)
            {
                case ETarget.Executable:
                    returnVal.Add("OutputType", "Exe");
                    break;
                case ETarget.Library:
                    returnVal.Add("OutputType", "Library");
                    break;
                case ETarget.Module:
                    returnVal.Add("OutputType", "Module");
                    break;
                case ETarget.WindowsExecutable:
                    returnVal.Add("OutputType", "WinExe");
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized CSharp.ETarget value");
            }
            return returnVal;
        }
        private static void
        NoLogoCommandLineProcessor(
             object sender,
             Opus.Core.StringArray commandLineBuilder,
             Opus.Core.Option option,
             Opus.Core.Target target)
        {
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("/nologo");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        NoLogoVisualStudioProcessor(
             object sender,
             Opus.Core.Option option,
             Opus.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            returnVal.Add("NoLogo", boolOption.Value ? "true" : "false");
            return returnVal;
        }
        private static void
        PlatformCommandLineProcessor(
             object sender,
             Opus.Core.StringArray commandLineBuilder,
             Opus.Core.Option option,
             Opus.Core.Target target)
        {
            // no such option for mono-csc
            if (Opus.Core.State.RunningMono)
            {
                return;
            }
            var enumOption = option as Opus.Core.ValueTypeOption<EPlatform>;
            switch (enumOption.Value)
            {
                case EPlatform.X86:
                    commandLineBuilder.Add("/platform:x86");
                    break;
                case EPlatform.X64:
                    commandLineBuilder.Add("/platform:x64");
                    break;
                case EPlatform.Itanium:
                    commandLineBuilder.Add("/platform:Itanium");
                    break;
                case EPlatform.AnyCpu:
                    commandLineBuilder.Add("/platform:AnyCpu");
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized CSharp.EPlatform value");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        PlatformVisualStudioProcessor(
             object sender,
             Opus.Core.Option option,
             Opus.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var enumOption = option as Opus.Core.ValueTypeOption<EPlatform>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            switch (enumOption.Value)
            {
                case EPlatform.X86:
                    returnVal.Add("PlatformTarget", "x86");
                    break;
                case EPlatform.X64:
                    returnVal.Add("PlatformTarget", "x64");
                    break;
                case EPlatform.Itanium:
                    returnVal.Add("PlatformTarget", "Itanium");
                    break;
                case EPlatform.AnyCpu:
                    returnVal.Add("PlatformTarget", "AnyCPU");
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized CSharp.EPlatform value");
            }
            return returnVal;
        }
        private static void
        DebugInformationCommandLineProcessor(
             object sender,
             Opus.Core.StringArray commandLineBuilder,
             Opus.Core.Option option,
             Opus.Core.Target target)
        {
            //var options = sender as OptionCollection;
            var enumOption = option as Opus.Core.ValueTypeOption<EDebugInformation>;
            switch (enumOption.Value)
            {
                case EDebugInformation.Disabled:
                    commandLineBuilder.Add("/debug-");
                    break;
                case EDebugInformation.ProgramDatabaseOnly:
                    {
                        if (Opus.Core.OSUtilities.IsWindowsHosting)
                        {
                            commandLineBuilder.Add("/debug+");
                            commandLineBuilder.Add("/debug:pdbinfo");
#if true
                            // TODO: pdbs
                            var pdbFileLoc = (sender as OptionCollection).OwningNode.Module.Locations[Assembly.PDBFile];
                            commandLineBuilder.Add(System.String.Format("/pdb:{0}", pdbFileLoc.GetSinglePath()));
#else
                            string pdbPathName = options.ProgramDatabaseFilePath;
                            if (pdbPathName.Contains(" "))
                            {
                                commandLineBuilder.Add(System.String.Format("/pdb:\"{0}\"", pdbPathName));
                            }
                            else
                            {
                                commandLineBuilder.Add(System.String.Format("/pdb:{0}", pdbPathName));
                            }
#endif
                        }
                        else
                        {
                            commandLineBuilder.Add("/debug+");
                        }
                    }
                    break;
                case EDebugInformation.Full:
                    {
                        if (Opus.Core.OSUtilities.IsWindowsHosting)
                        {
                            commandLineBuilder.Add("/debug+");
                            commandLineBuilder.Add("/debug:full");
#if true
                            var pdbFileLoc = (sender as OptionCollection).OwningNode.Module.Locations[Assembly.PDBFile];
                            commandLineBuilder.Add(System.String.Format("/pdb:{0}", pdbFileLoc.GetSinglePath()));
#else
                            string pdbPathName = options.ProgramDatabaseFilePath;
                            if (pdbPathName.Contains(" "))
                            {
                                commandLineBuilder.Add(System.String.Format("/pdb:\"{0}\"", pdbPathName));
                            }
                            else
                            {
                                commandLineBuilder.Add(System.String.Format("/pdb:{0}", pdbPathName));
                            }
#endif
                        }
                        else
                        {
                            commandLineBuilder.Add("/debug+");
                        }
                    }
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized CSharp.EDebugInformation value");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        DebugInformationVisualStudioProcessor(
             object sender,
             Opus.Core.Option option,
             Opus.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            var enumOption = option as Opus.Core.ValueTypeOption<EDebugInformation>;
            switch (enumOption.Value)
            {
                case EDebugInformation.Disabled:
                    returnVal.Add("DebugSymbols", "false");
                    break;
                case EDebugInformation.ProgramDatabaseOnly:
                    {
                        if (Opus.Core.OSUtilities.IsWindowsHosting)
                        {
                            returnVal.Add("DebugSymbols", "true");
                            returnVal.Add("DebugType", "pdbinfo");
                            // TODO
                            //var pdbFileLoc = (sender as OptionCollection).OwningNode.Module.Locations[Assembly.PDBFile];
                            //commandLineBuilder.Add(System.String.Format("/pdb:\"{0}\"", pdbFileLoc.GetSinglePath()));
                        }
                        else
                        {
                            returnVal.Add("DebugSymbols", "true");
                        }
                    }
                    break;
                case EDebugInformation.Full:
                    {
                        if (Opus.Core.OSUtilities.IsWindowsHosting)
                        {
                            returnVal.Add("DebugSymbols", "true");
                            returnVal.Add("DebugType", "Full");
                            // TODO
                            //var pdbFileLoc = (sender as OptionCollection).OwningNode.Module.Locations[Assembly.PDBFile];
                            //commandLineBuilder.Add(System.String.Format("/pdb:\"{0}\"", pdbFileLoc.GetSinglePath()));
                        }
                        else
                        {
                            returnVal.Add("DebugSymbols", "true");
                        }
                    }
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized CSharp.EDebugInformation value");
            }
            return returnVal;
        }
        private static void
        CheckedCommandLineProcessor(
             object sender,
             Opus.Core.StringArray commandLineBuilder,
             Opus.Core.Option option,
             Opus.Core.Target target)
        {
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("/checked+");
            }
            else
            {
                commandLineBuilder.Add("/checked-");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        CheckedVisualStudioProcessor(
             object sender,
             Opus.Core.Option option,
             Opus.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("CheckForOverflowUnderflow", boolOption.Value ? "true" : "false");
            return returnVal;
        }
        private static void
        UnsafeCommandLineProcessor(
             object sender,
             Opus.Core.StringArray commandLineBuilder,
             Opus.Core.Option option,
             Opus.Core.Target target)
        {
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("/unsafe+");
            }
            else
            {
                commandLineBuilder.Add("/unsafe-");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        UnsafeVisualStudioProcessor(
             object sender,
             Opus.Core.Option option,
             Opus.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("Unsafe", boolOption.Value ? "true" : "false");
            return returnVal;
        }
        private static void
        WarningLevelCommandLineProcessor(
             object sender,
             Opus.Core.StringArray commandLineBuilder,
             Opus.Core.Option option,
             Opus.Core.Target target)
        {
            var enumOption = option as Opus.Core.ValueTypeOption<EWarningLevel>;
            switch (enumOption.Value)
            {
                case EWarningLevel.Level0:
                case EWarningLevel.Level1:
                case EWarningLevel.Level2:
                case EWarningLevel.Level3:
                case EWarningLevel.Level4:
                    commandLineBuilder.Add(System.String.Format("/warn:{0}", enumOption.Value.ToString("d")));
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized CSharp.EWarningLevel value");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        WarningLevelVisualStudioProcessor(
             object sender,
             Opus.Core.Option option,
             Opus.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var enumOption = option as Opus.Core.ValueTypeOption<EWarningLevel>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            switch (enumOption.Value)
            {
                case EWarningLevel.Level0:
                case EWarningLevel.Level1:
                case EWarningLevel.Level2:
                case EWarningLevel.Level3:
                case EWarningLevel.Level4:
                    returnVal.Add("WarningLevel", enumOption.Value.ToString("d"));
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized CSharp.EWarningLevel value");
            }
            return returnVal;
        }
        private static void
        WarningsAsErrorsCommandLineProcessor(
             object sender,
             Opus.Core.StringArray commandLineBuilder,
             Opus.Core.Option option,
             Opus.Core.Target target)
        {
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("/warnaserror+");
            }
            else
            {
                commandLineBuilder.Add("/warnaserror-");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        WarningsAsErrorsVisualStudioProcessor(
             object sender,
             Opus.Core.Option option,
             Opus.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("TreatWarningsAsErrors", boolOption.Value ? "true" : "false");
            return returnVal;
        }
        private static void
        OptimizeCommandLineProcessor(
             object sender,
             Opus.Core.StringArray commandLineBuilder,
             Opus.Core.Option option,
             Opus.Core.Target target)
        {
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("/optimize+");
            }
            else
            {
                commandLineBuilder.Add("/optimize-");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        OptimizeVisualStudioProcessor(
             object sender,
             Opus.Core.Option option,
             Opus.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("Optimize", boolOption.Value ? "true" : "false");
            return returnVal;
        }
        private static void
        ReferencesCommandLineProcessor(
             object sender,
             Opus.Core.StringArray commandLineBuilder,
             Opus.Core.Option option,
             Opus.Core.Target target)
        {
            var fileCollectionOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.FileCollection>;
            if (fileCollectionOption.Value.Count > 0)
            {
                var fileList = new System.Text.StringBuilder();
                // TODO: convert to var
                foreach (Opus.Core.FileLocation location in fileCollectionOption.Value)
                {
                    var file = location.AbsolutePath;
                    // TODO: need to check whether ; is appropriate here for *nix platforms
                    // It hasn't failed before, but if you are to copy-n-paste an Opus command line
                    // to a shell, would it think that a ; is a continuation character?
                    if (file.Contains(" "))
                    {
                        fileList.AppendFormat("\"{0}\";", file);
                    }
                    else
                    {
                        fileList.AppendFormat("{0};", file);
                    }
                }
                commandLineBuilder.Add(System.String.Format("/reference:{0}", fileList.ToString().TrimEnd(new char[] { ';' })));
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        ReferencesVisualStudioProcessor(
             object sender,
             Opus.Core.Option option,
             Opus.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            // this is handled elsewhere
            return null;
        }
        private static void
        ModulesCommandLineProcessor(
             object sender,
             Opus.Core.StringArray commandLineBuilder,
             Opus.Core.Option option,
             Opus.Core.Target target)
        {
            var fileCollectionOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.FileCollection>;
            if (fileCollectionOption.Value.Count > 0)
            {
                var fileList = new System.Text.StringBuilder();
                // TODO: convert to var
                foreach (string file in fileCollectionOption.Value)
                {
                    // TODO: need to check whether ; is appropriate here for *nix platforms
                    // It hasn't failed before, but if you are to copy-n-paste an Opus command line
                    // to a shell, would it think that a ; is a continuation character?
                    if (file.Contains(" "))
                    {
                        fileList.AppendFormat("\"{0}\";", file);
                    }
                    else
                    {
                        fileList.AppendFormat("{0};", file);
                    }
                }
                commandLineBuilder.Add(System.String.Format("/addmodule:{0}", fileList.ToString().TrimEnd(new char[] { ';' })));
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        ModulesVisualStudioProcessor(
             object sender,
             Opus.Core.Option option,
             Opus.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            // TODO
            return returnVal;
        }
        private static void
        DefinesCommandLineProcessor(
             object sender,
             Opus.Core.StringArray commandLineBuilder,
             Opus.Core.Option option,
             Opus.Core.Target target)
        {
            var stringArrayOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.StringArray>;
            if (stringArrayOption.Value.Count == 0)
            {
                return;
            }
            var definesList = new System.Text.StringBuilder();
            definesList.Append("/define:");
            foreach (var define in stringArrayOption.Value)
            {
                definesList.AppendFormat("{0};", define);
            }
            commandLineBuilder.Add(definesList.ToString().TrimEnd(new char[] { ';' }));
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        DefinesVisualStudioProcessor(
             object sender,
             Opus.Core.Option option,
             Opus.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var stringArrayOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.StringArray>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            if (stringArrayOption.Value.Count == 0)
            {
                return returnVal;
            }
            var definesList = new System.Text.StringBuilder();
            foreach (var define in stringArrayOption.Value)
            {
                definesList.AppendFormat("{0};", define);
            }
            returnVal.Add("DefineConstants", definesList.ToString().TrimEnd(new char[] { ';' }));
            return returnVal;
        }
        #endregion
        protected override void
        SetDelegates(
            Opus.Core.DependencyNode node)
        {
            this["Target"].PrivateData = new PrivateData(TargetCommandLineProcessor,TargetVisualStudioProcessor);
            this["NoLogo"].PrivateData = new PrivateData(NoLogoCommandLineProcessor,NoLogoVisualStudioProcessor);
            this["Platform"].PrivateData = new PrivateData(PlatformCommandLineProcessor,PlatformVisualStudioProcessor);
            this["DebugInformation"].PrivateData = new PrivateData(DebugInformationCommandLineProcessor,DebugInformationVisualStudioProcessor);
            this["Checked"].PrivateData = new PrivateData(CheckedCommandLineProcessor,CheckedVisualStudioProcessor);
            this["Unsafe"].PrivateData = new PrivateData(UnsafeCommandLineProcessor,UnsafeVisualStudioProcessor);
            this["WarningLevel"].PrivateData = new PrivateData(WarningLevelCommandLineProcessor,WarningLevelVisualStudioProcessor);
            this["WarningsAsErrors"].PrivateData = new PrivateData(WarningsAsErrorsCommandLineProcessor,WarningsAsErrorsVisualStudioProcessor);
            this["Optimize"].PrivateData = new PrivateData(OptimizeCommandLineProcessor,OptimizeVisualStudioProcessor);
            this["References"].PrivateData = new PrivateData(ReferencesCommandLineProcessor,ReferencesVisualStudioProcessor);
            this["Modules"].PrivateData = new PrivateData(ModulesCommandLineProcessor,ModulesVisualStudioProcessor);
            this["Defines"].PrivateData = new PrivateData(DefinesCommandLineProcessor,DefinesVisualStudioProcessor);
        }
    }
}
