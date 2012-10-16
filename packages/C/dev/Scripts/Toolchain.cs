// <copyright file="Toolchain.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ExportToolchainOptionsDelegateAttribute : System.Attribute
    {
        // TODO
    }

    public abstract class Toolchain
    {
        public abstract string InstallPath(Opus.Core.Target target);
        public abstract string BinPath(Opus.Core.Target target);
        public abstract Opus.Core.StringArray Environment
        {
            get;
            protected set;
        }

#if false
        public abstract string PreprocessedOutputSuffix
        {
            get;
        }
        
        public abstract string ObjectFileSuffix
        {
            get;
        }
#endif

        public abstract string StaticLibraryPrefix
        {
            get;
        }

        public abstract string StaticLibrarySuffix
        {
            get;
        }

        public abstract string StaticImportLibraryPrefix
        {
            get;
        }

        public abstract string StaticImportLibrarySuffix
        {
            get;
        }

        public abstract string DynamicLibraryPrefix
        {
            get;
        }

        public abstract string DynamicLibrarySuffix
        {
            get;
        }

        public abstract string ExecutableSuffix
        {
            get;
        }

        public abstract string MapFileSuffix
        {
            get;
        }

        public virtual string Win32CompiledResourceSuffix
        {
            get
            {
                return ".resourcefileextension";
            }
        }

        public static string BinaryOutputSubDirectory
        {
            get
            {
                return "bin";
            }
        }

        public static string LibraryOutputSubDirectory
        {
            get
            {
                return "lib";
            }
        }

#if false
        public static string ObjectFileOutputSubDirectory
        {
            get
            {
                return "obj";
            }
        }
#endif
    }
}