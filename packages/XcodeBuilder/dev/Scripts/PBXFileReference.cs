// <copyright file="PBXFileReference.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed class PBXFileReference : XCodeNodeData, IWriteableNode
    {
        public enum EType
        {
            Executable,
            DynamicLibrary,
            StaticLibrary,
            SourceFile,
            HeaderFile,
            Framework,
        }

        public PBXFileReference(string name, EType type, string path, System.Uri rootPath)
            : base(name)
        {
            this.Type = type;
            this.ShortPath = System.IO.Path.GetFileName(path);
            if (EType.Framework == type)
            {
                this.ShortPath += ".framework";
                // TODO: this is a hack, as I don't know the SDK at this stage
                // there might need to be more options added here
                this.RelativePath = "Platforms/MacOSX.platform/Developer/SDKs/MacOSX10.8.sdk/System/Library/Frameworks/" + this.ShortPath;
            }
            else
            {
                var relative = Opus.Core.RelativePathUtilities.GetPath(path, rootPath);
                this.RelativePath = relative;
            }
        }

        public string ShortPath
        {
            get;
            private set;
        }

        private string RelativePath
        {
            get;
            set;
        }

        public EType Type
        {
            get;
            private set;
        }

#region IWriteableNode implementation

        void IWriteableNode.Write(System.IO.TextWriter writer)
        {
            switch (this.Type)
            {
            case EType.Executable:
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; explicitFileType = \"compiled.mach-o.executable\"; includeInIndex = 0; path = {1}; sourceTree = BUILT_PRODUCTS_DIR; }};", this.UUID, this.ShortPath);
                break;

            case EType.DynamicLibrary:
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; explicitFileType = \"compiled.mach-o.dylib\"; includeInIndex = 0; path = {1}; sourceTree = BUILT_PRODUCTS_DIR; }};", this.UUID, this.ShortPath);
                break;

            case EType.StaticLibrary:
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; explicitFileType = archive.ar; includeInIndex = 0; path = {1}; sourceTree = BUILT_PRODUCTS_DIR; }};", this.UUID, this.ShortPath);
                break;

            case EType.SourceFile:
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; lastKnownFileType = sourcecode.cpp.cpp; name = {1}; path = {2}; sourceTree = SOURCE_ROOT; }};", this.UUID, this.ShortPath, this.RelativePath);
                break;

            case EType.HeaderFile:
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; lastKnownFileType = sourcecode.c.h; name = {1}; path = {2}; sourceTree = SOURCE_ROOT; }};", this.UUID, this.ShortPath, this.RelativePath);
                break;

            case EType.Framework:
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; lastKnownFileType = wrapper.framework; name = {1}; path = {2}; sourceTree = DEVELOPER_DIR; }};", this.UUID, this.ShortPath, this.RelativePath);
                break;

            default:
                throw new Opus.Core.Exception("Unknown PBXFileReference type");
            }
        }

#endregion
    }
}
