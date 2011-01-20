// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=IMocOptions.cs -o=MocOptionProperties.cs -n=Qt -c=MocOptionCollection 
namespace Qt
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
    }
}
