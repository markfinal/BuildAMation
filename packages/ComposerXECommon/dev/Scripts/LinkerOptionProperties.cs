// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=D:\dev\Opus\trunk\bin\Debug\..\..\packages\C\dev\Scripts\ILinkerOptions.cs;D:\dev\Opus\trunk\bin\Debug\..\..\packages\ComposerXECommon\dev\Scripts\ILinkerOptions.cs -o=LinkerOptionProperties.cs -n=ComposerXECommon -c=LinkerOptionCollection 
namespace ComposerXECommon
{
    public partial class LinkerOptionCollection
    {
        C.ToolchainOptionCollection C.ILinkerOptions.ToolchainOptionCollection
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
        C.ELinkerOutput C.ILinkerOptions.OutputType
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
        bool C.ILinkerOptions.DoNotAutoIncludeStandardLibraries
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
        bool C.ILinkerOptions.DebugSymbols
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
        C.ESubsystem C.ILinkerOptions.SubSystem
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
        bool C.ILinkerOptions.DynamicLibrary
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
        Opus.Core.DirectoryCollection C.ILinkerOptions.LibraryPaths
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
        Opus.Core.FileCollection C.ILinkerOptions.StandardLibraries
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
        Opus.Core.FileCollection C.ILinkerOptions.Libraries
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
        bool C.ILinkerOptions.GenerateMapFile
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
        string C.ILinkerOptions.AdditionalOptions
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
        bool ILinkerOptions.CanUseOrigin
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
        bool ILinkerOptions.AllowUndefinedSymbols
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
        Opus.Core.StringArray ILinkerOptions.RPath
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
