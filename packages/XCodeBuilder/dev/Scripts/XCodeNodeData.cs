// <copyright file="XCodeNodeData.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XCodeBuilder package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public sealed class XCConfigurationListSection : IWriteableNode, System.Collections.IEnumerable
    {
        public XCConfigurationListSection()
        {
            this.ConfigurationLists = new System.Collections.Generic.List<XCConfigurationList>();
        }

        public void Add(XCConfigurationList configurationList)
        {
            lock (this.ConfigurationLists)
            {
                this.ConfigurationLists.Add(configurationList);
            }
        }

        public XCConfigurationList Get(string name)
        {
            lock (this.ConfigurationLists)
            {
                foreach (var configurationList in this.ConfigurationLists)
                {
                    if (configurationList.Name == name)
                    {
                        return configurationList;
                    }
                }

                var newConfigurationList = new XCConfigurationList(name);
                this.ConfigurationLists.Add(newConfigurationList);
                return newConfigurationList;
            }
        }

        private System.Collections.Generic.List<XCConfigurationList> ConfigurationLists
        {
            get;
            set;
        }

        #region IWriteableNode implementation
        void IWriteableNode.Write (System.IO.TextWriter writer)
        {
            if (this.ConfigurationLists.Count == 0)
            {
                return;
            }

            writer.WriteLine("");
            writer.WriteLine("/* Begin XCConfigurationList section */");
            foreach (var configurationList in this.ConfigurationLists)
            {
                (configurationList as IWriteableNode).Write(writer);
            }
            writer.WriteLine("/* End XCConfigurationList section */");
        }
        #endregion

        #region IEnumerable implementation

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return this.ConfigurationLists.GetEnumerator();
        }

        #endregion
    }

    public sealed class XCConfigurationList : XCodeNodeData, IWriteableNode
    {
        public XCConfigurationList(string name)
            : base(name)
        {
            this.BuildConfigurations = new System.Collections.Generic.List<XCBuildConfiguration>();
        }

        public XCodeNodeData Parent
        {
            get;
            set;
        }

        public void AddUnique(XCBuildConfiguration configuration)
        {
            lock (this.BuildConfigurations)
            {
                if (!this.BuildConfigurations.Contains(configuration))
                {
                    this.BuildConfigurations.Add(configuration);
                }
            }
        }

        private System.Collections.Generic.List<XCBuildConfiguration> BuildConfigurations
        {
            get;
            set;
        }

#region IWriteableNode implementation

        void IWriteableNode.Write (System.IO.TextWriter writer)
        {
            if (null == this.Parent)
            {
                throw new Opus.Core.Exception("Parent of this configuration list has not been set");
            }

            writer.WriteLine("\t\t{0} /* Build configuration list for {1} \"{2}\" */ = {{", this.UUID, this.Parent.GetType().Name, this.Name);
            writer.WriteLine("\t\t\tisa = XCConfigurationList;");
            writer.WriteLine("\t\t\tbuildConfigurations = (");
            foreach (var configuration in this.BuildConfigurations)
            {
                writer.WriteLine("\t\t\t\t{0} /* {1} */,", configuration.UUID, configuration.Name);
            }
            writer.WriteLine("\t\t\t);");
            writer.WriteLine("\t\t};");
        }

#endregion
    }

    public sealed class XCBuildConfigurationSection : IWriteableNode, System.Collections.IEnumerable
    {
        public XCBuildConfigurationSection()
        {
            this.BuildConfigurations = new System.Collections.Generic.List<XCBuildConfiguration>();
        }

        public void Add(XCBuildConfiguration target)
        {
            lock (this.BuildConfigurations)
            {
                this.BuildConfigurations.Add(target);
            }
        }

        public XCBuildConfiguration Get(string name)
        {
            lock(this.BuildConfigurations)
            {
                foreach (var buildConfiguration in this.BuildConfigurations)
                {
                    if (buildConfiguration.Name == name)
                    {
                        return buildConfiguration;
                    }
                }

                var newBuildConfiguration = new XCBuildConfiguration(name);
                this.Add(newBuildConfiguration);
                return newBuildConfiguration;
            }
        }

        private System.Collections.Generic.List<XCBuildConfiguration> BuildConfigurations
        {
            get;
            set;
        }

        #region IWriteableNode implementation
        void IWriteableNode.Write (System.IO.TextWriter writer)
        {
            if (this.BuildConfigurations.Count == 0)
            {
                return;
            }

            writer.WriteLine("");
            writer.WriteLine("/* Begin XCBuildConfiguration section */");
            foreach (var buildConfiguration in this.BuildConfigurations)
            {
                (buildConfiguration as IWriteableNode).Write(writer);
            }
            writer.WriteLine("/* End XCBuildConfiguration section */");
        }
        #endregion

        #region IEnumerable implementation

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return this.BuildConfigurations.GetEnumerator();
        }

        #endregion
    }

    public sealed class XCBuildConfiguration : XCodeNodeData, IWriteableNode
    {
        public XCBuildConfiguration(string name)
            : base(name)
        {}

#region IWriteableNode implementation

        void IWriteableNode.Write (System.IO.TextWriter writer)
        {
            writer.WriteLine("\t\t{0} /* {1} */ = {{", this.UUID, this.Name);
            writer.WriteLine("\t\t\tisa = XCBuildConfiguration;");
            writer.WriteLine("\t\t\tbuildSettings = {");
            writer.WriteLine("\t\t\t\tPRODUCT_NAME = \"$(TARGET_NAME)\";");
            writer.WriteLine("\t\t\t};");
            writer.WriteLine("\t\t\tname = {0};", this.Name);
            writer.WriteLine("\t\t};");
        }

#endregion
    }

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

    public sealed class PBXBuildFileSection : IWriteableNode, System.Collections.IEnumerable
    {
        public PBXBuildFileSection()
        {
            this.BuildFiles = new System.Collections.Generic.List<PBXBuildFile>();
        }

        public void Add(PBXBuildFile buildFile)
        {
            lock (this.BuildFiles)
            {
                this.BuildFiles.Add(buildFile);
            }
        }

        private System.Collections.Generic.List<PBXBuildFile> BuildFiles
        {
            get;
            set;
        }

#region IWriteableNode implementation
        void IWriteableNode.Write (System.IO.TextWriter writer)
        {
            if (this.BuildFiles.Count == 0)
            {
                return;
            }

            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXBuildFile section */");
            foreach (var buildFile in this.BuildFiles)
            {
                (buildFile as IWriteableNode).Write(writer);
            }
            writer.WriteLine("/* End PBXBuildFile section */");
        }
#endregion

#region IEnumerable implementation

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return this.BuildFiles.GetEnumerator();
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
            this.BuildFiles = new PBXBuildFileSection();
            this.BuildConfigurations = new XCBuildConfigurationSection();
            this.ConfigurationLists = new XCConfigurationListSection();
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

        public PBXBuildFileSection BuildFiles
        {
            get;
            private set;
        }

        public XCBuildConfigurationSection BuildConfigurations
        {
            get;
            private set;
        }

        public XCConfigurationListSection ConfigurationLists
        {
            get;
            private set;
        }

        private XCConfigurationList _BuildConfigurationList;
        public XCConfigurationList BuildConfigurationList
        {
            get
            {
                return this._BuildConfigurationList;
            }

            set
            {
                this._BuildConfigurationList = value;
                // TODO: can't do this yet as we're sharing configuration lists
                //value.Parent = this;
            }
        }

#region IWriteableNode implementation

        void IWriteableNode.Write(System.IO.TextWriter writer)
        {
            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXProject section */");
            writer.WriteLine("\t\t{0} /* Project object */ = {{", this.UUID);
            writer.WriteLine("\t\t\tisa = PBXProject;");
            writer.WriteLine("\t\t\tbuildConfigurationList = {0} /* Build configuration list for PBXProject \"{1}\" */;", this.BuildConfigurationList.UUID, this.Name);
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
            (this.BuildFiles as IWriteableNode).Write(writer);
            (this.BuildConfigurations as IWriteableNode).Write(writer);
            (this.ConfigurationLists as IWriteableNode).Write(writer);
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

        private XCConfigurationList _BuildConfigurationList;
        public XCConfigurationList BuildConfigurationList
        {
            get
            {
                return this._BuildConfigurationList;
            }

            set
            {
                this._BuildConfigurationList = value;
                value.Parent = this;
            }
        }

#region IWriteableNode implementation

        void IWriteableNode.Write(System.IO.TextWriter writer)
        {
            writer.WriteLine("\t\t{0} /* {1} */ = {{", this.UUID, this.Name);
            writer.WriteLine("\t\t\tisa = PBXNativeTarget;");
            writer.WriteLine("\t\t\tbuildConfigurationList = {0} /* Build configuration list for PBXNativeTarget \"{1}\" */;", this.BuildConfigurationList.UUID, this.Name);
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

        public PBXFileReference FileReference
        {
            get;
            set;
        }

#region IWriteableNode implementation

        void IWriteableNode.Write(System.IO.TextWriter writer)
        {
            writer.WriteLine("\t\t{0} /* {1} in TODO */ = {{ isa = PBXBuildFile; fileRef = {2} /* {1} */; }};", this.UUID, this.FileReference.Path, this.FileReference.UUID);
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
