// <copyright file="CxxCompiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Mingw package</summary>
// <author>Mark Final</author>
namespace Mingw
{
    // NEW STYLE
#if true
    public sealed class CxxCompiler : MingwCommon.CxxCompiler
    {
        public CxxCompiler(Opus.Core.IToolset toolset)
            : base(toolset)
        {
        }

        protected override string Filename
        {
            get
            {
                return "mingw32-g++";
            }
        }
    }
#else
    public sealed class CPlusPlusCompiler : C.CxxCompiler, Opus.Core.ITool, Opus.Core.IToolRequiredEnvironmentVariables, Opus.Core.IToolEnvironmentPaths, C.ICompiler
    {
        private Opus.Core.StringArray requiredEnvironmentVariables = new Opus.Core.StringArray();
        private Opus.Core.StringArray includeFolders = new Opus.Core.StringArray();
        private string binPath;

        public CPlusPlusCompiler(Opus.Core.Target target)
            : base(target)
        {
        }
    }
#endif
}
