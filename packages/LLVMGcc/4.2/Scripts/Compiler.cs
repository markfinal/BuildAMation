// <copyright file="Compiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>LLVMGcc package</summary>
// <author>Mark Final</author>
namespace LLVMGcc
{
    public sealed class CCompiler :
        GccCommon.CCompiler,
        Bam.Core.IToolSupportsResponseFile
    {
        public
        CCompiler(
            Bam.Core.IToolset toolset) : base(toolset)
        {}

        #region implemented abstract members of GccCommon.CCompiler
        protected override string Filename
        {
            get
            {
                return "llvm-gcc-4.2";
            }
        }
        #endregion

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
