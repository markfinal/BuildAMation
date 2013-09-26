// <copyright file="PBXTargetDependencySection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed class PBXTargetDependencySection : IWriteableNode, System.Collections.IEnumerable
    {
        public PBXTargetDependencySection()
        {
            this.TargetDependencies = new System.Collections.Generic.List<PBXTargetDependency>();
        }

        public PBXTargetDependency Get(string name, PBXNativeTarget nativeTarget)
        {
            lock (this.TargetDependencies)
            {
                foreach (var dependency in this.TargetDependencies)
                {
                    if ((dependency.Name == name) && (dependency.NativeTarget == nativeTarget))
                    {
                        return dependency;
                    }
                }

                var newDependency = new PBXTargetDependency(name, nativeTarget);
                this.TargetDependencies.Add(newDependency);
                return newDependency;
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
