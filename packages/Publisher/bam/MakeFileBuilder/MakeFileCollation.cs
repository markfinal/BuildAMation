#region License
// Copyright (c) 2010-2017, Mark Final
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
namespace Publisher
{
#if D_NEW_PUBLISHING
    public sealed class MakeFileCollatedObject2 :
        ICollatedObjectPolicy2
    {
        void
        ICollatedObjectPolicy2.Collate(
            CollatedObject2 sender,
            Bam.Core.ExecutionContext context)
        {
            var collatedInterface = sender as ICollatedObject2;
            var copySourcePath = collatedInterface.SourceModule.GeneratedPaths[collatedInterface.SourcePathKey];

            // post-fix with a directory separator to enforce that this is a directory destination
            var destinationDir = System.String.Format("{0}{1}",
                collatedInterface.PublishingDirectory.ToString(),
                System.IO.Path.DirectorySeparatorChar);

            Bam.Core.Log.MessageAll("** Module {0} with key {1} goes to '{2}' [{3}]",
                collatedInterface.SourceModule.ToString(),
                collatedInterface.SourcePathKey.ToString(),
                collatedInterface.PublishingDirectory.ToString(),
                sender);
        }
    }
#endif

    public sealed class MakeFileCollation :
        ICollationPolicy
    {
        void
        ICollationPolicy.Collate(
            Collation sender,
            Bam.Core.ExecutionContext context)
        {
            var meta = new MakeFileBuilder.MakeFileMeta(sender);
            var rule = meta.AddRule();
            rule.AddTarget(Bam.Core.TokenizedString.CreateVerbatim("publish"), isPhony: true);

            foreach (var required in sender.Requirements)
            {
                var requiredMeta = required.MetaData as MakeFileBuilder.MakeFileMeta;
                if (null == requiredMeta)
                {
                    continue;
                }
                foreach (var rules in requiredMeta.Rules)
                {
                    // TODO: only the first?
                    rule.AddPrerequisite(rules.Targets[0]);
                }
            }
        }
    }
}
