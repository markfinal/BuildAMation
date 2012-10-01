// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=IMocOptions.cs -o=MocOptionProperties.cs -n=QtCommon -c=MocOptionCollection 
namespace QtCommon
{
    public partial class MocOptionCollection
    {
        string IMocOptions.MocOutputPath
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
        Opus.Core.DirectoryCollection IMocOptions.IncludePaths
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
        C.DefineCollection IMocOptions.Defines
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
        bool IMocOptions.DoNotGenerateIncludeStatement
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
        bool IMocOptions.DoNotDisplayWarnings
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
        string IMocOptions.PathPrefix
        {
            get
            {
                return this.GetReferenceTypeOption<string>("PathPrefix");
            }
            set
            {
                this.SetReferenceTypeOption<string>("PathPrefix", value);
                this.ProcessNamedSetHandler("PathPrefixSetHandler", this["PathPrefix"]);
            }
        }
    }
}
