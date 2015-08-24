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
namespace V2
{
    public sealed class XcodeHeaderLibrary :
        IHeaderLibraryPolicy
    {
        void
        IHeaderLibraryPolicy.HeadersOnly(
            HeaderLibrary sender,
            Bam.Core.V2.ExecutionContext context,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> headers)
        {
            var library = new XcodeBuilder.V2.XcodeHeaderLibrary(sender);
            foreach (var header in headers)
            {
                if (header is Bam.Core.V2.IModuleGroup)
                {
                    foreach (var child in header.Children)
                    {
                        var headerMod = child as HeaderFile;
                        var headerFileRef = library.Project.FindOrCreateFileReference(
                            headerMod.InputPath,
                            XcodeBuilder.V2.FileReference.EFileType.HeaderFile,
                            sourceTree:XcodeBuilder.V2.FileReference.ESourceTree.Absolute);
                        library.AddHeader(headerFileRef);
                    }
                }
                else
                {
                    var headerMod = header as HeaderFile;
                    var headerFileRef = library.Project.FindOrCreateFileReference(
                        headerMod.InputPath,
                        XcodeBuilder.V2.FileReference.EFileType.HeaderFile,
                        sourceTree:XcodeBuilder.V2.FileReference.ESourceTree.Absolute);
                    library.AddHeader(headerFileRef);
                }
            }
        }
    }
}
}
namespace XcodeBuilder
{
    public sealed partial class XcodeBuilder
    {
        [Bam.Core.EmptyBuildFunction]
        public object
        Build(
            C.HeaderLibrary moduleToBuild,
            out bool success)
        {
            success = true;
            return null;
        }
    }
}
