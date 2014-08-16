// <copyright file="CxxCompiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Mingw package</summary>
// <author>Mark Final</author>
namespace Mingw
{
    public sealed class CxxCompiler :
        MingwCommon.CxxCompiler
    {
        public
        CxxCompiler(
            Bam.Core.IToolset toolset) : base(toolset)
        {}

        protected override string Filename
        {
            get
            {
                return "mingw32-g++";
            }
        }
    }
}
