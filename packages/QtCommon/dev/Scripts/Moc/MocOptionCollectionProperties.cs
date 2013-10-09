// Automatically generated file from OpusOptionCodeGenerator. DO NOT EDIT.
// Command line:
// -i=IMocOptions.cs -n=QtCommon -c=MocOptionCollection -p -d -dd=../../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs -pv=MocPrivateData
namespace QtCommon
{
    public partial class MocOptionCollection
    {
        #region IMocOptions Option properties
        string IMocOptions.MocOutputPath
        {
            get
            {
                return this.GetReferenceTypeOption<string>("MocOutputPath", this.SuperSetOptionCollection);
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
                return this.GetReferenceTypeOption<Opus.Core.DirectoryCollection>("IncludePaths", this.SuperSetOptionCollection);
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
                return this.GetReferenceTypeOption<C.DefineCollection>("Defines", this.SuperSetOptionCollection);
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
                return this.GetValueTypeOption<bool>("DoNotGenerateIncludeStatement", this.SuperSetOptionCollection);
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
                return this.GetValueTypeOption<bool>("DoNotDisplayWarnings", this.SuperSetOptionCollection);
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
                return this.GetReferenceTypeOption<string>("PathPrefix", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<string>("PathPrefix", value);
                this.ProcessNamedSetHandler("PathPrefixSetHandler", this["PathPrefix"]);
            }
        }
        #endregion
    }
}
