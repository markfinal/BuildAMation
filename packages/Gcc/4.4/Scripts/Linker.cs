// <copyright file="Linker.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Gcc package</summary>
// <author>Mark Final</author>
namespace Gcc
{
    public sealed class Linker : GccCommon.Linker, Opus.Core.IToolSupportsResponseFile
    {
        public Linker(Opus.Core.IToolset toolset)
            : base(toolset)
        {
        }

        protected override string Filename
        {
            get
            {
                return "gcc-4.4";
            }
        }

        #region IToolSupportsResponseFile Members

        string Opus.Core.IToolSupportsResponseFile.Option
        {
            get
            {
                return "@";
            }
        }

        #endregion
    }
}