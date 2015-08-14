#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
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
            ReferencedExecutable,
            DynamicLibrary,
            ReferencedDynamicLibrary,
            Plugin,
            ReferencedPlugin,
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
            this.ChangeType(type);
        }

        public void
        ChangeType(
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
                var relative = Bam.Core.RelativePathUtilities.GetPath(this.FullPath, this.RootPath);
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

            case EType.ReferencedExecutable:
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; lastKnownFileType = \"compiled.mach-o.executable\"; name = {1}; path = {2}; sourceTree = \"<group>\"; }};", this.UUID, this.ShortPath, this.RelativePath);
                break;

            case EType.DynamicLibrary:
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; explicitFileType = \"compiled.mach-o.dylib\"; includeInIndex = 0; path = {1}; sourceTree = BUILT_PRODUCTS_DIR; }};", this.UUID, this.ShortPath);
                break;

            case EType.ReferencedDynamicLibrary:
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; lastKnownFileType = \"compiled.mach-o.dylib\"; name = {1}; path = {2}; sourceTree = \"<group>\"; }};", this.UUID, this.ShortPath, this.RelativePath);
                break;

            case EType.Plugin:
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; explicitFileType = \"compiled.mach-o.bundle\"; includeInIndex = 0; path = {1}; sourceTree = BUILT_PRODUCTS_DIR; }};", this.UUID, this.ShortPath);
                break;

            case EType.ReferencedPlugin:
                writer.WriteLine("\t\t{0} /* {1} */ = {{isa = PBXFileReference; lastKnownFileType = \"compiled.mach-o.bundle\"; name = {1}; path = {2}; sourceTree = \"<group>\"; }};", this.UUID, this.ShortPath, this.RelativePath);
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
                throw new Bam.Core.Exception("Unknown PBXFileReference type");
            }
        }

#endregion
    }
}
