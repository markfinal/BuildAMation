// <copyright file="Linker.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Gcc package</summary>
// <author>Mark Final</author>
namespace Gcc
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
                return "gcc-4.5";
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
