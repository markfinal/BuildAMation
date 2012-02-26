// <copyright file="Toolchain.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ComposerXECommon package</summary>
// <author>Mark Final</author>
namespace ComposerXECommon
{
    public abstract class Toolchain : C.Toolchain
    {
        public override string PreprocessedOutputSuffix
        {
            get
            {
                return ".i";
            }
        }

        public override string ObjectFileSuffix
        {
            get
            {
                return ".o";
            }
        }

        public override string StaticLibraryPrefix
        {
            get
            {
                return "lib";
            }
        }

        public override string StaticLibrarySuffix
        {
            get
            {
                return ".a";
            }
        }

        public override string StaticImportLibraryPrefix
        {
            get
            {
                return this.StaticLibraryPrefix;
            }
        }

        public override string StaticImportLibrarySuffix
        {
            get
            {
                if (Opus.Core.OSUtilities.IsUnixHosting)
                {
                    return ".so";
                }
                else if (Opus.Core.OSUtilities.IsOSXHosting)
                {
                    return ".dylib";
                }
                else
                {
                    return null;
                }
            }
        }

        public override string DynamicLibraryPrefix
        {
            get
            {
                return this.StaticLibraryPrefix;
            }
        }

        public override string DynamicLibrarySuffix
        {
            get
            {
                return this.StaticImportLibrarySuffix;
            }
        }

        public override string ExecutableSuffix
        {
            get
            {
                return string.Empty;
            }
        }

        public override string MapFileSuffix
        {
            get
            {
                return ".map";
            }
        }

        // this is not used
        public override Opus.Core.StringArray Environment
        {
            get;
            protected set;
        }
    }
}