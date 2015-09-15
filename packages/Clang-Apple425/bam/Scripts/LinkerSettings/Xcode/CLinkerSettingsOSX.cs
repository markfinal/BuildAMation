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
namespace Clang
{
    public static partial class XcodeImplementation
    {
        public static void
        Convert(
            this C.ILinkerSettingsOSX options,
            Bam.Core.Module module,
            XcodeBuilder.Configuration configuration)
        {
            if (options.Frameworks.Count > 0)
            {
                var meta = module.MetaData as XcodeBuilder.XcodeMeta;
                (meta as XcodeBuilder.XcodeCommonLinkable).EnsureFrameworksBuildPhaseExists();
                var project = meta.Project;
                foreach (var framework in options.Frameworks)
                {
                    var frameworkFileRefPath = framework;
                    var isAbsolute = System.IO.Path.IsPathRooted(frameworkFileRefPath.Parse());

                    if (!isAbsolute)
                    {
                        // assume it's a system framework
                        frameworkFileRefPath = Bam.Core.TokenizedString.Create("/System/Library/Frameworks/" + framework.Parse() + ".framework", null, verbatim:true);
                    }

                    var fileRef = project.FindOrCreateFileReference(
                        frameworkFileRefPath,
                        XcodeBuilder.FileReference.EFileType.WrapperFramework,
                        sourceTree:XcodeBuilder.FileReference.ESourceTree.Absolute);
                    project.MainGroup.AddReference(fileRef);

                    var buildFile = project.FindOrCreateBuildFile(
                        frameworkFileRefPath,
                        fileRef);

                    meta.Target.FrameworksBuildPhase.AddBuildFile(buildFile);
                }
            }
            if (options.FrameworkSearchDirectories.Count > 0)
            {
                var option = new XcodeBuilder.MultiConfigurationValue();
                foreach (var path in options.FrameworkSearchDirectories)
                {
                    option.Add(path.Parse());
                }
                configuration["FRAMEWORK_SEARCH_PATHS"] = option;
            }
        }
    }
}
