// <copyright file="ToolsetProvider.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public static class ToolsetProvider
    {
        static string GetCCompilerToolset(System.Type toolType)
        {
            System.Collections.Generic.Dictionary<System.Type, string> map = Opus.Core.State.Get("Toolchains", "Map") as System.Collections.Generic.Dictionary<System.Type, string>;
            return map[typeof(ICompilerTool)];
        }

        static string GetCxxCompilerToolset(System.Type toolType)
        {
            System.Collections.Generic.Dictionary<System.Type, string> map = Opus.Core.State.Get("Toolchains", "Map") as System.Collections.Generic.Dictionary<System.Type, string>;
            return map[typeof(ICxxCompilerTool)];
        }

        static string GetLinkerToolset(System.Type toolType)
        {
            System.Collections.Generic.Dictionary<System.Type, string> map = Opus.Core.State.Get("Toolchains", "Map") as System.Collections.Generic.Dictionary<System.Type, string>;
            return map[typeof(ILinkerTool)];
        }
    }
}
