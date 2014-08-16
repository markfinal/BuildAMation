// <copyright file="BuildPhase.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public abstract class BuildPhase :
        XcodeNodeData
    {
        protected
        BuildPhase(
            string name,
            string moduleName) : base(name)
        {
            this.ModuleName = moduleName;
            this.Files = new Bam.Core.Array<PBXBuildFile>();
        }

        public string ModuleName
        {
            get;
            set;
        }

        public Bam.Core.Array<PBXBuildFile> Files
        {
            get;
            private set;
        }
    }
}
