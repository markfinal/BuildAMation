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
namespace Publisher
{
namespace V2
{
    public sealed class MakeFilePackager :
        IPackagePolicy
    {
        private static void
        CopyFileRule(
            MakeFileBuilder.V2.MakeFileMeta meta,
            MakeFileBuilder.V2.MakeFileMeta sourceMeta,
            MakeFileBuilder.V2.Rule parentRule,
            string outputDirectory,
            Bam.Core.TokenizedString sourcePath)
        {
            var copyRule = meta.AddRule();
            var target = copyRule.AddTarget(Bam.Core.TokenizedString.Create(outputDirectory + "/" + System.IO.Path.GetFileName(sourcePath.Parse()), null));

            // TODO: there needs to be a mapping from this path to any existing targets so that the target variable names can be used
            copyRule.AddPrerequisite(sourcePath);

            var command = new System.Text.StringBuilder();
            command.AppendFormat("cp -fv $< $@");
            copyRule.AddShellCommand(command.ToString());

            parentRule.AddPrerequisite(target);

            meta.CommonMetaData.Directories.AddUnique(outputDirectory);
        }

        void
        IPackagePolicy.Package(
            Package sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.TokenizedString packageRoot,
            System.Collections.ObjectModel.ReadOnlyDictionary<Bam.Core.Module,
            System.Collections.Generic.Dictionary<Bam.Core.TokenizedString,
            PackageReference>> packageObjects)
        {
            var meta = new MakeFileBuilder.V2.MakeFileMeta(sender);
            var rule = meta.AddRule();
            rule.AddTarget(Bam.Core.TokenizedString.Create("publish", null, verbatim:true), isPhony:true);

            foreach (var module in packageObjects)
            {
                var moduleMeta = module.Key.MetaData as MakeFileBuilder.V2.MakeFileMeta;
                foreach (var path in module.Value)
                {
                    if (path.Value.IsMarker)
                    {
                        var outputDir = packageRoot.Parse();
                        if (null != path.Value.SubDirectory)
                        {
                            outputDir = System.IO.Path.Combine(outputDir, path.Value.SubDirectory);
                        }

                        CopyFileRule(meta, moduleMeta, rule, outputDir, path.Key);
                        path.Value.DestinationDir = outputDir;
                    }
                    else
                    {
                        var subdir = path.Value.SubDirectory;
                        foreach (var reference in path.Value.References)
                        {
                            var destinationDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(reference.DestinationDir, path.Value.SubDirectory));
                            CopyFileRule(meta, moduleMeta, rule, destinationDir, path.Key);
                            path.Value.DestinationDir = destinationDir;
                        }
                    }
                }
            }
        }
    }
}
}
