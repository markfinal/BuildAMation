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
            StaticLibrary,
            SourceFile,
            HeaderFile
        }

        public PBXFileReference(string name, EType type, string path, System.Uri rootPath)
            : base(name)
        {
            this.Type = type;
            this.ShortPath = System.IO.Path.GetFileName(path);

            var relative = Opus.Core.RelativePathUtilities.GetPath(path, rootPath);
            this.RelativePath = relative;
            Opus.Core.Log.MessageAll("path {0}: {1}", path, relative);
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

        private EType Type
        {
            get;
            set;
        }

#region IWriteableNode implementation

        void IWriteableNode.Write(System.IO.TextWriter writer)
        {
            switch (this.Type)
            {
            case EType.Executable:
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; explicitFileType = \"compiled.mach-o.executable\"; includeInIndex = 0; path = {1}; sourceTree = BUILT_PRODUCTS_DIR; }};", this.UUID, this.ShortPath);
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

            default:
                throw new Opus.Core.Exception("Unknown PBXFileReference type");
            }
        }

#endregion
    }
}
