#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion
namespace C
{
    public static class ToolsetProvider
    {
        static string
        GenericGetToolset(
            System.Type toolType,
            string typeName,
            string option)
        {
            if (!Bam.Core.State.HasCategory("C") || !Bam.Core.State.Has("C", "ToolToToolsetName"))
            {
                throw new Bam.Core.Exception("{0} toolset has not been specified. {1}", typeName, option);
            }

            var map = Bam.Core.State.Get("C", "ToolToToolsetName") as System.Collections.Generic.Dictionary<System.Type, string>;
            try
            {
                return map[toolType];
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                throw new Bam.Core.Exception("Tool '{0}' ({1}) has not been registered", typeName, toolType.ToString());
            }
        }

        static string
        GetCCompilerToolset(
            System.Type toolType)
        {
            return GenericGetToolset(toolType, "C compiler", "Use C.CC=<toolset>");
        }

        static string
        GetCxxCompilerToolset(
            System.Type toolType)
        {
            return GenericGetToolset(toolType, "C++ compiler", "Use C.CXX=<toolset>");
        }

        static string
        GetObjCCompilerToolset(
            System.Type toolType)
        {
            return GenericGetToolset(toolType, "ObjectiveC compiler", "Use C.OBJCC=<toolset>");
        }

        static string
        GetObjCxxCompilerToolset(
            System.Type toolType)
        {
            return GenericGetToolset(toolType, "ObjectiveC++ compiler", "Use C.OBJCCXX=<toolset>");
        }

        static string
        GetLinkerToolset(
            System.Type toolType)
        {
            return GenericGetToolset(toolType, "C linker", "Use C.LINK=<toolset>");
        }

        static string
        GetArchiverToolset(
            System.Type toolType)
        {
            return GenericGetToolset(toolType, "C archiver", "Use C.AR=<toolset>");
        }

        static string
        GetWinResourceCompilerToolset(
            System.Type toolType)
        {
            return GenericGetToolset(toolType, "Windows resource compiler", "Use C.RC=<toolset>");
        }

        static string
        GetWinManifestToolToolset(
            System.Type toolType)
        {
            return GenericGetToolset(toolType, "Windows manifest tool", "Use C.MT=<toolset>");
        }

        static string
        GetNullOpToolset(
            System.Type toolType)
        {
            return GenericGetToolset(toolType, "C", "Use C.toolset=<toolset>");
        }

        static string
        GetThirdPartyToolset(
            System.Type toolType)
        {
            return GenericGetToolset(toolType, "C", "Use C.toolset=<toolset>");
        }

        static string
        GetPosixSharedLibrarySymlinksToolset(
            System.Type toolType)
        {
            return GenericGetToolset(toolType, "Posix shared library symlinks tool", "Use C.SOSymLinks=<toolset>");
        }
    }
}
