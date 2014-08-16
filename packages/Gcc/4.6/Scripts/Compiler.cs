// <copyright file="Compiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Gcc package</summary>
// <author>Mark Final</author>
namespace Gcc
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
                return "gcc-4.6";
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
