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

        public void Add(PBXFileReference fileRef)
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
            if (this.FileReferences.Count == 0)
            {
                return;
            }

            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXFileReference section */");
            foreach (var FileRef in this.FileReferences)
            {
                (FileRef as IWriteableNode).Write(writer);
            }
            writer.WriteLine("/* End PBXFileReference section */");
        }
#endregion
    }

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

    public sealed class PBXProject : XCodeNodeData, IWriteableNode
    {
        public PBXProject(string name)
            : base(name)
        {
            this.NativeTargets = new PBXNativeTargetSection();
            this.FileReferences = new PBXFileReferenceSection();
        }

        public PBXNativeTargetSection NativeTargets
        {
            get;
            private set;
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
            foreach (PBXNativeTarget target in this.NativeTargets)
            {
                writer.WriteLine("\t\t\t\t{0} /* {1} */,", target.UUID, target.Name);
            }
            writer.WriteLine("\t\t\t);");
            writer.WriteLine("\t\t};");
            writer.WriteLine("/* End PBXProject section */");

            (this.NativeTargets as IWriteableNode).Write(writer);
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
        public PBXFileReference(string name, string path)
            : base(name)
        {
            // TODO: should this always be stripped?
            this.Path = System.IO.Path.GetFileName(path);
        }

        public string Path
        {
            get;
            private set;
        }

        public bool IsExecutable
        {
            get;
            set;
        }

        public bool IsSourceCode
        {
            get;
            set;
        }

#region IWriteableNode implementation

        void IWriteableNode.Write(System.IO.TextWriter writer)
        {
            if (this.IsExecutable)
            {
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; explicitFileType = \"compiled.mach-o.executable\"; path = {2}; sourceTree = BUILT_PRODUCTS_DIR;}};", this.UUID, this.Name, this.Path);
            }
            else if (this.IsSourceCode)
            {
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; lastKnownFileType = sourcecode.cpp.cpp; path = {2}; sourceTree = \"<group>\";}};", this.UUID, this.Name, this.Path);
            }
            else
            {
                throw new Opus.Core.Exception("Unknown PBXFileReference type");
            }
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
