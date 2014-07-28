// <copyright file="PBXFileReference.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed class PBXFileReference :
        XcodeNodeData,
        IWriteableNode
    {
        public enum EType
        {
            ApplicationBundle,
            Executable,
            DynamicLibrary,
            ReferencedDynamicLibrary,
            StaticLibrary,
            ReferencedStaticLibrary,
            CSourceFile,
            CxxSourceFile,
            ObjCSourceFile,
            ObjCxxSourceFile,
            HeaderFile,
            Framework,
            PList,
            Text
        }

        public
        PBXFileReference(
            string name,
            EType type,
            string path,
            System.Uri rootPath) : base(name)
        {
            this.RootPath = rootPath;
            this.FullPath = path;
            this.SetType(type);
        }

        public void
        SetType(
            EType type)
        {
            this.Type = type;
            this.ShortPath = CalculateShortPath(type, this.FullPath);
            if (EType.Framework == type)
            {
                // TODO: this is a hack, as I don't know the SDK at this stage
                // there might need to be more options added here
                this.RelativePath = "Platforms/MacOSX.platform/Developer/SDKs/MacOSX10.8.sdk/System/Library/Frameworks/" + this.ShortPath;
            }
            else
            {
                var relative = Opus.Core.RelativePathUtilities.GetPath(this.FullPath, this.RootPath);
                if (relative.Contains("-"))
                {
                    relative = System.String.Format("\"{0}\"", relative);
                }
                this.RelativePath = relative;
            }
        }

        public static string
        CalculateShortPath(
            EType type,
            string path)
        {
            var shortPath = System.IO.Path.GetFileName(path);
            if (EType.Framework == type)
            {
                shortPath += ".framework";
            }
            else if (EType.ApplicationBundle == type)
            {
                shortPath += ".app";
            }
            return shortPath;
        }

        private System.Uri RootPath
        {
            get;
            set;
        }

        public string FullPath
        {
            get;
            private set;
        }

        public string ShortPath
        {
            get;
            private set;
        }

        public string RelativePath
        {
            get;
            private set;
        }

        public EType Type
        {
            get;
            private set;
        }

#region IWriteableNode implementation

        void
        IWriteableNode.Write(
            System.IO.TextWriter writer)
        {
            switch (this.Type)
            {
            case EType.ApplicationBundle:
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; explicitFileType = wrapper.application; includeInIndex = 0; path = {1}; sourceTree = BUILT_PRODUCTS_DIR; }};", this.UUID, this.ShortPath);
                break;

            case EType.Executable:
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; explicitFileType = \"compiled.mach-o.executable\"; includeInIndex = 0; path = {1}; sourceTree = BUILT_PRODUCTS_DIR; }};", this.UUID, this.ShortPath);
                break;

            case EType.DynamicLibrary:
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; explicitFileType = \"compiled.mach-o.dylib\"; includeInIndex = 0; path = {1}; sourceTree = BUILT_PRODUCTS_DIR; }};", this.UUID, this.ShortPath);
                break;

            case EType.ReferencedDynamicLibrary:
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; lastKnownFileType = \"compiled.mach-o.dylib\"; name = {1}; path = {2}; sourceTree = \"<group>\"; }};", this.UUID, this.ShortPath, this.RelativePath);
                break;

            case EType.StaticLibrary:
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; explicitFileType = archive.ar; includeInIndex = 0; path = {1}; sourceTree = BUILT_PRODUCTS_DIR; }};", this.UUID, this.ShortPath);
                break;

            case EType.ReferencedStaticLibrary:
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; lastKnownFileType = archive.ar; name = {1}; path = {2}; sourceTree = \"<group>\"; }};", this.UUID, this.ShortPath, this.RelativePath);
                break;

            case EType.CSourceFile:
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; lastKnownFileType = sourcecode.c.c; name = {1}; path = {2}; sourceTree = SOURCE_ROOT; }};", this.UUID, this.ShortPath, this.RelativePath);
                break;

            case EType.CxxSourceFile:
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; lastKnownFileType = sourcecode.cpp.cpp; name = {1}; path = {2}; sourceTree = SOURCE_ROOT; }};", this.UUID, this.ShortPath, this.RelativePath);
                break;

            case EType.ObjCSourceFile:
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; lastKnownFileType = sourcecode.c.objc; name = {1}; path = {2}; sourceTree = SOURCE_ROOT; }};", this.UUID, this.ShortPath, this.RelativePath);
                break;

            case EType.ObjCxxSourceFile:
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; lastKnownFileType = sourcecode.cpp.objcpp; name = {1}; path = {2}; sourceTree = SOURCE_ROOT; }};", this.UUID, this.ShortPath, this.RelativePath);
                break;

            case EType.HeaderFile:
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; lastKnownFileType = sourcecode.c.h; name = {1}; path = {2}; sourceTree = SOURCE_ROOT; }};", this.UUID, this.ShortPath, this.RelativePath);
                break;

            case EType.Framework:
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; lastKnownFileType = wrapper.framework; name = {1}; path = {2}; sourceTree = DEVELOPER_DIR; }};", this.UUID, this.ShortPath, this.RelativePath);
                break;

            case EType.PList:
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; lastKnownFileType = text.plist; name = {1}; path = {2}; sourceTree = \"<group>\"; }};", this.UUID, this.ShortPath, this.RelativePath);
                break;

            case EType.Text:
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; explicitFileType = text; name = {1}; includeInIndex = 0; path = {2}; sourceTree = \"<group>\"; }};", this.UUID, this.ShortPath, this.RelativePath);
                break;

            default:
                throw new Opus.Core.Exception("Unknown PBXFileReference type");
            }
        }

#endregion
    }
}
