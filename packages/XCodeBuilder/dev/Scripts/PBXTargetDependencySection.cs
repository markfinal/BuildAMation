// <copyright file="PBXTargetDependencySection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XCodeBuilder package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public sealed class PBXTargetDependencySection : IWriteableNode, System.Collections.IEnumerable
    {
        public PBXTargetDependencySection()
        {
            this.TargetDependencies = new System.Collections.Generic.List<PBXTargetDependency>();
        }

        public void Add(PBXTargetDependency dependency)
        {
            lock (this.TargetDependencies)
            {
                this.TargetDependencies.Add(dependency);
            }
        }

        private System.Collections.Generic.List<PBXTargetDependency> TargetDependencies
        {
            get;
            set;
        }

#region IWriteableNode implementation
        void IWriteableNode.Write (System.IO.TextWriter writer)
        {
            if (this.TargetDependencies.Count == 0)
            {
                return;
            }

            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXTargetDependency section */");
            foreach (var dependency in this.TargetDependencies)
            {
                (dependency as IWriteableNode).Write(writer);
            }
            writer.WriteLine("/* End PBXTargetDependency section */");
        }
#endregion

#region IEnumerable implementation

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return this.TargetDependencies.GetEnumerator();
        }

#endregion
    }
}
