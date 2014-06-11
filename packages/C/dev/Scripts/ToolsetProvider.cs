// <copyright file="ToolsetProvider.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public static class ToolsetProvider
    {
        static string GenericGetToolset(System.Type toolType, string typeName, string option)
        {
            if (!Opus.Core.State.HasCategory("C") || !Opus.Core.State.Has("C", "ToolToToolsetName"))
            {
                throw new Opus.Core.Exception("{0} toolset has not been specified. {1}", typeName, option);
            }

            var map = Opus.Core.State.Get("C", "ToolToToolsetName") as System.Collections.Generic.Dictionary<System.Type, string>;
            try
            {
                return map[toolType];
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                throw new Opus.Core.Exception("Tool '{0}' ({1}) has not been registered", typeName, toolType.ToString());
            }
        }

        static string GetCCompilerToolset(System.Type toolType)
        {
            return GenericGetToolset(toolType, "C compiler", "Use C.CC=<toolset>");
        }

        static string GetCxxCompilerToolset(System.Type toolType)
        {
            return GenericGetToolset(toolType, "C++ compiler", "Use C.CXX=<toolset>");
        }

        static string GetObjCCompilerToolset(System.Type toolType)
        {
            return GenericGetToolset(toolType, "ObjectiveC compiler", "Use C.OBJCC=<toolset>");
        }

        static string GetObjCxxCompilerToolset(System.Type toolType)
        {
            return GenericGetToolset(toolType, "ObjectiveC++ compiler", "Use C.OBJCCXX=<toolset>");
        }

        static string GetLinkerToolset(System.Type toolType)
        {
            return GenericGetToolset(toolType, "C linker", "Use C.LINK=<toolset>");
        }

        static string GetArchiverToolset(System.Type toolType)
        {
            return GenericGetToolset(toolType, "C archiver", "Use C.AR=<toolset>");
        }

        static string GetWinResourceCompilerToolset(System.Type toolType)
        {
            return GenericGetToolset(toolType, "Windows resource compiler", "Use C.RC=<toolset>");
        }

        static string GetWinManifestToolToolset(System.Type toolType)
        {
            return GenericGetToolset(toolType, "Windows manifest tool", "Use C.MT=<toolset>");
        }

        static string GetNullOpToolset(System.Type toolType)
        {
            return GenericGetToolset(toolType, "C", "Use C.toolset=<toolset>");
        }

        static string GetThirdPartyToolset(System.Type toolType)
        {
            return GenericGetToolset(toolType, "C", "Use C.toolset=<toolset>");
        }
    }
}
