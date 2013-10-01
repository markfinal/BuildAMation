// <copyright file="PBXCopyFilesBuildPhaseSection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed class PBXCopyFilesBuildPhaseSection : IWriteableNode, System.Collections.IEnumerable
    {
        public PBXCopyFilesBuildPhaseSection()
        {
            this.CopyFilesBuildPhases = new System.Collections.Generic.List<PBXCopyFilesBuildPhase>();
        }

        public void Add(PBXCopyFilesBuildPhase buildPhase)
        {
            lock (this.CopyFilesBuildPhases)
            {
                this.CopyFilesBuildPhases.Add(buildPhase);
            }
        }

        public PBXCopyFilesBuildPhase Get(string name, string moduleName)
        {
            lock (this.CopyFilesBuildPhases)
            {
                foreach (var buildPhase in this.CopyFilesBuildPhases)
                {
                    if ((buildPhase.Name == name) && (buildPhase.ModuleName == moduleName))
                    {
                        return buildPhase;
                    }
                }

                var newBuildPhase = new PBXCopyFilesBuildPhase(name, moduleName);
                this.CopyFilesBuildPhases.Add(newBuildPhase);
                return newBuildPhase;
            }
        }

        private System.Collections.Generic.List<PBXCopyFilesBuildPhase> CopyFilesBuildPhases
        {
            get;
            set;
        }

#region IWriteableNode implementation
        void IWriteableNode.Write (System.IO.TextWriter writer)
        {
            if (this.CopyFilesBuildPhases.Count == 0)
            {
                return;
            }

            var orderedList = new System.Collections.Generic.List<PBXCopyFilesBuildPhase>(this.CopyFilesBuildPhases);
            orderedList.Sort(
                delegate(PBXCopyFilesBuildPhase p1, PBXCopyFilesBuildPhase p2)
                {
                    return p1.UUID.CompareTo(p2.UUID);
                }
            );

            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXCopyFilesBuildPhase section */");
            foreach (var buildPhase in orderedList)
            {
                (buildPhase as IWriteableNode).Write(writer);
            }
            writer.WriteLine("/* End PBXCopyFilesBuildPhase section */");
        }
#endregion

#region IEnumerable implementation

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return this.CopyFilesBuildPhases.GetEnumerator();
        }

#endregion
    }
}
