// Automatically generated file from OpusOptionCodeGenerator. DO NOT EDIT.
// Command line arguments:
//     -i=../../../C/dev/Scripts/ILinkerOptions.cs&ILinkerOptions.cs
//     -n=MingwCommon
//     -c=LinkerOptionCollection
//     -p
//     -d
//     -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs
//     -pv=PrivateData

namespace MingwCommon
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
        Bam.Core.DirectoryCollection C.ILinkerOptions.LibraryPaths
        {
            get
            {
                return this.GetReferenceTypeOption<Bam.Core.DirectoryCollection>("LibraryPaths", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Bam.Core.DirectoryCollection>("LibraryPaths", value);
                this.ProcessNamedSetHandler("LibraryPathsSetHandler", this["LibraryPaths"]);
            }
        }
        Bam.Core.FileCollection C.ILinkerOptions.StandardLibraries
        {
            get
            {
                return this.GetReferenceTypeOption<Bam.Core.FileCollection>("StandardLibraries", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Bam.Core.FileCollection>("StandardLibraries", value);
                this.ProcessNamedSetHandler("StandardLibrariesSetHandler", this["StandardLibraries"]);
            }
        }
        Bam.Core.FileCollection C.ILinkerOptions.Libraries
        {
            get
            {
                return this.GetReferenceTypeOption<Bam.Core.FileCollection>("Libraries", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Bam.Core.FileCollection>("Libraries", value);
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
        bool ILinkerOptions.EnableAutoImport
        {
            get
            {
                return this.GetValueTypeOption<bool>("EnableAutoImport", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("EnableAutoImport", value);
                this.ProcessNamedSetHandler("EnableAutoImportSetHandler", this["EnableAutoImport"]);
            }
        }
        #endregion
    }
}
