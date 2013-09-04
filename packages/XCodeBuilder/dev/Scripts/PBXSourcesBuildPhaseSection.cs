// <copyright file="PBXSourcesBuildPhaseSection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XCodeBuilder package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public sealed class PBXSourcesBuildPhaseSection : IWriteableNode, System.Collections.IEnumerable
    {
        public PBXSourcesBuildPhaseSection()
        {
            this.SourcesBuildPhase = new System.Collections.Generic.List<PBXSourcesBuildPhase>();
        }

        public void Add(PBXSourcesBuildPhase buildPhase)
        {
            lock (this.SourcesBuildPhase)
            {
                this.SourcesBuildPhase.Add(buildPhase);
            }
        }

        public PBXSourcesBuildPhase Get(string name, string moduleName)
        {
            lock (this.SourcesBuildPhase)
            {
                foreach (var buildPhase in this.SourcesBuildPhase)
                {
                    if ((buildPhase.Name == name) && (buildPhase.ModuleName == moduleName))
                    {
                        return buildPhase;
                    }
                }

                var newBuildPhase = new PBXSourcesBuildPhase(name, moduleName);
                this.SourcesBuildPhase.Add(newBuildPhase);
                return newBuildPhase;
            }
        }

        private System.Collections.Generic.List<PBXSourcesBuildPhase> SourcesBuildPhase
        {
            get;
            set;
        }

#region IWriteableNode implementation
        void IWriteableNode.Write (System.IO.TextWriter writer)
        {
            if (this.SourcesBuildPhase.Count == 0)
            {
                return;
            }

            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXSourcesBuildPhase section */");
            foreach (var buildPhase in this.SourcesBuildPhase)
            {
                (buildPhase as IWriteableNode).Write(writer);
            }
            writer.WriteLine("/* End PBXSourcesBuildPhase section */");
        }
#endregion

#region IEnumerable implementation

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return this.SourcesBuildPhase.GetEnumerator();
        }

#endregion
    }
}
