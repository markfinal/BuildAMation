// <copyright file="Linker.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>LLVMGcc package</summary>
// <author>Mark Final</author>
namespace LLVMGcc
{
    public sealed class Linker :
        GccCommon.Linker,
        Bam.Core.IToolSupportsResponseFile
    {
        public
        Linker(
            Bam.Core.IToolset toolset) : base(toolset)
        {}

        protected override string Filename
        {
            get
            {
                return "llvm-gcc-4.2";
            }
        }

        #region IToolSupportsResponseFile Members

        string Bam.Core.IToolSupportsResponseFile.Option
        {
            get
            {
                return "@";
            }
        }

        #endregion
    }
}
