// <copyright file="PBXGroupSection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed class PBXGroupSection :
        IWriteableNode,
        System.Collections.IEnumerable
    {
        public
        PBXGroupSection()
        {
            this.Groups = new Opus.Core.Array<PBXGroup>();
        }

        public PBXGroup
        Get(
            string name)
        {
            lock (this.Groups)
            {
                foreach (var group in this.Groups)
                {
                    if (group.Name == name)
                    {
                        return group;
                    }
                }

                var newGroup = new PBXGroup(name);
                this.Groups.Add(newGroup);
                return newGroup;
            }
        }

        private Opus.Core.Array<PBXGroup> Groups
        {
            get;
            set;
        }

#region IWriteableNode implementation
        void
        IWriteableNode.Write(
            System.IO.TextWriter writer)
        {
            if (this.Groups.Count == 0)
            {
                return;
            }

            var orderedList = new System.Collections.Generic.List<PBXGroup>(this.Groups);
            orderedList.Sort(
                delegate(PBXGroup p1, PBXGroup p2)
                {
                    return p1.UUID.CompareTo(p2.UUID);
                }
            );

            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXGroup section */");
            foreach (var group in orderedList)
            {
                (group as IWriteableNode).Write(writer);
            }
            writer.WriteLine("/* End PBXGroup section */");
        }
#endregion

#region IEnumerable implementation

        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
        {
            return this.Groups.GetEnumerator();
        }

#endregion
    }
}
