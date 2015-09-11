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
namespace GccCommon
{
    public abstract class Linker :
        C.ILinkerTool,
        Bam.Core.IToolEnvironmentVariables
    {
        protected Bam.Core.IToolset toolset;
        private Bam.Core.StringArray environment;

        protected
        Linker(
            Bam.Core.IToolset toolset)
        {
            this.toolset = toolset;
            this.environment = new Bam.Core.StringArray("/usr/bin");
        }

        protected abstract string Filename
        {
            get;
        }

        #region ILinkerTool Members

        string C.ILinkerTool.ExecutableSuffix
        {
            get
            {
                return string.Empty;
            }
        }

        string C.ILinkerTool.MapFileSuffix
        {
            get
            {
                return ".map";
            }
        }

        string C.ILinkerTool.StartLibraryList
        {
            get
            {
                if (Bam.Core.OSUtilities.IsLinuxHosting)
                {
                    return "-Wl,--start-group";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        string C.ILinkerTool.EndLibraryList
        {
            get
            {
                if (Bam.Core.OSUtilities.IsLinuxHosting)
                {
                    return "-Wl,--end-group";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        string C.ILinkerTool.DynamicLibraryPrefix
        {
            get
            {
                return "lib";
            }
        }

        string C.ILinkerTool.DynamicLibrarySuffix
        {
            get
            {
                if (Bam.Core.OSUtilities.IsLinuxHosting)
                {
                    return ".so";
                }
                else if (Bam.Core.OSUtilities.IsOSXHosting)
                {
                    return ".dylib";
                }
                else
                {
                    return null;
                }
            }
        }

        string C.ILinkerTool.BinaryOutputSubDirectory
        {
            get
            {
                return "bin";
            }
        }

        Bam.Core.StringArray
        C.ILinkerTool.LibPaths(
            Bam.Core.BaseTarget baseTarget)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region ITool Members

        string
        Bam.Core.ITool.Executable(
            Bam.Core.BaseTarget baseTarget)
        {
            var installPath = this.toolset.BinPath(baseTarget);
            var executablePath = System.IO.Path.Combine(installPath, this.Filename);
            return executablePath;
        }

        Bam.Core.Array<Bam.Core.LocationKey>
        Bam.Core.ITool.OutputLocationKeys(
            Bam.Core.BaseModule module)
        {
            var array = new Bam.Core.Array<Bam.Core.LocationKey>(
                C.Application.OutputFile,
                C.Application.OutputDir,
                C.Application.MapFile,
                C.Application.MapFileDir);
            if (module is C.DynamicLibrary)
            {
                array.AddRange(new [] {
                    C.DynamicLibrary.ImportLibraryFile,
                    C.DynamicLibrary.ImportLibraryDir});
            }
            return array;
        }

        #endregion

        #region IToolEnvironmentVariables implementation
        System.Collections.Generic.Dictionary<string, Bam.Core.StringArray>
        Bam.Core.IToolEnvironmentVariables.Variables(
            Bam.Core.BaseTarget baseTarget)
        {
            var dictionary = new System.Collections.Generic.Dictionary<string, Bam.Core.StringArray>();
            dictionary["PATH"] = this.environment;
            return dictionary;
        }
        #endregion
    }
}
