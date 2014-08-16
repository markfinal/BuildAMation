// <copyright file="Linker.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Mingw package</summary>
// <author>Mark Final</author>
namespace Mingw
{
    public sealed class Linker :
        MingwCommon.Linker
    {
        public
        Linker(
            Bam.Core.IToolset toolset) : base(toolset)
        {}

        protected override string Filename
        {
            get
            {
                return "mingw32-gcc-4.8.1";
            }
        }
    }
}
