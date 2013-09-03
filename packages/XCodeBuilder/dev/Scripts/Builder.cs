// <copyright file="Builder.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XCodeBuilder package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public sealed partial class XCodeBuilder : Opus.Core.IBuilder
    {
        public PBXProject Project
        {
            get;
            private set;
        }
    }
}
