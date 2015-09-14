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
    public sealed class VSSolutionPackager :
        IPackagePolicy
    {
        void
        IPackagePolicy.Package(
            Package sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.TokenizedString packageRoot,
            System.Collections.ObjectModel.ReadOnlyDictionary<Bam.Core.V2.Module, System.Collections.Generic.Dictionary<Bam.Core.V2.TokenizedString, PackageReference>> packageObjects)
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
                        path.Value.DestinationDir = System.IO.Path.GetDirectoryName(sourcePath);
                    }
                    else
                    {
                        var subdir = path.Value.SubDirectory;
                        foreach (var reference in path.Value.References)
                        {
                            var commands = new Bam.Core.StringArray();
                            if (null != module.Key.MetaData)
                            {
                                var destinationDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(reference.DestinationDir, subdir));
                                commands.Add(System.String.Format("IF NOT EXIST {0} MKDIR {0}", destinationDir));
                                commands.Add(System.String.Format(@"copy /V /Y $(OutputPath)$(TargetFileName) {0}\$(TargetFileName)", destinationDir));
#if true
                                var project = module.Key.MetaData as VSSolutionBuilder.V2.VSProject;
                                var config = project.GetConfiguration(module.Key);
                                config.AddPostBuildCommands(commands);
#else
                                (module.Key.MetaData as VSSolutionBuilder.V2.VSCommonProject).AddPostBuildCommands(commands);
#endif
                                path.Value.DestinationDir = destinationDir;
                            }
                            else
                            {
                                commands.Add(System.String.Format(@"copy /V /Y {0} $(OutDir)\{1}\{2}", sourcePath, subdir, System.IO.Path.GetFileName(sourcePath)));
#if true
                                var project = reference.Module.MetaData as VSSolutionBuilder.V2.VSProject;
                                var config = project.GetConfiguration(reference.Module);
                                config.AddPostBuildCommands(commands);
#else
                                (reference.Module.MetaData as VSSolutionBuilder.V2.VSCommonProject).AddPostBuildCommands(commands);
#endif
                            }
                        }
                    }
                }
            }
        }
    }
}
}
