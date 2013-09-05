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
                if (child is PBXBuildFile)
                {
                    writer.WriteLine("\t\t\t\t{0} /* {1} */,", child.UUID, System.IO.Path.GetFileName((child as PBXBuildFile).FileReference.ShortPath));
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
