// <copyright file="PBXGroup.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed class PBXGroup :
        XCodeNodeData,
        IWriteableNode
    {
        public
        PBXGroup(
            string name) : base(name)
        {
            this.Children = new Opus.Core.Array<XCodeNodeData>();
        }

        public string Path
        {
            get;
            set;
        }

        public string SourceTree
        {
            get;
            set;
        }

        public Opus.Core.Array<XCodeNodeData> Children
        {
            get;
            private set;
        }

#region IWriteableNode implementation

        void
        IWriteableNode.Write(
            System.IO.TextWriter writer)
        {
            if (0 == this.Children.Count)
            {
                return;
            }
            if (string.IsNullOrEmpty(this.SourceTree))
            {
                throw new Opus.Core.Exception("Source tree not set");
            }

            if (string.IsNullOrEmpty(this.Name))
            {
                // this is the main group
                writer.WriteLine("\t\t{0} = {{", this.UUID);
            }
            else
            {
                writer.WriteLine("\t\t{0} /* {1} */ = {{", this.UUID, this.Name);
            }
            writer.WriteLine("\t\t\tisa = PBXGroup;");
            writer.WriteLine("\t\t\tchildren = (");
            foreach (var child in this.Children)
            {
                if (child is PBXFileReference)
                {
                    writer.WriteLine("\t\t\t\t{0} /* {1} */,", child.UUID, (child as PBXFileReference).ShortPath);
                }
                else
                {
                    writer.WriteLine("\t\t\t\t{0} /* {1} */,", child.UUID, child.Name);
                }
            }
            writer.WriteLine("\t\t\t);");
            if (string.IsNullOrEmpty(this.Path))
            {
                if (!string.IsNullOrEmpty(this.Name))
                {
                    writer.WriteLine("\t\t\tname = {0};", this.Name);
                }
            }
            else
            {
                writer.WriteLine("\t\t\tpath = {0};", this.Path);
            }
            writer.WriteLine("\t\t\tsourceTree = \"{0}\";", this.SourceTree);
            writer.WriteLine("\t\t};");
        }

#endregion
    }
}
