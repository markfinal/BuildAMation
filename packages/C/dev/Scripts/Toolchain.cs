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

        public abstract string ObjectFileExtension
        {
            get;
        }

        public abstract string StaticLibraryExtension
        {
            get;
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

        public static string ObjectFileOutputSubDirectory
        {
            get
            {
                return "obj";
            }
        }
    }
}