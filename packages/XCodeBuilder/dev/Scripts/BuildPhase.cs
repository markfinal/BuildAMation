// <copyright file="BuildPhase.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XCodeBuilder package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public abstract class BuildPhase : XCodeNodeData
    {
        protected BuildPhase(string name, string moduleName)
            : base(name)
        {
            this.ModuleName = moduleName;
            this.Files = new System.Collections.Generic.List<PBXBuildFile>();
        }

        public string ModuleName
        {
            get;
            set;
        }

        public System.Collections.Generic.List<PBXBuildFile> Files
        {
            get;
            private set;
        }
    }
}
