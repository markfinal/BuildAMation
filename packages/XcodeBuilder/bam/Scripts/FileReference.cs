#region License
// Copyright (c) 2010-2019, Mark Final
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
    /// <summary>
    /// Extensions to convert enumerations to strings.
    /// </summary>
    static class EnumToStringExtensions
    {
        /// <summary>
        /// Extension function to convert FileReference.FileType to string.
        /// </summary>
        /// <param name="type">The type of the FileReference.</param>
        /// <returns>Stringified type.</returns>
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

            case FileReference.EFileType.Assembler:
                return "sourecode.asm";

            case FileReference.EFileType.ZipArchive:
                return "archive.zip";

            case FileReference.EFileType.MetalShaderSource:
                return "sourcecode.metal";

            case FileReference.EFileType.GLSLShaderSource:
                return "sourcecode.glsl";

            default:
                throw new Bam.Core.Exception($"Unrecognized file type {type.ToString()}");
            }
        }

        /// <summary>
        /// Extension function to convert FileReference.ESourceTree to string.
        /// </summary>
        /// <param name="sourceTree">The source tree of the FileReference.</param>
        /// <returns>Stringified source tree.</returns>
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
                throw new Bam.Core.Exception($"Unknown source tree, {sourceTree}");
            }
        }
    }

    /// <summary>
    /// Class representing a PBXFileReference in an Xcode project.
    /// </summary>
    sealed class FileReference :
        Object
    {
        /// <summary>
        /// The file type options
        /// </summary>
        public enum EFileType
        {
            SourceCodeC,                //!< C source code
            SourceCodeCxx,              //!< C++ source code
            SourceCodeObjC,             //!< Objective C source code
            SourceCodeObjCxx,           //!< Objective C++ source code
            HeaderFile,                 //!< Header file
            Archive,                    //!< Static library archive
            Executable,                 //!< Executable file
            DynamicLibrary,             //!< Dylib
            WrapperFramework,           //!< Framework
            ApplicationBundle,          //!< Application bundle
            Project,                    //!< Xcode project file
            YaccFile,                   //!< Yacc bison file
            LexFile,                    //!< Lex flex file
            TextBasedDylibDefinition,   //!< TBD (Text Based Dylib)
            TextFile,                   //!< Text file
            Assembler,                  //!< Assembler source
            ZipArchive,                 //!< Zip archive
            MetalShaderSource,          //!< Metal shader source
            GLSLShaderSource            //!< GLSL shader source
        }

        /// <summary>
        /// The source tree options
        /// </summary>
        public enum ESourceTree
        {
            NA,               //<! maps to <unknown>
            Absolute,         //<! absolute path
            Group,            //<! group of things? (which group, where?)
            SourceRoot,       //<! relative to project
            DeveloperDir,     //<! relative to developer directory
            BuiltProductsDir, //<! relative to where products are built in project
            SDKRoot           //<! relative to SDK root
        }

        /// <summary>
        /// Construct an instance
        /// </summary>
        /// <param name="path">The TokenizedString to the file reference</param>
        /// <param name="type">Type of filereference</param>
        /// <param name="project">Project to add the filereference to</param>
        /// <param name="explicitType">Optional: Is this an explicit type? Defaults to false.</param>
        /// <param name="sourceTree">Optional: Which source tree does the file reference belong in? Defaults to unknown.</param>
        /// <param name="relativePath">Optional: The relative path to use in the Xcode project. Defaults to null.</param>
        public FileReference(
            Bam.Core.TokenizedString path,
            EFileType type,
            Project project,
            bool explicitType = false,
            ESourceTree sourceTree = ESourceTree.NA,
            string relativePath = null)
            :
            base(project, path.ToString(), "PBXFileReference", type.ToString(), project.GUID, explicitType.ToString(), sourceTree.ToString(), relativePath)
        {
            this.Path = path;
            this.Type = type;
            this.SourceTree = sourceTree;
            this.ExplicitType = explicitType;
            this.RelativePath = relativePath;
        }

        /// <summary>
        /// Make another FileReference that can be linked across projects.
        /// </summary>
        /// <param name="module">Module in which to create the linked TokenizedString</param>
        /// <param name="project">Project in which to add the linked FileReference.</param>
        /// <returns></returns>
        public FileReference
        MakeLinkableAlias(
            Bam.Core.Module module,
            Project project)
        {
            var alias = module.CreateTokenizedString("$(packagename)/$CONFIGURATION/@filename($(0))", this.Path);
            if (!alias.IsParsed)
            {
                alias.Parse();
            }
            return project.EnsureFileReferenceExists(
                alias,
                this.Type,
                explicitType: false,
                sourceTree: ESourceTree.Group);
        }

        /// <summary>
        /// Get the path of the file reference.
        /// </summary>
        public Bam.Core.TokenizedString Path { get; private set; }

        /// <summary>
        /// Get the type of the file reference.
        /// </summary>
        public EFileType Type { get; private set; }
        private bool ExplicitType { get; set; }

        /// <summary>
        /// Get the source tree of the file reference
        /// </summary>
        public ESourceTree SourceTree { get; private set; }
        private string RelativePath { get; set; }

        /// <summary>
        /// Convert the file reference from an executable to an application bundle.
        /// </summary>
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

        /// <summary>
        /// Serialize the file reference.
        /// </summary>
        /// <param name="text">StringBuilder to write to.</param>
        /// <param name="indentLevel">Number of tabs to indent by.</param>
        public override void
        Serialize(
            System.Text.StringBuilder text,
            int indentLevel)
        {
            var leafname = System.IO.Path.GetFileName(this.Path.ToString());

            var indent = new string('\t', indentLevel);
            text.Append($"{indent}{this.GUID} /* {leafname} */ = {{isa = {this.IsA}; ");
            if (this.ExplicitType)
            {
                text.Append($"explicitFileType = {this.Type.AsString()}; ");
            }
            else
            {
                text.Append($"lastKnownFileType = {this.Type.AsString()}; ");
            }

            text.Append($"name = \"{leafname}\"; ");

            string path = null;
            switch (this.SourceTree)
            {
                case ESourceTree.NA:
                case ESourceTree.Absolute:
                case ESourceTree.SDKRoot:
                    path = this.Path.ToString();
                    break;

                case ESourceTree.BuiltProductsDir:
                    path = System.IO.Path.GetFileName(this.Path.ToString());
                    break;

                case ESourceTree.Group:
                case ESourceTree.SourceRoot:
                    path = (null != this.RelativePath) ? this.RelativePath : this.Path.ToString();
                    break;

                default:
                    throw new Bam.Core.Exception("Other source trees not handled yet");
            }
            text.Append($"path = \"{path}\"; ");

            text.Append($"sourceTree = {this.SourceTree.AsString()}; ");
            text.AppendLine("};");
        }
    }
}
