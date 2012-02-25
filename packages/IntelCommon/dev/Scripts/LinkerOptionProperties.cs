// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=D:\dev\Opus\trunk\bin\Release\..\..\packages\C\dev\Scripts\ILinkerOptions.cs;D:\dev\Opus\trunk\bin\Release\..\..\packages\IntelCommon\dev\Scripts\ILinkerOptions.cs -o=LinkerOptionProperties.cs -n=IntelCommon -c=LinkerOptionCollection 
namespace IntelCommon
{
    public partial class LinkerOptionCollection
    {
        public C.ToolchainOptionCollection ToolchainOptionCollection
        {
            get
            {
                return this.GetReferenceTypeOption<C.ToolchainOptionCollection>("ToolchainOptionCollection");
            }
            set
            {
                this.SetReferenceTypeOption<C.ToolchainOptionCollection>("ToolchainOptionCollection", value);
                this.ProcessNamedSetHandler("ToolchainOptionCollectionSetHandler", this["ToolchainOptionCollection"]);
            }
        }
        public C.ELinkerOutput OutputType
        {
            get
            {
                return this.GetValueTypeOption<C.ELinkerOutput>("OutputType");
            }
            set
            {
                this.SetValueTypeOption<C.ELinkerOutput>("OutputType", value);
                this.ProcessNamedSetHandler("OutputTypeSetHandler", this["OutputType"]);
            }
        }
        public bool DoNotAutoIncludeStandardLibraries
        {
            get
            {
                return this.GetValueTypeOption<bool>("DoNotAutoIncludeStandardLibraries");
            }
            set
            {
                this.SetValueTypeOption<bool>("DoNotAutoIncludeStandardLibraries", value);
                this.ProcessNamedSetHandler("DoNotAutoIncludeStandardLibrariesSetHandler", this["DoNotAutoIncludeStandardLibraries"]);
            }
        }
        public bool DebugSymbols
        {
            get
            {
                return this.GetValueTypeOption<bool>("DebugSymbols");
            }
            set
            {
                this.SetValueTypeOption<bool>("DebugSymbols", value);
                this.ProcessNamedSetHandler("DebugSymbolsSetHandler", this["DebugSymbols"]);
            }
        }
        public C.ESubsystem SubSystem
        {
            get
            {
                return this.GetValueTypeOption<C.ESubsystem>("SubSystem");
            }
            set
            {
                this.SetValueTypeOption<C.ESubsystem>("SubSystem", value);
                this.ProcessNamedSetHandler("SubSystemSetHandler", this["SubSystem"]);
            }
        }
        public bool DynamicLibrary
        {
            get
            {
                return this.GetValueTypeOption<bool>("DynamicLibrary");
            }
            set
            {
                this.SetValueTypeOption<bool>("DynamicLibrary", value);
                this.ProcessNamedSetHandler("DynamicLibrarySetHandler", this["DynamicLibrary"]);
            }
        }
        public Opus.Core.DirectoryCollection LibraryPaths
        {
            get
            {
                return this.GetReferenceTypeOption<Opus.Core.DirectoryCollection>("LibraryPaths");
            }
            set
            {
                this.SetReferenceTypeOption<Opus.Core.DirectoryCollection>("LibraryPaths", value);
                this.ProcessNamedSetHandler("LibraryPathsSetHandler", this["LibraryPaths"]);
            }
        }
        public Opus.Core.FileCollection StandardLibraries
        {
            get
            {
                return this.GetReferenceTypeOption<Opus.Core.FileCollection>("StandardLibraries");
            }
            set
            {
                this.SetReferenceTypeOption<Opus.Core.FileCollection>("StandardLibraries", value);
                this.ProcessNamedSetHandler("StandardLibrariesSetHandler", this["StandardLibraries"]);
            }
        }
        public Opus.Core.FileCollection Libraries
        {
            get
            {
                return this.GetReferenceTypeOption<Opus.Core.FileCollection>("Libraries");
            }
            set
            {
                this.SetReferenceTypeOption<Opus.Core.FileCollection>("Libraries", value);
                this.ProcessNamedSetHandler("LibrariesSetHandler", this["Libraries"]);
            }
        }
        public bool GenerateMapFile
        {
            get
            {
                return this.GetValueTypeOption<bool>("GenerateMapFile");
            }
            set
            {
                this.SetValueTypeOption<bool>("GenerateMapFile", value);
                this.ProcessNamedSetHandler("GenerateMapFileSetHandler", this["GenerateMapFile"]);
            }
        }
        public string AdditionalOptions
        {
            get
            {
                return this.GetReferenceTypeOption<string>("AdditionalOptions");
            }
            set
            {
                this.SetReferenceTypeOption<string>("AdditionalOptions", value);
                this.ProcessNamedSetHandler("AdditionalOptionsSetHandler", this["AdditionalOptions"]);
            }
        }
        public bool CanUseOrigin
        {
            get
            {
                return this.GetValueTypeOption<bool>("CanUseOrigin");
            }
            set
            {
                this.SetValueTypeOption<bool>("CanUseOrigin", value);
                this.ProcessNamedSetHandler("CanUseOriginSetHandler", this["CanUseOrigin"]);
            }
        }
        public bool AllowUndefinedSymbols
        {
            get
            {
                return this.GetValueTypeOption<bool>("AllowUndefinedSymbols");
            }
            set
            {
                this.SetValueTypeOption<bool>("AllowUndefinedSymbols", value);
                this.ProcessNamedSetHandler("AllowUndefinedSymbolsSetHandler", this["AllowUndefinedSymbols"]);
            }
        }
        public Opus.Core.StringArray RPath
        {
            get
            {
                return this.GetReferenceTypeOption<Opus.Core.StringArray>("RPath");
            }
            set
            {
                this.SetReferenceTypeOption<Opus.Core.StringArray>("RPath", value);
                this.ProcessNamedSetHandler("RPathSetHandler", this["RPath"]);
            }
        }
    }
}
