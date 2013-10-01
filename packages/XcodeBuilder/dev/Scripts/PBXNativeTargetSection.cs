// <copyright file="PBXNativeTargetSection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed class PBXNativeTargetSection : IWriteableNode, System.Collections.IEnumerable
    {
        public PBXNativeTargetSection()
        {
            this.NativeTargets = new System.Collections.Generic.List<PBXNativeTarget>();
        }

        public PBXNativeTarget Get(string name, PBXNativeTarget.EType type)
        {
            lock(this.NativeTargets)
            {
                foreach (var target in this.NativeTargets)
                {
                    if ((target.Name == name) && (target.Type == type))
                    {
                        return target;
                    }
                }

                var newTarget = new PBXNativeTarget(name, type);
                this.NativeTargets.Add(newTarget);
                return newTarget;
            }
        }

        private System.Collections.Generic.List<PBXNativeTarget> NativeTargets
        {
            get;
            set;
        }

#region IWriteableNode implementation
        void IWriteableNode.Write (System.IO.TextWriter writer)
        {
            if (this.NativeTargets.Count == 0)
            {
                return;
            }

            var orderedList = new System.Collections.Generic.List<PBXNativeTarget>(this.NativeTargets);
            orderedList.Sort(
                delegate(PBXNativeTarget p1, PBXNativeTarget p2)
                {
                    return p1.UUID.CompareTo(p2.UUID);
                }
            );

            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXNativeTarget section */");
            foreach (var nativeTarget in orderedList)
            {
                (nativeTarget as IWriteableNode).Write(writer);
            }
            writer.WriteLine("/* End PBXNativeTarget section */");
        }
#endregion

#region IEnumerable implementation

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return this.NativeTargets.GetEnumerator();
        }

#endregion
    }
}
