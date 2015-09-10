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
//     -i=../../../C/dev/Scripts/ILinkerOptions.cs&ILinkerOptions.cs
//     -n=VisualCCommon
//     -c=LinkerOptionCollection
//     -p
//     -d
//     -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs&../../../VisualStudioProcessor/dev/Scripts/VisualStudioDelegate.cs
//     -pv=PrivateData
#endregion // BamOptionGenerator
namespace VisualCCommon
{
    public partial class LinkerOptionCollection
    {
        #region C.ILinkerOptions Option delegates
        private static void
        OutputTypeCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var enumOption = option as Bam.Core.ValueTypeOption<C.ELinkerOutput>;
            var options = sender as LinkerOptionCollection;
            switch (enumOption.Value)
            {
                case C.ELinkerOutput.Executable:
                {
                    var outputFileLocation = options.GetModuleLocation(C.Application.OutputFile);
                    commandLineBuilder.Add(System.String.Format("-OUT:{0}", outputFileLocation.GetSinglePath()));
                    break;
                }
                case C.ELinkerOutput.DynamicLibrary:
                {
                    var outputFileLocation = options.GetModuleLocation(C.Application.OutputFile);
                    commandLineBuilder.Add(System.String.Format("-OUT:{0}", outputFileLocation.GetSinglePath()));
                    commandLineBuilder.Add("-DLL");
                    var staticImportLibraryLoc = options.GetModuleLocation(C.DynamicLibrary.ImportLibraryFile);
                    commandLineBuilder.Add(System.String.Format("-IMPLIB:{0}", staticImportLibraryLoc.GetSinglePath()));
                    break;
                }
                default:
                    throw new Bam.Core.Exception("Unrecognized value for C.ELinkerOutput");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        OutputTypeVisualStudioProcessor(
             object sender,
             Bam.Core.Option option,
             Bam.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var enumOption = option as Bam.Core.ValueTypeOption<C.ELinkerOutput>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            var options = sender as LinkerOptionCollection;
            switch (enumOption.Value)
            {
                case C.ELinkerOutput.Executable:
                {
                    var outputFileLocation = options.GetModuleLocation(C.Application.OutputFile);
                    returnVal.Add("OutputFile", outputFileLocation.GetSinglePath());
                    break;
                }
                case C.ELinkerOutput.DynamicLibrary:
                {
                    var outputFileLocation = options.GetModuleLocation(C.Application.OutputFile);
                    returnVal.Add("OutputFile", outputFileLocation.GetSinglePath());
                    var staticImportLibraryLoc = options.GetModuleLocation(C.DynamicLibrary.ImportLibraryFile);
                    returnVal.Add("ImportLibrary", staticImportLibraryLoc.GetSinglePath());
                    break;
                }
                default:
                    throw new Bam.Core.Exception("Unrecognized value for C.ELinkerOutput");
            }
            return returnVal;
        }
        private static void
        DoNotAutoIncludeStandardLibrariesCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var ignoreStandardLibrariesOption = option as Bam.Core.ValueTypeOption<bool>;
            if (ignoreStandardLibrariesOption.Value)
            {
                commandLineBuilder.Add("-NODEFAULTLIB");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        DoNotAutoIncludeStandardLibrariesVisualStudioProcessor(
             object sender,
             Bam.Core.Option option,
             Bam.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var ignoreStandardLibrariesOption = option as Bam.Core.ValueTypeOption<bool>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("IgnoreAllDefaultLibraries", ignoreStandardLibrariesOption.Value.ToString().ToLower());
            return returnVal;
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
                commandLineBuilder.Add("-DEBUG");
                var pdbFile = (sender as Bam.Core.BaseOptionCollection).GetModuleLocation(Linker.PDBFile);
                var pdbPath = pdbFile.GetSinglePath();
                commandLineBuilder.Add(System.String.Format("-PDB:{0}", pdbPath));
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        DebugSymbolsVisualStudioProcessor(
             object sender,
             Bam.Core.Option option,
             Bam.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var debugSymbolsOption = option as Bam.Core.ValueTypeOption<bool>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("GenerateDebugInformation", debugSymbolsOption.Value.ToString().ToLower());
            if (debugSymbolsOption.Value)
            {
                var pdbFile = (sender as Bam.Core.BaseOptionCollection).GetModuleLocation(Linker.PDBFile);
                var pdbPath = pdbFile.GetSinglePath();
                returnVal.Add("ProgramDatabaseFile", pdbPath);
            }
            return returnVal;
        }
        private static void
        SubSystemCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var subSystemOption = option as Bam.Core.ValueTypeOption<C.ESubsystem>;
            switch (subSystemOption.Value)
            {
                case C.ESubsystem.NotSet:
                    // do nothing
                    break;
                case C.ESubsystem.Console:
                    commandLineBuilder.Add("-SUBSYSTEM:CONSOLE");
                    break;
                case C.ESubsystem.Windows:
                    commandLineBuilder.Add("-SUBSYSTEM:WINDOWS");
                    break;
                default:
                    throw new Bam.Core.Exception("Unrecognized subsystem option");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        SubSystemVisualStudioProcessor(
             object sender,
             Bam.Core.Option option,
             Bam.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var subSystemOption = option as Bam.Core.ValueTypeOption<C.ESubsystem>;
            switch (subSystemOption.Value)
            {
                case C.ESubsystem.NotSet:
                case C.ESubsystem.Console:
                case C.ESubsystem.Windows:
                    {
                        var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
                        if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
                        {
                            returnVal.Add("SubSystem", subSystemOption.Value.ToString("D"));
                        }
                        else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
                        {
                            returnVal.Add("SubSystem", subSystemOption.Value.ToString());
                        }
                        return returnVal;
                    }
                default:
                    throw new Bam.Core.Exception("Unrecognized subsystem option");
            }
        }
        private static void
        LibraryPathsCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var librarySearchPathsOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.DirectoryCollection>;
            // TODO: change to var, and returning Locations
            foreach (string librarySearchPath in librarySearchPathsOption.Value)
            {
                if (librarySearchPath.Contains(" "))
                {
                    commandLineBuilder.Add(System.String.Format("-LIBPATH:\"{0}\"", librarySearchPath));
                }
                else
                {
                    commandLineBuilder.Add(System.String.Format("-LIBPATH:{0}", librarySearchPath));
                }
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        LibraryPathsVisualStudioProcessor(
             object sender,
             Bam.Core.Option option,
             Bam.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var libraryPathsOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.DirectoryCollection>;
            var libraryPaths = new System.Text.StringBuilder();
            // TODO: change to var, returning Locations
            foreach (string libraryPath in libraryPathsOption.Value)
            {
                if (libraryPath.Contains(" "))
                {
                    libraryPaths.Append(System.String.Format("\"{0}\";", libraryPath));
                }
                else
                {
                    libraryPaths.Append(System.String.Format("{0};", libraryPath));
                }
            }
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("AdditionalLibraryDirectories", libraryPaths.ToString());
            return returnVal;
        }
        private static void
        StandardLibrariesCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var librariesOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.FileCollection>;
            // TODO: change to var, and returning Locations
            foreach (Bam.Core.Location libraryPath in librariesOption.Value)
            {
                commandLineBuilder.Add(libraryPath.GetSinglePath());
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        StandardLibrariesVisualStudioProcessor(
             object sender,
             Bam.Core.Option option,
             Bam.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var options = sender as C.ILinkerOptions;
            if (options.DoNotAutoIncludeStandardLibraries)
            {
                var standardLibraryPathsOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.FileCollection>;
                var standardLibraryPaths = new System.Text.StringBuilder();
                if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
                {
                    // this stops any other libraries from being inherited
                    standardLibraryPaths.Append("$(NOINHERIT) ");
                    // TODO: change to var, returning Locations
                    foreach (string standardLibraryPath in standardLibraryPathsOption.Value)
                    {
                        if (standardLibraryPath.Contains(" "))
                        {
                            standardLibraryPaths.Append(System.String.Format("\"{0}\" ", standardLibraryPath));
                        }
                        else
                        {
                            standardLibraryPaths.Append(System.String.Format("{0} ", standardLibraryPath));
                        }
                    }
                }
                else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
                {
                    // TODO: change to var, returning Locations
                    foreach (string standardLibraryPath in standardLibraryPathsOption.Value)
                    {
                        standardLibraryPaths.Append(System.String.Format("{0};", standardLibraryPath));
                    }
                }
                var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
                returnVal.Add("AdditionalDependencies", standardLibraryPaths.ToString());
                return returnVal;
            }
            else
            {
                return null;
            }
        }
        private static void
        LibrariesCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var librariesOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.FileCollection>;
            // TODO: change to var, and returning Locations
            foreach (Bam.Core.Location libraryPath in librariesOption.Value)
            {
                commandLineBuilder.Add(libraryPath.GetSinglePath());
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        LibrariesVisualStudioProcessor(
             object sender,
             Bam.Core.Option option,
             Bam.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var libraryPathsOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.FileCollection>;
            var libraryPaths = new System.Text.StringBuilder();
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                // this stops any other libraries from being inherited
                libraryPaths.Append("$(NOINHERIT) ");
                // TODO: change to var, returning Locations
                foreach (Bam.Core.Location location in libraryPathsOption.Value)
                {
                    var standardLibraryPath = location.GetSinglePath();
                    if (standardLibraryPath.Contains(" "))
                    {
                        libraryPaths.Append(System.String.Format("\"{0}\" ", standardLibraryPath));
                    }
                    else
                    {
                        libraryPaths.Append(System.String.Format("{0} ", standardLibraryPath));
                    }
                }
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                // TODO: change to var, returning Locations
                foreach (Bam.Core.Location location in libraryPathsOption.Value)
                {
                    var standardLibraryPath = location.GetSinglePath();
                    libraryPaths.Append(System.String.Format("{0};", standardLibraryPath));
                }
            }
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("AdditionalDependencies", libraryPaths.ToString());
            return returnVal;
        }
        private static void
        GenerateMapFileCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                var mapFileLoc = (sender as LinkerOptionCollection).GetModuleLocation(C.Application.MapFile);
                commandLineBuilder.Add(System.String.Format("-MAP:{0}", mapFileLoc.GetSinglePath()));
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        GenerateMapFileVisualStudioProcessor(
             object sender,
             Bam.Core.Option option,
             Bam.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("GenerateMapFile", boolOption.Value.ToString().ToLower());
            if (boolOption.Value)
            {
                var mapFileLoc = (sender as LinkerOptionCollection).GetModuleLocation(C.Application.MapFile);
                returnVal.Add("MapFileName", mapFileLoc.GetSingleRawPath());
            }
            return returnVal;
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
        private static VisualStudioProcessor.ToolAttributeDictionary
        AdditionalOptionsVisualStudioProcessor(
             object sender,
             Bam.Core.Option option,
             Bam.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var stringOption = option as Bam.Core.ReferenceTypeOption<string>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("AdditionalOptions", stringOption.Value);
            return returnVal;
        }
        #endregion
        #region ILinkerOptions Option delegates
        private static void
        NoLogoCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var noLogoOption = option as Bam.Core.ValueTypeOption<bool>;
            if (noLogoOption.Value)
            {
                commandLineBuilder.Add("-NOLOGO");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        NoLogoVisualStudioProcessor(
             object sender,
             Bam.Core.Option option,
             Bam.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var noLogoOption = option as Bam.Core.ValueTypeOption<bool>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("SuppressStartupBanner", noLogoOption.Value.ToString().ToLower());
            return returnVal;
        }
        private static void
        StackReserveAndCommitCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var stackSizeOption = option as Bam.Core.ReferenceTypeOption<string>;
            var stackSize = stackSizeOption.Value;
            if (stackSize != null)
            {
                // this will be formatted as "reserve[,commit]"
                commandLineBuilder.Add(System.String.Format("-STACK:{0}", stackSizeOption.Value));
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        StackReserveAndCommitVisualStudioProcessor(
             object sender,
             Bam.Core.Option option,
             Bam.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var stackSizeOption = option as Bam.Core.ReferenceTypeOption<string>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            var stackSize = stackSizeOption.Value;
            if (stackSize != null)
            {
                // this will be formatted as "reserve[,commit]"
                var split = stackSize.Split(',');
                returnVal.Add("StackReserveSize", split[0]);
                if (split.Length > 1)
                {
                    returnVal.Add("StackCommitSize", split[1]);
                }
            }
            return returnVal;
        }
        private static void
        IgnoredLibrariesCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var ignoredLibrariesOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.StringArray>;
            // TODO: change to var
            foreach (string library in ignoredLibrariesOption.Value)
            {
                commandLineBuilder.Add(System.String.Format("-NODEFAULTLIB:{0}", library));
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        IgnoredLibrariesVisualStudioProcessor(
             object sender,
             Bam.Core.Option option,
             Bam.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var ignoredLibrariesOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.StringArray>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            var value = ignoredLibrariesOption.Value.ToString(';');
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                returnVal.Add("IgnoreDefaultLibraryNames", value);
            }
            else
            {
                returnVal.Add("IgnoreSpecificDefaultLibraries", value);
            }
            return returnVal;
        }
        private static void
        IncrementalLinkCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-INCREMENTAL");
            }
            else
            {
                commandLineBuilder.Add("-INCREMENTAL:NO");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        IncrementalLinkVisualStudioProcessor(
             object sender,
             Bam.Core.Option option,
             Bam.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
            if (vsTarget == VisualStudioProcessor.EVisualStudioTarget.VCPROJ)
            {
                if (boolOption.Value)
                {
                    returnVal.Add("LinkIncremental", "2");
                }
                else
                {
                    returnVal.Add("LinkIncremental", "0");
                }
            }
            else
            {
                // TODO: this is wrong - it needs to be on the PropertyGroup, not the ItemDefinitionGroup
                returnVal.Add("LinkIncremental", boolOption.Value.ToString().ToLower());
            }
            return returnVal;
        }
        #endregion
        protected override void
        SetDelegates(
            Bam.Core.DependencyNode node)
        {
            this["OutputType"].PrivateData = new PrivateData(OutputTypeCommandLineProcessor,OutputTypeVisualStudioProcessor);
            this["DoNotAutoIncludeStandardLibraries"].PrivateData = new PrivateData(DoNotAutoIncludeStandardLibrariesCommandLineProcessor,DoNotAutoIncludeStandardLibrariesVisualStudioProcessor);
            this["DebugSymbols"].PrivateData = new PrivateData(DebugSymbolsCommandLineProcessor,DebugSymbolsVisualStudioProcessor);
            this["SubSystem"].PrivateData = new PrivateData(SubSystemCommandLineProcessor,SubSystemVisualStudioProcessor);
            this["LibraryPaths"].PrivateData = new PrivateData(LibraryPathsCommandLineProcessor,LibraryPathsVisualStudioProcessor);
            this["StandardLibraries"].PrivateData = new PrivateData(StandardLibrariesCommandLineProcessor,StandardLibrariesVisualStudioProcessor);
            this["Libraries"].PrivateData = new PrivateData(LibrariesCommandLineProcessor,LibrariesVisualStudioProcessor);
            this["GenerateMapFile"].PrivateData = new PrivateData(GenerateMapFileCommandLineProcessor,GenerateMapFileVisualStudioProcessor);
            this["AdditionalOptions"].PrivateData = new PrivateData(AdditionalOptionsCommandLineProcessor,AdditionalOptionsVisualStudioProcessor);
            // Property 'MajorVersion' is value only - no delegates
            // Property 'MinorVersion' is value only - no delegates
            // Property 'PatchVersion' is value only - no delegates
            this["NoLogo"].PrivateData = new PrivateData(NoLogoCommandLineProcessor,NoLogoVisualStudioProcessor);
            this["StackReserveAndCommit"].PrivateData = new PrivateData(StackReserveAndCommitCommandLineProcessor,StackReserveAndCommitVisualStudioProcessor);
            this["IgnoredLibraries"].PrivateData = new PrivateData(IgnoredLibrariesCommandLineProcessor,IgnoredLibrariesVisualStudioProcessor);
            this["IncrementalLink"].PrivateData = new PrivateData(IncrementalLinkCommandLineProcessor,IncrementalLinkVisualStudioProcessor);
        }
    }
}
