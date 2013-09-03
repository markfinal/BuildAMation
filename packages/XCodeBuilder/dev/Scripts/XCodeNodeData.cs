// <copyright file="XCodeNodeData.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XCodeBuilder package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public sealed class PBXFileReferenceSection : IWriteableNode
    {
        public PBXFileReferenceSection()
        {
            this.FileReferences = new System.Collections.Generic.List<PBXFileReference>();
        }

        public void AddFileRef(PBXFileReference fileRef)
        {
            lock (this.FileReferences)
            {
                this.FileReferences.Add(fileRef);
            }
        }

        private System.Collections.Generic.List<PBXFileReference> FileReferences
        {
            get;
            set;
        }

#region IWriteableNode implementation
        void IWriteableNode.Write (System.IO.TextWriter writer)
        {
            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXFileReference section */");
            foreach (var FileRef in this.FileReferences)
            {
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference;}};", FileRef.UUID, FileRef.Name);
            }
            writer.WriteLine("/* End PBXFileReference section */");
        }
#endregion
    }

    public sealed class PBXProject : XCodeNodeData, IWriteableNode
    {
        public PBXProject(string name)
            : base(name)
        {
            this.NativeTargets = new System.Collections.Generic.List<PBXNativeTarget>();
            this.FileReferences = new PBXFileReferenceSection();
        }

        private System.Collections.Generic.List<PBXNativeTarget> NativeTargets
        {
            get;
            set;
        }

        public void AddNativeTarget(PBXNativeTarget target)
        {
            lock (this.NativeTargets)
            {
                this.NativeTargets.Add(target);
            }
        }

        public PBXFileReferenceSection FileReferences
        {
            get;
            private set;
        }

#region IWriteableNode implementation

        void IWriteableNode.Write(System.IO.TextWriter writer)
        {
            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXProject section */");
            writer.WriteLine("\t\t{0} /* Project object */ = {{", this.UUID);
            writer.WriteLine("\t\t\tisa = PBXProject;");
            writer.WriteLine("\t\t\ttargets = (");
            foreach (var target in this.NativeTargets)
            {
                writer.WriteLine("\t\t\t\t{0} /* {1} */,", target.UUID, target.Name);
            }
            writer.WriteLine("\t\t\t);");
            writer.WriteLine("\t\t};");
            writer.WriteLine("/* End PBXProject section */");

            foreach (var target in this.NativeTargets)
            {
                (target as IWriteableNode).Write(writer);
            }

            (this.FileReferences as IWriteableNode).Write(writer);
        }
#endregion
    }

    public sealed class PBXNativeTarget : XCodeNodeData, IWriteableNode
    {
        public PBXNativeTarget(string name)
            : base (name)
        {}

        public PBXFileReference ProductReference
        {
            get;
            set;
        }

#region IWriteableNode implementation

        void IWriteableNode.Write(System.IO.TextWriter writer)
        {
            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXNativeTarget section */");
            writer.WriteLine("\t\t{0} /* {1} */ = {{", this.UUID, this.Name);
            writer.WriteLine("\t\t\tisa = PBXNativeTarget;");
            writer.WriteLine("\t\t\tbuildPhases = (");
            writer.WriteLine("\t\t\t);");
            writer.WriteLine("\t\t\tbuildRules = (");
            writer.WriteLine("\t\t\t);");
            writer.WriteLine("\t\t\tdependencies = (");
            writer.WriteLine("\t\t\t);");
            writer.WriteLine("\t\t\tname = {0};", this.Name);
            writer.WriteLine("\t\t\tproductType= \"com.apple.product-type.tool\";");
            writer.WriteLine("\t\t\tproductReference = {0};", this.ProductReference.UUID);
            writer.WriteLine("\t\t};");
            writer.WriteLine("/* End PBXNativeTarget section */");
        }
#endregion
    }

    public sealed class PBXBuildFile : XCodeNodeData, IWriteableNode
    {
        public PBXBuildFile(string name)
            : base(name)
        {}

#region IWriteableNode implementation

        void IWriteableNode.Write(System.IO.TextWriter writer)
        {
            throw new System.NotImplementedException ();
        }

#endregion
    }

    public sealed class PBXFileReference : XCodeNodeData, IWriteableNode
    {
        public PBXFileReference(string name)
            : base(name)
        {}

#region IWriteableNode implementation

        void IWriteableNode.Write(System.IO.TextWriter writer)
        {
            throw new System.NotImplementedException ();
        }

#endregion
    }

    public interface IWriteableNode
    {
        void Write(System.IO.TextWriter writer);
    }

    public abstract class XCodeNodeData
    {
        protected XCodeNodeData(string name)
        {
            this.Name = name;
            this.UUID = this.Generate96BitUUID();
        }

        public string Name
        {
            get;
            private set;
        }

        public string UUID
        {
            get;
            private set;
        }

        private string Generate96BitUUID()
        {
            var guid = System.Guid.NewGuid();
            var toString = guid.ToString("N").ToUpper(); // this is 32 characters long

            // cannot create a 24-char (96 bit) GUID, so just chop off the front and rear 4 characters
            // this ought to be random enough
            var toString24Chars = toString.Substring(4, 24);
            return toString24Chars;
        }
    }
}
