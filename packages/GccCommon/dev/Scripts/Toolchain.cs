// <copyright file="Toolchain.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
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
                if (Opus.Core.OSUtilities.IsUnixHosting)
                {
                    return ".so";
                }
                else if (Opus.Core.OSUtilities.IsUnixHosting)
                {
                    return ".dylib";
                }
                else
                {
                    return null;
                }
            }
        }
    }
}