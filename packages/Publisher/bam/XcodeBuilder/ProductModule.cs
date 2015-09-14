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
    public sealed class XcodePackager :
        IPackagePolicy
    {
        void
        IPackagePolicy.Package(
            Package sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.TokenizedString packageRoot,
            System.Collections.ObjectModel.ReadOnlyDictionary<Bam.Core.Module, System.Collections.Generic.Dictionary<Bam.Core.TokenizedString, PackageReference>> packageObjects)
        {
            // instead of copying to the package root, modules are copied next to their dependees
            foreach (var module in packageObjects)
            {
                foreach (var path in module.Value)
                {
                    var sourcePath = path.Key.ToString();
                    if (path.Value.IsMarker)
                    {
                        // no copy is needed, but as we're copying other files relative to this, record where they have to go
                        // therefore ignore any subdirectory on this module

                        // this has to be the path that Xcode writes to
                        var dir = Bam.Core.TokenizedString.Create("$(pkgbuilddir)/$(config)", module.Key).Parse();
                        path.Value.DestinationDir = dir;

                        if ((path.Value.SubDirectory != null) && path.Value.SubDirectory.Contains(".app/"))
                        {
                            var meta = module.Key.MetaData as XcodeBuilder.XcodeMeta;
                            meta.Target.MakeApplicationBundle();
                        }
                    }
                    else
                    {
                        var subdir = path.Value.SubDirectory;
                        foreach (var reference in path.Value.References)
                        {
                            if (reference.Module.PackageDefinition == module.Key.PackageDefinition)
                            {
                                // same package has the same output folder, so don't bother copying
                                continue;
                            }
                            var commands = new Bam.Core.StringArray();
                            if (null != module.Key.MetaData)
                            {
                                var destinationDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(reference.DestinationDir, subdir));
                                commands.Add(System.String.Format("[[ ! -d {0} ]] && mkdir -p {0}", destinationDir));
                                commands.Add(System.String.Format("cp -v $CONFIGURATION_BUILD_DIR/$EXECUTABLE_NAME {0}/$EXECUTABLE_NAME", destinationDir));
                                (module.Key.MetaData as XcodeBuilder.XcodeCommonProject).AddPostBuildCommands(commands);
                                path.Value.DestinationDir = destinationDir;
                            }
                            else
                            {
                                commands.Add(System.String.Format("cp -v {0} $CONFIGURATION_BUILD_DIR/{1}/{2}", sourcePath, subdir, System.IO.Path.GetFileName(sourcePath)));
                                (reference.Module.MetaData as XcodeBuilder.XcodeCommonProject).AddPostBuildCommands(commands);
                            }
                        }
                    }
                }
            }
        }
    }
}
