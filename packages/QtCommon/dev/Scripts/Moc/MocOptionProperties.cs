// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=IMocOptions.cs -o=MocOptionProperties.cs -n=QtCommon -c=MocOptionCollection 
namespace QtCommon
{
    public partial class MocOptionCollection
    {
        public string MocOutputPath
        {
            get
            {
                return this.GetReferenceTypeOption<string>("MocOutputPath");
            }
            set
            {
                this.SetReferenceTypeOption<string>("MocOutputPath", value);
                this.ProcessNamedSetHandler("MocOutputPathSetHandler", this["MocOutputPath"]);
            }
        }
        public Opus.Core.DirectoryCollection IncludePaths
        {
            get
            {
                return this.GetReferenceTypeOption<Opus.Core.DirectoryCollection>("IncludePaths");
            }
            set
            {
                this.SetReferenceTypeOption<Opus.Core.DirectoryCollection>("IncludePaths", value);
                this.ProcessNamedSetHandler("IncludePathsSetHandler", this["IncludePaths"]);
            }
        }
        public C.DefineCollection Defines
        {
            get
            {
                return this.GetReferenceTypeOption<C.DefineCollection>("Defines");
            }
            set
            {
                this.SetReferenceTypeOption<C.DefineCollection>("Defines", value);
                this.ProcessNamedSetHandler("DefinesSetHandler", this["Defines"]);
            }
        }
        public bool DoNotGenerateIncludeStatement
        {
            get
            {
                return this.GetValueTypeOption<bool>("DoNotGenerateIncludeStatement");
            }
            set
            {
                this.SetValueTypeOption<bool>("DoNotGenerateIncludeStatement", value);
                this.ProcessNamedSetHandler("DoNotGenerateIncludeStatementSetHandler", this["DoNotGenerateIncludeStatement"]);
            }
        }
        public bool DoNotDisplayWarnings
        {
            get
            {
                return this.GetValueTypeOption<bool>("DoNotDisplayWarnings");
            }
            set
            {
                this.SetValueTypeOption<bool>("DoNotDisplayWarnings", value);
                this.ProcessNamedSetHandler("DoNotDisplayWarningsSetHandler", this["DoNotDisplayWarnings"]);
            }
        }
    }
}
