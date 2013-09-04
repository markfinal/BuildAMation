// <copyright file="PBXNativeTargetSection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XCodeBuilder package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public sealed class PBXNativeTargetSection : IWriteableNode, System.Collections.IEnumerable
    {

        public PBXNativeTargetSection()
        {
            this.NativeTargets = new System.Collections.Generic.List<PBXNativeTarget>();
        }

        public void Add(PBXNativeTarget target)
        {
            lock (this.NativeTargets)
            {
                this.NativeTargets.Add(target);
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

            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXNativeTarget section */");
            foreach (var nativeTarget in this.NativeTargets)
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
