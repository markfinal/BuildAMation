// <copyright file="PBXTargetDependency.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XCodeBuilder package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public sealed class PBXTargetDependency : XCodeNodeData, IWriteableNode
    {
        public PBXTargetDependency(string name, PBXNativeTarget nativeTarget)
            : base(name)
        {
            this.NativeTarget = nativeTarget;
        }

        private PBXNativeTarget NativeTarget
        {
            get;
            set;
        }

        // TODO: Need targetProxy

#region IWriteableNode implementation

        void IWriteableNode.Write(System.IO.TextWriter writer)
        {
            if (this.NativeTarget == null)
            {
                throw new Opus.Core.Exception("Native target not set on this dependency");
            }

            writer.WriteLine("\t\t{0} /* {1} */ = {{", this.UUID, this.Name);
            writer.WriteLine("\t\t\tisa = PBXTargetDependency;");
            writer.WriteLine("\t\t\ttarget = {0} /* {1} */;", this.NativeTarget.UUID, this.NativeTarget.Name);
            writer.WriteLine("\t\t};");
        }

#endregion
    }
}
