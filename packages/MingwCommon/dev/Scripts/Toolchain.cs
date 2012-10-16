// <copyright file="Toolchain.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MingwCommon package</summary>
// <author>Mark Final</author>
namespace MingwCommon
{
    public abstract class Toolchain : C.Toolchain
    {
#if false
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
                return this.StaticLibrarySuffix;
            }
        }

        public override string DynamicLibraryPrefix
        {
            get
            {
                return string.Empty;
            }
        }

        public override string DynamicLibrarySuffix
        {
            get
            {
                return ".dll";
            }
        }
#endif

#if false
        public override string ExecutableSuffix
        {
            get
            {
                return ".exe";
            }
        }

        public override string MapFileSuffix
        {
            get
            {
                return ".map";
            }
        }
#endif

        public override string Win32CompiledResourceSuffix
        {
            get
            {
                return ".o";
            }
        }
    }
}