// <copyright file="PBXShellScriptBuildPhaseSection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed class PBXShellScriptBuildPhaseSection : IWriteableNode, System.Collections.IEnumerable
    {
        public PBXShellScriptBuildPhaseSection()
        {
            this.ShellScriptBuildPhases = new System.Collections.Generic.List<PBXShellScriptBuildPhase>();
        }

        public void Add(PBXShellScriptBuildPhase buildPhase)
        {
            lock (this.ShellScriptBuildPhases)
            {
                this.ShellScriptBuildPhases.Add(buildPhase);
            }
        }

        public PBXShellScriptBuildPhase Get(string name, string moduleName)
        {
            lock (this.ShellScriptBuildPhases)
            {
                foreach (var buildPhase in this.ShellScriptBuildPhases)
                {
                    if ((buildPhase.Name == name) && (buildPhase.ModuleName == moduleName))
                    {
                        return buildPhase;
                    }
                }

                var newBuildPhase = new PBXShellScriptBuildPhase(name, moduleName);
                this.ShellScriptBuildPhases.Add(newBuildPhase);
                return newBuildPhase;
            }
        }

        private System.Collections.Generic.List<PBXShellScriptBuildPhase> ShellScriptBuildPhases
        {
            get;
            set;
        }

#region IWriteableNode implementation
        void IWriteableNode.Write (System.IO.TextWriter writer)
        {
            if (this.ShellScriptBuildPhases.Count == 0)
            {
                return;
            }

            var orderedList = new System.Collections.Generic.List<PBXShellScriptBuildPhase>(this.ShellScriptBuildPhases);
            orderedList.Sort(
                delegate(PBXShellScriptBuildPhase p1, PBXShellScriptBuildPhase p2)
                {
                    return p1.UUID.CompareTo(p2.UUID);
                }
            );

            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXShellScriptBuildPhase section */");
            foreach (var buildPhase in orderedList)
            {
                (buildPhase as IWriteableNode).Write(writer);
            }
            writer.WriteLine("/* End PBXShellScriptBuildPhase section */");
        }
#endregion

#region IEnumerable implementation

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return this.ShellScriptBuildPhases.GetEnumerator();
        }

#endregion
    }
}
