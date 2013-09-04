// <copyright file="PBXGroupSection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XCodeBuilder package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public sealed class PBXGroupSection : IWriteableNode, System.Collections.IEnumerable
    {
        public PBXGroupSection()
        {
            this.Groups = new System.Collections.Generic.List<PBXGroup>();
        }

        public void Add(PBXGroup group)
        {
            lock (this.Groups)
            {
                this.Groups.Add(group);
            }
        }

        private System.Collections.Generic.List<PBXGroup> Groups
        {
            get;
            set;
        }

#region IWriteableNode implementation
        void IWriteableNode.Write (System.IO.TextWriter writer)
        {
            if (this.Groups.Count == 0)
            {
                return;
            }

            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXGroup section */");
            foreach (var group in this.Groups)
            {
                (group as IWriteableNode).Write(writer);
            }
            writer.WriteLine("/* End PBXGroup section */");
        }
#endregion

#region IEnumerable implementation

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return this.Groups.GetEnumerator();
        }

#endregion
    }
}
