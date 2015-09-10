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
#region BamOptionGenerator
// Automatically generated file from BamOptionGenerator.
// Command line arguments:
//     -i=IOptions.cs
//     -n=CSharp
//     -c=OptionCollection
//     -p
//     -d
//     -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs&../../../VisualStudioProcessor/dev/Scripts/VisualStudioDelegate.cs
//     -pv=PrivateData
#endregion // BamOptionGenerator
namespace CSharp
{
    public partial class OptionCollection
    {
        #region IOptions Option delegates
        private static void
        TargetCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var options = sender as OptionCollection;
            var enumOption = option as Bam.Core.ValueTypeOption<ETarget>;
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
                    throw new Bam.Core.Exception("Unrecognized CSharp.ETarget value");
            }
            var outputPath = options.GetModuleLocation(CSharp.Assembly.OutputFile).GetSinglePath();
            commandLineBuilder.Add(System.String.Format("/out:{0}", outputPath));
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        TargetVisualStudioProcessor(
             object sender,
             Bam.Core.Option option,
             Bam.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            var enumOption = option as Bam.Core.ValueTypeOption<ETarget>;
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
                    throw new Bam.Core.Exception("Unrecognized CSharp.ETarget value");
            }
            return returnVal;
        }
        private static void
        NoLogoCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("/nologo");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        NoLogoVisualStudioProcessor(
             object sender,
             Bam.Core.Option option,
             Bam.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
            returnVal.Add("NoLogo", boolOption.Value ? "true" : "false");
            return returnVal;
        }
        private static void
        PlatformCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            // no such option for mono-csc
            if (Bam.Core.State.RunningMono)
            {
                return;
            }
            var enumOption = option as Bam.Core.ValueTypeOption<EPlatform>;
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
                    throw new Bam.Core.Exception("Unrecognized CSharp.EPlatform value");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        PlatformVisualStudioProcessor(
             object sender,
             Bam.Core.Option option,
             Bam.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var enumOption = option as Bam.Core.ValueTypeOption<EPlatform>;
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
                    throw new Bam.Core.Exception("Unrecognized CSharp.EPlatform value");
            }
            return returnVal;
        }
        private static void
        DebugInformationCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            //var options = sender as OptionCollection;
            var enumOption = option as Bam.Core.ValueTypeOption<EDebugInformation>;
            switch (enumOption.Value)
            {
                case EDebugInformation.Disabled:
                    commandLineBuilder.Add("/debug-");
                    break;
                case EDebugInformation.ProgramDatabaseOnly:
                    {
                        if (Bam.Core.OSUtilities.IsWindowsHosting)
                        {
                            commandLineBuilder.Add("/debug+");
                            commandLineBuilder.Add("/debug:pdbinfo");
                            var pdbFileLoc = (sender as OptionCollection).GetModuleLocation(Assembly.PDBFile);
                            commandLineBuilder.Add(System.String.Format("/pdb:{0}", pdbFileLoc.GetSinglePath()));
                        }
                        else
                        {
                            commandLineBuilder.Add("/debug+");
                        }
                    }
                    break;
                case EDebugInformation.Full:
                    {
                        if (Bam.Core.OSUtilities.IsWindowsHosting)
                        {
                            commandLineBuilder.Add("/debug+");
                            commandLineBuilder.Add("/debug:full");
                            var pdbFileLoc = (sender as OptionCollection).GetModuleLocation(Assembly.PDBFile);
                            commandLineBuilder.Add(System.String.Format("/pdb:{0}", pdbFileLoc.GetSinglePath()));
                        }
                        else
                        {
                            commandLineBuilder.Add("/debug+");
                        }
                    }
                    break;
                default:
                    throw new Bam.Core.Exception("Unrecognized CSharp.EDebugInformation value");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        DebugInformationVisualStudioProcessor(
             object sender,
             Bam.Core.Option option,
             Bam.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            var enumOption = option as Bam.Core.ValueTypeOption<EDebugInformation>;
            switch (enumOption.Value)
            {
                case EDebugInformation.Disabled:
                    returnVal.Add("DebugSymbols", "false");
                    break;
                case EDebugInformation.ProgramDatabaseOnly:
                    {
                        if (Bam.Core.OSUtilities.IsWindowsHosting)
                        {
                            returnVal.Add("DebugSymbols", "true");
                            returnVal.Add("DebugType", "pdbinfo");
                            // TODO
                            //var pdbFileLoc = (sender as OptionCollection).GetModuleLocation(Assembly.PDBFile);
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
                        if (Bam.Core.OSUtilities.IsWindowsHosting)
                        {
                            returnVal.Add("DebugSymbols", "true");
                            returnVal.Add("DebugType", "Full");
                            // TODO
                            //var pdbFileLoc = (sender as OptionCollection).GetModuleLocation(Assembly.PDBFile);
                            //commandLineBuilder.Add(System.String.Format("/pdb:\"{0}\"", pdbFileLoc.GetSinglePath()));
                        }
                        else
                        {
                            returnVal.Add("DebugSymbols", "true");
                        }
                    }
                    break;
                default:
                    throw new Bam.Core.Exception("Unrecognized CSharp.EDebugInformation value");
            }
            return returnVal;
        }
        private static void
        CheckedCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
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
             Bam.Core.Option option,
             Bam.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("CheckForOverflowUnderflow", boolOption.Value ? "true" : "false");
            return returnVal;
        }
        private static void
        UnsafeCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
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
             Bam.Core.Option option,
             Bam.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("Unsafe", boolOption.Value ? "true" : "false");
            return returnVal;
        }
        private static void
        WarningLevelCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var enumOption = option as Bam.Core.ValueTypeOption<EWarningLevel>;
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
                    throw new Bam.Core.Exception("Unrecognized CSharp.EWarningLevel value");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        WarningLevelVisualStudioProcessor(
             object sender,
             Bam.Core.Option option,
             Bam.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var enumOption = option as Bam.Core.ValueTypeOption<EWarningLevel>;
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
                    throw new Bam.Core.Exception("Unrecognized CSharp.EWarningLevel value");
            }
            return returnVal;
        }
        private static void
        WarningsAsErrorsCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
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
             Bam.Core.Option option,
             Bam.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("TreatWarningsAsErrors", boolOption.Value ? "true" : "false");
            return returnVal;
        }
        private static void
        OptimizeCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
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
             Bam.Core.Option option,
             Bam.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("Optimize", boolOption.Value ? "true" : "false");
            return returnVal;
        }
        private static void
        ReferencesCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var fileCollectionOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.FileCollection>;
            if (fileCollectionOption.Value.Count > 0)
            {
                var fileList = new System.Text.StringBuilder();
                // TODO: convert to var
                foreach (Bam.Core.FileLocation location in fileCollectionOption.Value)
                {
                    var file = location.AbsolutePath;
                    // TODO: need to check whether ; is appropriate here for *nix platforms
                    // It hasn't failed before, but if you are to copy-n-paste an bam command line
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
             Bam.Core.Option option,
             Bam.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            // this is handled elsewhere
            return null;
        }
        private static void
        ModulesCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var fileCollectionOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.FileCollection>;
            if (fileCollectionOption.Value.Count > 0)
            {
                var fileList = new System.Text.StringBuilder();
                // TODO: convert to var
                foreach (string file in fileCollectionOption.Value)
                {
                    // TODO: need to check whether ; is appropriate here for *nix platforms
                    // It hasn't failed before, but if you are to copy-n-paste an bam command line
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
             Bam.Core.Option option,
             Bam.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            // TODO
            return returnVal;
        }
        private static void
        DefinesCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var stringArrayOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.StringArray>;
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
             Bam.Core.Option option,
             Bam.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var stringArrayOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.StringArray>;
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
            Bam.Core.DependencyNode node)
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
