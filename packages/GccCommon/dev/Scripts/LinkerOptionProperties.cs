// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=D:\dev\prototypes\Opus\dev\bin\Debug\..\..\packages\C\dev\Scripts\ILinkerOptions.cs -o=LinkerOptionProperties.cs -n=GccCommon -c=LinkerOptionCollection 
namespace GccCommon
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
        public bool IgnoreStandardLibraries
        {
            get
            {
                return this.GetValueTypeOption<bool>("IgnoreStandardLibraries");
            }
            set
            {
                this.SetValueTypeOption<bool>("IgnoreStandardLibraries", value);
                this.ProcessNamedSetHandler("IgnoreStandardLibrariesSetHandler", this["IgnoreStandardLibraries"]);
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
    }
}
