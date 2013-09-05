// <copyright file="PBXCopyFilesBuildPhaseSection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XCodeBuilder package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
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

            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXCopyFilesBuildPhase section */");
            foreach (var buildPhase in this.CopyFilesBuildPhases)
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
