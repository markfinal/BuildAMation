// <copyright file="PBXSourcesBuildPhase.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XCodeBuilder package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public sealed class PBXSourcesBuildPhase : XCodeNodeData, IWriteableNode
    {
        public PBXSourcesBuildPhase(string name)
            : base(name)
        {}

#region IWriteableNode implementation

        void IWriteableNode.Write(System.IO.TextWriter writer)
        {
        }

#endregion
    }
}
