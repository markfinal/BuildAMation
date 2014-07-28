// <copyright file="PBXTargetDependency.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed class PBXTargetDependency :
        XcodeNodeData,
        IWriteableNode
    {
        public
        PBXTargetDependency(
            string name,
            PBXNativeTarget nativeTarget) : base(name)
        {
            this.NativeTarget = nativeTarget;
        }

        public PBXNativeTarget NativeTarget
        {
            get;
            private set;
        }

        public PBXContainerItemProxy TargetProxy
        {
            get;
            set;
        }

#region IWriteableNode implementation

        void
        IWriteableNode.Write(
            System.IO.TextWriter writer)
        {
            if (this.NativeTarget == null)
            {
                throw new Opus.Core.Exception("Native target not set on this dependency");
            }
            if (this.TargetProxy == null)
            {
                throw new Opus.Core.Exception("Target proxy not set on this dependency");
            }

            writer.WriteLine("\t\t{0} /* PBXTargetDependency */ = {{", this.UUID);
            writer.WriteLine("\t\t\tisa = PBXTargetDependency;");
            writer.WriteLine("\t\t\ttarget = {0} /* {1} */;", this.NativeTarget.UUID, this.NativeTarget.Name);
            writer.WriteLine("\t\t\ttargetProxy = {0} /* {1} */;", this.TargetProxy.UUID, this.TargetProxy.GetType().Name);
            writer.WriteLine("\t\t};");
        }

#endregion
    }
}
