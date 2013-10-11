// <copyright file="Builder.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed partial class XcodeBuilder : Opus.Core.IBuilder
    {
        public Workspace Workspace
        {
            get;
            private set;
        }

        public PBXProject Project
        {
            get;
            private set;
        }

        public System.Uri ProjectRootUri
        {
            get;
            private set;
        }

        public string ProjectPath
        {
            get;
            private set;
        }
    }
}
