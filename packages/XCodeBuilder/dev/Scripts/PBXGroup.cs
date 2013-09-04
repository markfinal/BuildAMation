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
            if (0 == this.Children.Count)
            {
                return;
            }
            if (this.SourceTree.Equals(string.Empty))
            {
                throw new Opus.Core.Exception("Source tree not set");
            }

            writer.WriteLine("\t\t{0} /* {1} */ = {{", this.UUID, this.Name);
            writer.WriteLine("\t\t\tisa = PBXGroup;");
            writer.WriteLine("\t\t\tchildren = (");
            foreach (var child in this.Children)
            {
                writer.WriteLine("\t\t\t\t{0} /* {1} */,", child.UUID, child.Name);
            }
            writer.WriteLine("\t\t\t);");
            writer.WriteLine("\t\t\tsourceTree = \"{0}\";", this.SourceTree);
            writer.WriteLine("\t\t};");
        }

#endregion
    }
}
