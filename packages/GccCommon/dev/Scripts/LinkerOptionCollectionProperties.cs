// Automatically generated file from OpusOptionCodeGenerator. DO NOT EDIT.
// Command line:
// -i=../../../C/dev/Scripts/ILinkerOptions.cs:../../../C/dev/Scripts/ILinkerOptionsOSX.cs:ILinkerOptions.cs -n=GccCommon -c=LinkerOptionCollection -p -d -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs:../../../XcodeProjectProcessor/dev/Scripts/Delegate.cs -pv=PrivateData
namespace GccCommon
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
        #endregion
        #region C.ILinkerOptionsOSX Option properties
        bool C.ILinkerOptionsOSX.ApplicationBundle
        {
            get
            {
                return this.GetValueTypeOption<bool>("ApplicationBundle", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("ApplicationBundle", value);
                this.ProcessNamedSetHandler("ApplicationBundleSetHandler", this["ApplicationBundle"]);
            }
        }
        Opus.Core.StringArray C.ILinkerOptionsOSX.Frameworks
        {
            get
            {
                return this.GetReferenceTypeOption<Opus.Core.StringArray>("Frameworks", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Opus.Core.StringArray>("Frameworks", value);
                this.ProcessNamedSetHandler("FrameworksSetHandler", this["Frameworks"]);
            }
        }
        Opus.Core.DirectoryCollection C.ILinkerOptionsOSX.FrameworkSearchDirectories
        {
            get
            {
                return this.GetReferenceTypeOption<Opus.Core.DirectoryCollection>("FrameworkSearchDirectories", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Opus.Core.DirectoryCollection>("FrameworkSearchDirectories", value);
                this.ProcessNamedSetHandler("FrameworkSearchDirectoriesSetHandler", this["FrameworkSearchDirectories"]);
            }
        }
        bool C.ILinkerOptionsOSX.SuppressReadOnlyRelocations
        {
            get
            {
                return this.GetValueTypeOption<bool>("SuppressReadOnlyRelocations", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("SuppressReadOnlyRelocations", value);
                this.ProcessNamedSetHandler("SuppressReadOnlyRelocationsSetHandler", this["SuppressReadOnlyRelocations"]);
            }
        }
        #endregion
        #region ILinkerOptions Option properties
        bool ILinkerOptions.CanUseOrigin
        {
            get
            {
                return this.GetValueTypeOption<bool>("CanUseOrigin", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("CanUseOrigin", value);
                this.ProcessNamedSetHandler("CanUseOriginSetHandler", this["CanUseOrigin"]);
            }
        }
        bool ILinkerOptions.AllowUndefinedSymbols
        {
            get
            {
                return this.GetValueTypeOption<bool>("AllowUndefinedSymbols", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("AllowUndefinedSymbols", value);
                this.ProcessNamedSetHandler("AllowUndefinedSymbolsSetHandler", this["AllowUndefinedSymbols"]);
            }
        }
        Opus.Core.StringArray ILinkerOptions.RPath
        {
            get
            {
                return this.GetReferenceTypeOption<Opus.Core.StringArray>("RPath", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Opus.Core.StringArray>("RPath", value);
                this.ProcessNamedSetHandler("RPathSetHandler", this["RPath"]);
            }
        }
        bool ILinkerOptions.SixtyFourBit
        {
            get
            {
                return this.GetValueTypeOption<bool>("SixtyFourBit", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("SixtyFourBit", value);
                this.ProcessNamedSetHandler("SixtyFourBitSetHandler", this["SixtyFourBit"]);
            }
        }
        #endregion
    }
}
