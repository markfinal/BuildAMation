// <copyright file="PBXGroup.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XCodeBuilder package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public sealed class PBXGroup : XCodeNodeData, IWriteableNode
    {
        public PBXGroup(string name)
            : base(name)
        {
            this.Children = new System.Collections.Generic.List<XCodeNodeData>();
        }

        public string SourceTree
        {
            get;
            set;
        }

        public System.Collections.Generic.List<XCodeNodeData> Children
        {
            get;
            private set;
        }

#region IWriteableNode implementation

        void IWriteableNode.Write(System.IO.TextWriter writer)
        {
        }

#endregion
    }
}
