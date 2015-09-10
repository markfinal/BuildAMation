#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
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
