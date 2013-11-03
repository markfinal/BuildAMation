// <copyright file="Compiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Mingw package</summary>
// <author>Mark Final</author>
namespace Mingw
{
    public sealed class CCompiler : MingwCommon.CCompiler
    {
        public CCompiler(Opus.Core.IToolset toolset)
            : base(toolset)
        {
        }

        protected override string Filename
        {
            get
            {
                return "mingw32-gcc-4.8.1";
            }
        }
    }
}