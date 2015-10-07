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
    public sealed class FileReference :
        Object
    {
        public enum EFileType
        {
            SourceCodeC,
            HeaderFile,
            Archive,
            Executable,
            DynamicLibrary,
            WrapperFramework,
            ApplicationBundle
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

        private FileReference()
        {
            this.IsA = "PBXFileReference";
        }

        public FileReference(
            Bam.Core.TokenizedString path,
            EFileType type,
            Project project,
            bool explicitType = false,
            ESourceTree sourceTree = ESourceTree.NA)
            :
            this()
        {
            this.Path = path;
            this.Type = type;
            this.Project = project;
            this.SourceTree = sourceTree;
            this.ExplicitType = explicitType;
            this.LinkedTo = null;
        }

        public FileReference(
            FileReference other,
            Project owningProject)
            :
            this()
        {
            this.Path = other.Path;
            this.Type = other.Type;
            this.Project = owningProject;
            // TODO: Linked FileReferences should be <group> and non-explicit
            this.SourceTree = other.SourceTree;
            this.ExplicitType = other.ExplicitType;
            this.LinkedTo = other;
        }

        /// <summary>
        /// This generates a new GUID
        /// or returns null if the target is in the current project
        /// </summary>
        /// <param name="project"></param>
        /// <param name="configuration"></param>
        /// <param name="originalFileRef"></param>
        /// <returns></returns>
        public static FileReference
        MakeLinkedClone(
            Project project,
            Bam.Core.EConfiguration configuration,
            FileReference originalFileRef)
        {
            if (project == originalFileRef.Project)
            {
                return null;
            }
            var clone = project.FindOrCreateFileReference(originalFileRef);
            return clone;
        }

        private Bam.Core.TokenizedString ThePath;
        public Bam.Core.TokenizedString Path
        {
            get
            {
                return this.ThePath;
            }
            private set
            {
                this.ThePath = value;
                this.Name = System.IO.Path.GetFileName(value.Parse());
            }
        }

        public EFileType Type
        {
            get;
            private set;
        }

        private Project Project
        {
            get;
            set;
        }

        private bool ExplicitType
        {
            get;
            set;
        }

        private ESourceTree SourceTree
        {
            get;
            set;
        }

        public FileReference LinkedTo
        {
            get;
            private set;
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

        private string FileTypeAsString()
        {
            switch (this.Type)
            {
                case EFileType.SourceCodeC:
                    return "sourcecode.c.c";

                case EFileType.HeaderFile:
                    return "sourcecode.c.h";

                case EFileType.Archive:
                    return "archive.ar";

                case EFileType.Executable:
                    return "compiled.mach-o.executable";

                case EFileType.DynamicLibrary:
                    return "compiled.mach-o.dylib";

                case EFileType.WrapperFramework:
                    return "wrapper.framework";

                case EFileType.ApplicationBundle:
                    return "wrapper.application";
            }

            throw new Bam.Core.Exception("Unrecognized file type {0}", this.Type.ToString());
        }

        private string SourceTreeAsString()
        {
            switch (this.SourceTree)
            {
                case ESourceTree.NA:
                    return "\"<unknown>\"";

                case ESourceTree.Absolute:
                    return "\"<absolute>\"";

                case ESourceTree.Group:
                    return "\"<group>\"";

                case ESourceTree.SourceRoot:
                    return "SOURCE_ROOT";

                case ESourceTree.DeveloperDir:
                    return "DEVELOPER_DIR";

                case ESourceTree.BuiltProductsDir:
                    return "BUILT_PRODUCTS_DIR";

                case ESourceTree.SDKRoot:
                    return "SDKROOT";

                default:
                    throw new Bam.Core.Exception("Unknown source tree");
            }
        }

        public override void Serialize(System.Text.StringBuilder text, int indentLevel)
        {
            var leafname = System.IO.Path.GetFileName(this.Path.ToString());

            var indent = new string('\t', indentLevel);
            text.AppendFormat("{0}{1} /* {2} */ = {{isa = {3}; ", indent, this.GUID, leafname, this.IsA);
            if (this.ExplicitType)
            {
                text.AppendFormat("explicitFileType = {0}; ", this.FileTypeAsString());
            }
            else
            {
                text.AppendFormat("lastKnownFileType = {0}; ", this.FileTypeAsString());
            }

            text.AppendFormat("name = \"{0}\"; ", leafname);

            string path = null;
            switch (this.SourceTree)
            {
                case ESourceTree.NA:
                case ESourceTree.Absolute:
                    path = this.Path.ToString();
                    break;

                case ESourceTree.BuiltProductsDir:
                    {
                        if ((null != this.LinkedTo) &&
                            (this.LinkedTo.Project.GUID != this.Project.GUID) &&
                            (this.LinkedTo.Project.BuiltProductsDir != this.Project.BuiltProductsDir))
                        {
                            // product is in a different BUILT_PRODUCTS_DIR - make a relative path
                            // but must use the definition of CONFIGURATION_BUILD_DIR to denote where it will end up
                            // cannot use the real path
                            var configName = this.Project.Module.BuildEnvironment.Configuration.ToString();
                            var macros = new Bam.Core.MacroList();
                            macros.Add("moduleoutputdir", Bam.Core.TokenizedString.CreateVerbatim(configName));
                            var fully = this.Path.Parse(macros);
                            var configurationBuildDir = this.Project.BuiltProductsDir + "/" + configName + "/";
                            path = Bam.Core.RelativePathUtilities.GetPath(fully, configurationBuildDir);
                        }
                        else
                        {
                            path = System.IO.Path.GetFileName(this.Path.ToString());
                        }
                    }
                    break;

                default:
                    throw new Bam.Core.Exception("Other source trees not handled yet");
            }
            text.AppendFormat("path = \"{0}\"; ", path);

            text.AppendFormat("sourceTree = {0}; ", this.SourceTreeAsString());
            text.AppendFormat("}};");
            text.AppendLine();
        }
    }
}
