// Automatically generated file from OpusOptionCodeGenerator. DO NOT EDIT.
// Command line:
// -i=../../../C/dev/Scripts/ILinkerOptions.cs;ILinkerOptions.cs -n=VisualCCommon -c=LinkerOptionCollection -p -d -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs;../../../VisualStudioProcessor/dev/Scripts/VisualStudioDelegate.cs -pv=PrivateData
namespace VisualCCommon
{
    public partial class LinkerOptionCollection
    {
        #region C.ILinkerOptions Option properties
        C.ELinkerOutput C.ILinkerOptions.OutputType
        {
            get
            {
                return this.GetValueTypeOption<C.ELinkerOutput>("OutputType", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<C.ELinkerOutput>("OutputType", value);
                this.ProcessNamedSetHandler("OutputTypeSetHandler", this["OutputType"]);
            }
        }
        bool C.ILinkerOptions.DoNotAutoIncludeStandardLibraries
        {
            get
            {
                return this.GetValueTypeOption<bool>("DoNotAutoIncludeStandardLibraries", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("DoNotAutoIncludeStandardLibraries", value);
                this.ProcessNamedSetHandler("DoNotAutoIncludeStandardLibrariesSetHandler", this["DoNotAutoIncludeStandardLibraries"]);
            }
        }
        bool C.ILinkerOptions.DebugSymbols
        {
            get
            {
                return this.GetValueTypeOption<bool>("DebugSymbols", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("DebugSymbols", value);
                this.ProcessNamedSetHandler("DebugSymbolsSetHandler", this["DebugSymbols"]);
            }
        }
        C.ESubsystem C.ILinkerOptions.SubSystem
        {
            get
            {
                return this.GetValueTypeOption<C.ESubsystem>("SubSystem", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<C.ESubsystem>("SubSystem", value);
                this.ProcessNamedSetHandler("SubSystemSetHandler", this["SubSystem"]);
            }
        }
        bool C.ILinkerOptions.DynamicLibrary
        {
            get
            {
                return this.GetValueTypeOption<bool>("DynamicLibrary", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("DynamicLibrary", value);
                this.ProcessNamedSetHandler("DynamicLibrarySetHandler", this["DynamicLibrary"]);
            }
        }
        Opus.Core.DirectoryCollection C.ILinkerOptions.LibraryPaths
        {
            get
            {
                return this.GetReferenceTypeOption<Opus.Core.DirectoryCollection>("LibraryPaths", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Opus.Core.DirectoryCollection>("LibraryPaths", value);
                this.ProcessNamedSetHandler("LibraryPathsSetHandler", this["LibraryPaths"]);
            }
        }
        Opus.Core.FileCollection C.ILinkerOptions.StandardLibraries
        {
            get
            {
                return this.GetReferenceTypeOption<Opus.Core.FileCollection>("StandardLibraries", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Opus.Core.FileCollection>("StandardLibraries", value);
                this.ProcessNamedSetHandler("StandardLibrariesSetHandler", this["StandardLibraries"]);
            }
        }
        Opus.Core.FileCollection C.ILinkerOptions.Libraries
        {
            get
            {
                return this.GetReferenceTypeOption<Opus.Core.FileCollection>("Libraries", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Opus.Core.FileCollection>("Libraries", value);
                this.ProcessNamedSetHandler("LibrariesSetHandler", this["Libraries"]);
            }
        }
        bool C.ILinkerOptions.GenerateMapFile
        {
            get
            {
                return this.GetValueTypeOption<bool>("GenerateMapFile", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("GenerateMapFile", value);
                this.ProcessNamedSetHandler("GenerateMapFileSetHandler", this["GenerateMapFile"]);
            }
        }
        string C.ILinkerOptions.AdditionalOptions
        {
            get
            {
                return this.GetReferenceTypeOption<string>("AdditionalOptions", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<string>("AdditionalOptions", value);
                this.ProcessNamedSetHandler("AdditionalOptionsSetHandler", this["AdditionalOptions"]);
            }
        }
        int C.ILinkerOptions.MajorVersion
        {
            get
            {
                return this.GetValueTypeOption<int>("MajorVersion", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<int>("MajorVersion", value);
                this.ProcessNamedSetHandler("MajorVersionSetHandler", this["MajorVersion"]);
            }
        }
        int C.ILinkerOptions.MinorVersion
        {
            get
            {
                return this.GetValueTypeOption<int>("MinorVersion", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<int>("MinorVersion", value);
                this.ProcessNamedSetHandler("MinorVersionSetHandler", this["MinorVersion"]);
            }
        }
        int C.ILinkerOptions.PatchVersion
        {
            get
            {
                return this.GetValueTypeOption<int>("PatchVersion", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<int>("PatchVersion", value);
                this.ProcessNamedSetHandler("PatchVersionSetHandler", this["PatchVersion"]);
            }
        }
        #endregion
        #region ILinkerOptions Option properties
        bool ILinkerOptions.NoLogo
        {
            get
            {
                return this.GetValueTypeOption<bool>("NoLogo", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("NoLogo", value);
                this.ProcessNamedSetHandler("NoLogoSetHandler", this["NoLogo"]);
            }
        }
        string ILinkerOptions.StackReserveAndCommit
        {
            get
            {
                return this.GetReferenceTypeOption<string>("StackReserveAndCommit", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<string>("StackReserveAndCommit", value);
                this.ProcessNamedSetHandler("StackReserveAndCommitSetHandler", this["StackReserveAndCommit"]);
            }
        }
        Opus.Core.StringArray ILinkerOptions.IgnoredLibraries
        {
            get
            {
                return this.GetReferenceTypeOption<Opus.Core.StringArray>("IgnoredLibraries", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Opus.Core.StringArray>("IgnoredLibraries", value);
                this.ProcessNamedSetHandler("IgnoredLibrariesSetHandler", this["IgnoredLibraries"]);
            }
        }
        bool ILinkerOptions.IncrementalLink
        {
            get
            {
                return this.GetValueTypeOption<bool>("IncrementalLink", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("IncrementalLink", value);
                this.ProcessNamedSetHandler("IncrementalLinkSetHandler", this["IncrementalLink"]);
            }
        }
        #endregion
    }
}
