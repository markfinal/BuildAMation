// <copyright file="CxxCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Mingw package</summary>
// <author>Mark Final</author>
namespace Mingw
{
    public sealed partial class CxxCompilerOptionCollection : MingwCommon.CxxCompilerOptionCollection
    {
        public CxxCompilerOptionCollection()
            : base()
        {
        }

        public CxxCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            Opus.Core.IToolset info = Opus.Core.ToolsetFactory.CreateToolset(typeof(Mingw.Toolset));

            C.ICompilerTool compilerTool = info.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;
            string cppIncludePath = System.IO.Path.Combine(compilerTool.IncludePaths(node.Target)[0], "c++");

            // TODO: these should be simplified by the use of the MingwDetailData
            cppIncludePath = System.IO.Path.Combine(cppIncludePath, "3.4.5");
            (this as C.ICCompilerOptions).SystemIncludePaths.AddAbsoluteDirectory(cppIncludePath, false);
            (this as C.ICCompilerOptions).SystemIncludePaths.AddAbsoluteDirectory(System.IO.Path.Combine(cppIncludePath, "mingw32"), false);
        }
    }
}
