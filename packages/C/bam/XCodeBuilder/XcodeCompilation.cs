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
namespace C
{
    public sealed class XcodeCompilation :
        ICompilationPolicy
    {
        void
        ICompilationPolicy.Compile(
            ObjectFile sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.TokenizedString objectFilePath,
            Bam.Core.Module source)
        {
            var encapsulating = sender.GetEncapsulatingReferencedModule();

            var workspace = Bam.Core.Graph.Instance.MetaData as XcodeBuilder.WorkspaceMeta;
            var target = workspace.EnsureTargetExists(encapsulating);

            XcodeBuilder.FileReference.EFileType fileType;
            if (sender is C.ObjectFile)
            {
                fileType = XcodeBuilder.FileReference.EFileType.SourceCodeC;
            }
            else if (sender is C.Cxx.ObjectFile)
            {
                fileType = XcodeBuilder.FileReference.EFileType.SourceCodeCxx;
            }
            else if (sender is C.ObjC.ObjectFile)
            {
                fileType = XcodeBuilder.FileReference.EFileType.SourceCodeObjC;
            }
            else if (sender is C.ObjCxx.ObjectFile)
            {
                fileType = XcodeBuilder.FileReference.EFileType.SourceCodeObjCxx;
            }
            else
            {
                throw new Bam.Core.Exception("Unknown object file type, {0}", sender.GetType().ToString());
            }

            sender.MetaData = target.EnsureSourceBuildFileExists(source.GeneratedPaths[C.SourceFile.Key], fileType);

            // this is for stand-alone object files
            if (encapsulating == sender || encapsulating == (sender as Bam.Core.IChildModule).Parent)
            {
                target.Type = XcodeBuilder.Target.EProductType.ObjFile;
                var configuration = target.GetConfiguration(sender);
                configuration.SetProductName(Bam.Core.TokenizedString.CreateVerbatim("${TARGET_NAME}"));
                (sender.Settings as XcodeProjectProcessor.IConvertToProject).Convert(sender, configuration);
            }

            // any non-C module targets should be order-only dependencies
            foreach (var dependent in sender.Dependents)
            {
                if (null == dependent.MetaData)
                {
                    continue;
                }
                if (dependent is C.CModule)
                {
                    continue;
                }
                var dependentTarget = dependent.MetaData as XcodeBuilder.Target;
                if (null != dependentTarget)
                {
                    target.Requires(dependentTarget);
                }
            }
        }
    }
}
