// <copyright file="BuildPhase.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public abstract class BuildPhase :
        XCodeNodeData
    {
        protected
        BuildPhase(
            string name,
            string moduleName) : base(name)
        {
            this.ModuleName = moduleName;
            this.Files = new Opus.Core.Array<PBXBuildFile>();
        }

        public string ModuleName
        {
            get;
            set;
        }

        public Opus.Core.Array<PBXBuildFile> Files
        {
            get;
            private set;
        }
    }
}
