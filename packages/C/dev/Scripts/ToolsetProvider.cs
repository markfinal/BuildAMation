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
            if (!Opus.Core.State.HasCategory("Toolchains") || !Opus.Core.State.Has("Toolchains", "Map"))
            {
                throw new Opus.Core.Exception("C compiler toolset has not been specified. Use C.CC=<toolset>", false);
            }

            System.Collections.Generic.Dictionary<System.Type, string> map = Opus.Core.State.Get("Toolchains", "Map") as System.Collections.Generic.Dictionary<System.Type, string>;
            return map[typeof(ICompilerTool)];
        }

        static string GetCxxCompilerToolset(System.Type toolType)
        {
            if (!Opus.Core.State.HasCategory("Toolchains") || !Opus.Core.State.Has("Toolchains", "Map"))
            {
                throw new Opus.Core.Exception("C++ compiler toolset has not been specified. Use C.CXX=<toolset>", false);
            }

            System.Collections.Generic.Dictionary<System.Type, string> map = Opus.Core.State.Get("Toolchains", "Map") as System.Collections.Generic.Dictionary<System.Type, string>;
            return map[typeof(ICxxCompilerTool)];
        }

        static string GetLinkerToolset(System.Type toolType)
        {
            if (!Opus.Core.State.HasCategory("Toolchains") || !Opus.Core.State.Has("Toolchains", "Map"))
            {
                throw new Opus.Core.Exception("C linker toolset has not been specified. Use C.LINK=<toolset>", false);
            }

            System.Collections.Generic.Dictionary<System.Type, string> map = Opus.Core.State.Get("Toolchains", "Map") as System.Collections.Generic.Dictionary<System.Type, string>;
            return map[typeof(ILinkerTool)];
        }

        static string GetArchiverToolset(System.Type toolType)
        {
            if (!Opus.Core.State.HasCategory("Toolchains") || !Opus.Core.State.Has("Toolchains", "Map"))
            {
                throw new Opus.Core.Exception("C archiver toolset has not been specified. Use C.AR=<toolset>", false);
            }

            System.Collections.Generic.Dictionary<System.Type, string> map = Opus.Core.State.Get("Toolchains", "Map") as System.Collections.Generic.Dictionary<System.Type, string>;
            return map[typeof(IArchiverTool)];
        }

        static string GetWinResourceompilerToolset(System.Type toolType)
        {
            if (!Opus.Core.State.HasCategory("Toolchains") || !Opus.Core.State.Has("Toolchains", "Map"))
            {
                throw new Opus.Core.Exception("C windows resource compiler toolset has not been specified. Use C.RC=<toolset>", false);
            }

            System.Collections.Generic.Dictionary<System.Type, string> map = Opus.Core.State.Get("Toolchains", "Map") as System.Collections.Generic.Dictionary<System.Type, string>;
            return map[typeof(IWinResourceCompilerTool)];
        }
    }
}
