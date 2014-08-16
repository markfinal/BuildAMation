// <copyright file="CxxCompiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Gcc package</summary>
// <author>Mark Final</author>
namespace Gcc
{
    public sealed class CxxCompiler :
        GccCommon.CxxCompiler,
        Bam.Core.IToolSupportsResponseFile
    {
        public
        CxxCompiler(
            Bam.Core.IToolset toolset) : base(toolset)
        {}

        #region implemented abstract members of GccCommon.CxxCompiler
        protected override string Filename
        {
            get
            {
                return "g++-4.4";
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
