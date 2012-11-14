// <copyright file="CPlusPlusCompiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ComposerXE package</summary>
// <author>Mark Final</author>
namespace ComposerXE
{
    // NEW STYLE
#if true
#else
    public sealed class CPlusPlusCompiler : CCompiler
    {
        public CPlusPlusCompiler(Opus.Core.Target target)
            : base(target)
        {
        }
    }
#endif
}
