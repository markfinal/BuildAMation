// <copyright file="Toolchain.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MingwCommon package</summary>
// <author>Mark Final</author>
namespace MingwCommon
{
    public abstract class Toolchain : C.Toolchain
    {
        public override string ObjectFileExtension
        {
            get
            {
                return ".o";
            }
        }

        public override string StaticLibraryExtension
        {
            get
            {
                return ".a";
            }
        }

        public override string StaticImportLibraryExtension
        {
            get
            {
                return this.StaticLibraryExtension;
            }
        }
    }
}