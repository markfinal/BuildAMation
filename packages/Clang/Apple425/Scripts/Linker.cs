// <copyright file="Linker.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Clang package</summary>
// <author>Mark Final</author>
namespace Clang
{
    public sealed class Linker : GccCommon.Linker
    {
        public Linker(Opus.Core.IToolset toolset)
            : base(toolset)
        {}

        #region implemented abstract members of Linker
        protected override string Filename
        {
            get
            {
                return "clang";
            }
        }
        #endregion
    }
}
