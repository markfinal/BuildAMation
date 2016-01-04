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
namespace ClangCommon
{
    public static partial class XcodeLinkerImplementation
    {
        public static void
        Convert(
            this C.ICommonLinkerSettingsOSX settings,
            Bam.Core.Module module,
            XcodeBuilder.Configuration configuration)
        {
            if (settings.Frameworks.Count > 0)
            {
                var target = module.MetaData as XcodeBuilder.Target;
                var project = target.Project;
                foreach (var framework in settings.Frameworks)
                {
                    var frameworkFileRefPath = framework;
                    var isAbsolute = System.IO.Path.IsPathRooted(frameworkFileRefPath.Parse());

                    if (!isAbsolute)
                    {
                        // TODO: change to a positional token
                        // assume it's a system framework
                        frameworkFileRefPath = Bam.Core.TokenizedString.Create("/System/Library/Frameworks/$(0).framework", null, new Bam.Core.TokenizedStringArray(framework));
                    }

                    var buildFile = target.EnsureFrameworksBuildFileExists(
                        frameworkFileRefPath,
                        XcodeBuilder.FileReference.EFileType.WrapperFramework);
                    project.MainGroup.AddChild(buildFile.FileRef);
                }
            }
            if (settings.FrameworkSearchPaths.Count > 0)
            {
                var option = new XcodeBuilder.MultiConfigurationValue();
                foreach (var path in settings.FrameworkSearchPaths)
                {
                    option.Add(path.Parse());
                }
                configuration["FRAMEWORK_SEARCH_PATHS"] = option;
            }
            if (null != settings.InstallName)
            {
                if (module is C.IDynamicLibrary)
                {
                    configuration["LD_DYLIB_INSTALL_NAME"] = new XcodeBuilder.UniqueConfigurationValue(settings.InstallName.Parse());
                }
            }
            // settings.MinimumVersionSupported is dealt with in XcodeBuilder as there is not a difference
            // between compiler and linker setting in the project
        }
    }
}
