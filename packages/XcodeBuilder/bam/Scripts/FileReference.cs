#region License
// Copyright (c) 2010-2016, Mark Final
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
    public static class EnumToStringExtensions
    {
        public static string
        AsString(
            this FileReference.EFileType type)
        {
            switch (type)
            {
            case FileReference.EFileType.SourceCodeC:
                return "sourcecode.c.c";

            case FileReference.EFileType.SourceCodeCxx:
                return "sourcecode.cpp.cpp";

            case FileReference.EFileType.SourceCodeObjC:
                return "sourcecode.c.objc";

            case FileReference.EFileType.SourceCodeObjCxx:
                return "sourcecode.cpp.objcpp";

            case FileReference.EFileType.HeaderFile:
                return "sourcecode.c.h";

            case FileReference.EFileType.Archive:
                return "archive.ar";

            case FileReference.EFileType.Executable:
                return "compiled.mach-o.executable";

            case FileReference.EFileType.DynamicLibrary:
                return "compiled.mach-o.dylib";

            case FileReference.EFileType.WrapperFramework:
                return "wrapper.framework";

            case FileReference.EFileType.ApplicationBundle:
                return "wrapper.application";

            case FileReference.EFileType.Project:
                return "wrapper.pb-project";

            case FileReference.EFileType.LexFile:
                return "sourcecode.lex";

            case FileReference.EFileType.YaccFile:
                return "sourcecode.yacc";

            case FileReference.EFileType.TextBasedDylibDefinition:
                return "sourcecode.text-based-dylib-definition";

            case FileReference.EFileType.TextFile:
                return "text";

            default:
                throw new Bam.Core.Exception("Unrecognized file type {0}", type.ToString());
            }
        }

        public static string
        AsString(
            this FileReference.ESourceTree sourceTree)
        {
            switch (sourceTree)
            {
            case FileReference.ESourceTree.NA:
                return "\"<unknown>\"";

            case FileReference.ESourceTree.Absolute:
                return "\"<absolute>\"";

            case FileReference.ESourceTree.Group:
                return "\"<group>\"";

            case FileReference.ESourceTree.SourceRoot:
                return "SOURCE_ROOT";

            case FileReference.ESourceTree.DeveloperDir:
                return "DEVELOPER_DIR";

            case FileReference.ESourceTree.BuiltProductsDir:
                return "BUILT_PRODUCTS_DIR";

            case FileReference.ESourceTree.SDKRoot:
                return "SDKROOT";

            default:
                throw new Bam.Core.Exception("Unknown source tree, {0}", sourceTree);
            }
        }
    }

    public sealed class FileReference :
        Object
    {
        public enum EFileType
        {
            SourceCodeC,
            SourceCodeCxx,
            SourceCodeObjC,
            SourceCodeObjCxx,
            HeaderFile,
            Archive,
            Executable,
            DynamicLibrary,
            WrapperFramework,
            ApplicationBundle,
            Project,
            YaccFile,
            LexFile,
            TextBasedDylibDefinition,
            TextFile
        }

        public enum ESourceTree
        {
            NA,       /* maps to <unknown> */
            Absolute, /* absolute path */
            Group,    /* group of things? (which group, where?) */
            SourceRoot, /* relative to project */
            DeveloperDir, /* relative to developer directory */
            BuiltProductsDir, /* relative to where products are built in project */
            SDKRoot /* relative to SDK root */
        }

        public FileReference(
            Bam.Core.TokenizedString path,
            EFileType type,
            Project project,
            bool explicitType = false,
            ESourceTree sourceTree = ESourceTree.NA,
            string relativePath = null)
            :
            base(project, path.Parse(), "PBXFileReference", type.ToString(), project.GUID, explicitType.ToString(), sourceTree.ToString(), relativePath)
        {
            this.Path = path;
            this.Type = type;
            this.SourceTree = sourceTree;
            this.ExplicitType = explicitType;
            this.RelativePath = relativePath;
        }

        public FileReference
        MakeLinkableAlias(
            Bam.Core.Module module,
            Project project)
        {
            return project.EnsureFileReferenceExists(
                module.CreateTokenizedString("$(packagename)/$CONFIGURATION/@filename($(0))", this.Path),
                this.Type,
                explicitType: false,
                sourceTree: ESourceTree.Group);
        }

        public Bam.Core.TokenizedString Path
        {
            get;
            private set;
        }

        public EFileType Type
        {
            get;
            private set;
        }

        private bool ExplicitType
        {
            get;
            set;
        }

        public ESourceTree SourceTree
        {
            get;
            private set;
        }

        private string RelativePath
        {
            get;
            set;
        }

        public void
        MakeApplicationBundle()
        {
            if (this.Type == EFileType.ApplicationBundle)
            {
                return;
            }
            if (this.Type != EFileType.Executable)
            {
                throw new Bam.Core.Exception("Can only make executables into application bundles");
            }
            this.Type = EFileType.ApplicationBundle;
        }

        public override void
        Serialize(
            System.Text.StringBuilder text,
            int indentLevel)
        {
            var leafname = System.IO.Path.GetFileName(this.Path.ToString());

            var indent = new string('\t', indentLevel);
            text.AppendFormat("{0}{1} /* {2} */ = {{isa = {3}; ", indent, this.GUID, leafname, this.IsA);
            if (this.ExplicitType)
            {
                text.AppendFormat("explicitFileType = {0}; ", this.Type.AsString());
            }
            else
            {
                text.AppendFormat("lastKnownFileType = {0}; ", this.Type.AsString());
            }

            text.AppendFormat("name = \"{0}\"; ", leafname);

            string path = null;
            switch (this.SourceTree)
            {
                case ESourceTree.NA:
                case ESourceTree.Absolute:
                case ESourceTree.SDKRoot:
                    path = this.Path.Parse();
                    break;

                case ESourceTree.BuiltProductsDir:
                    path = System.IO.Path.GetFileName(this.Path.Parse());
                    break;

                case ESourceTree.Group:
                case ESourceTree.SourceRoot:
                    path = (null != this.RelativePath) ? this.RelativePath : this.Path.Parse();
                    break;

                default:
                    throw new Bam.Core.Exception("Other source trees not handled yet");
            }
            text.AppendFormat("path = \"{0}\"; ", path);

            text.AppendFormat("sourceTree = {0}; ", this.SourceTree.AsString());
            text.AppendFormat("}};");
            text.AppendLine();
        }
    }
}
