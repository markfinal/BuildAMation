// <copyright file="Builder.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed partial class XcodeBuilder :
        Bam.Core.IBuilder
    {
        public Workspace Workspace
        {
            get;
            private set;
        }
    }
}
