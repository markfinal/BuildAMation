#region License
// Copyright (c) 2010-2016, Mark Final
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
namespace Gcc
{
    // this implementation is here because the specific version of the Gcc compiler exposes a new interface
    // and because C# cannot derive from a generic type, this C++ option collection must derive from the specific
    // C option collection
    public sealed partial class CxxCompilerOptionCollection :
        CCompilerOptionCollection,
        C.ICxxCompilerOptions
    {
        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);

            // TODO: think I can move this to GccCommon, but it misses out the C++ include paths for some reason (see Test9-dev)
            var target = node.Target;
            var gccToolset = target.Toolset as GccCommon.Toolset;
            var machineType = gccToolset.GccDetail.Target;
            var cxxIncludePath = gccToolset.GccDetail.GxxIncludePath;

            if (!System.IO.Directory.Exists(cxxIncludePath))
            {
                throw new Bam.Core.Exception("Gcc C++ include path '{0}' does not exist. Is g++ installed?", cxxIncludePath);
            }
            var cxxIncludePath2 = System.String.Format("{0}/{1}", cxxIncludePath, machineType);
            if (!System.IO.Directory.Exists(cxxIncludePath2))
            {
                throw new Bam.Core.Exception("Gcc C++ include path '{0}' does not exist. Is g++ installed?", cxxIncludePath2);
            }

            var cCompilerOptions = this as C.ICCompilerOptions;
            cCompilerOptions.SystemIncludePaths.Add(cxxIncludePath);
            cCompilerOptions.SystemIncludePaths.Add(cxxIncludePath2);

            GccCommon.CxxCompilerOptionCollection.ExportedDefaults(this, node);
        }

        public
        CxxCompilerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}
    }
}
