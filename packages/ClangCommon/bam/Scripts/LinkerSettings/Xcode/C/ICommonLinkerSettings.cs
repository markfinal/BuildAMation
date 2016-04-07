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
        private static XcodeBuilder.BuildFile
        CreateLinkableReferences(
            XcodeBuilder.Target target,
            string pathToLibrary,
            XcodeBuilder.FileReference.EFileType type,
            XcodeBuilder.FileReference.ESourceTree sourcetree)
        {
            var fileRef = target.Project.EnsureFileReferenceExists(
                Bam.Core.TokenizedString.CreateVerbatim(pathToLibrary),
                XcodeBuilder.FileReference.EFileType.Archive,
                true,
                sourcetree);
            var buildFile = target.EnsureFrameworksBuildFileExists(
                fileRef.Path,
                XcodeBuilder.FileReference.EFileType.Archive);
            target.Project.MainGroup.AddChild(fileRef);
            return buildFile;
        }

        private static XcodeBuilder.BuildFile
        FindLibraryInLibrarySearchPaths(
            XcodeBuilder.Target target,
            C.ICommonLinkerSettings settings,
            string libname)
        {
            foreach (var searchPath in settings.LibraryPaths)
            {
                var realSearchpath = searchPath.Parse();
                // some lib paths might not exist yet
                if (!System.IO.Directory.Exists(realSearchpath))
                {
                    continue;
                }
                // look for dylibs first
                var pattern = System.String.Format("lib{0}.dylib", libname);
                Bam.Core.Log.DebugMessage("Searching {0} for {1}", realSearchpath, pattern);
                var results = System.IO.Directory.GetFiles(realSearchpath, pattern);
                if (results.Length > 0)
                {
                    if (results.Length > 1)
                    {
                        throw new Bam.Core.Exception("Found {0} instances of {1} dynamic libraries in {2}. Which was intended?", results.Length, libname, realSearchpath);
                    }
                    return CreateLinkableReferences(
                        target,
                        System.String.Format("{0}/lib{1}.dylib", realSearchpath, libname),
                        XcodeBuilder.FileReference.EFileType.DynamicLibrary,
                        XcodeBuilder.FileReference.ESourceTree.Absolute);
                }
                else
                {
                    // then static libs
                    pattern = System.String.Format("lib{0}.a", libname);
                    Bam.Core.Log.DebugMessage("Searching {0} for {1}", realSearchpath, pattern);
                    results = System.IO.Directory.GetFiles(realSearchpath, pattern);
                    if (results.Length > 0)
                    {
                        if (results.Length > 1)
                        {
                            throw new Bam.Core.Exception("Found {0} instances of {1} static libraries in {2}. Which was intended?", results.Length, libname, realSearchpath);
                        }
                        return CreateLinkableReferences(
                            target,
                            System.String.Format("{0}/lib{1}.a", realSearchpath, libname),
                            XcodeBuilder.FileReference.EFileType.Archive,
                            XcodeBuilder.FileReference.ESourceTree.Absolute);
                    }
                }
            }
            return null;
        }

        private static XcodeBuilder.BuildFile
        FindLibraryInSDKSearchPaths(
            XcodeBuilder.Target target,
            C.ICommonLinkerSettings settings,
            string libname)
        {
            var meta = Bam.Core.Graph.Instance.PackageMetaData<Clang.MetaData>("Clang");
            var searchPath = System.String.Format("{0}/usr/lib", meta.SDKPath);
            var pattern = System.String.Format("lib{0}.dylib", libname);
            Bam.Core.Log.DebugMessage("Searching {0} for {1}", searchPath, pattern);
            var results = System.IO.Directory.GetFiles(searchPath, pattern);
            if (results.Length > 0)
            {
                if (results.Length > 1)
                {
                    throw new Bam.Core.Exception("Found {0} instances of {1} dynamic libraries in {2}. Which was intended?", results.Length, libname, searchPath);
                }
                return CreateLinkableReferences(
                    target,
                    System.String.Format("usr/lib/lib{0}.dylib", libname),
                    XcodeBuilder.FileReference.EFileType.DynamicLibrary,
                    XcodeBuilder.FileReference.ESourceTree.SDKRoot);
            }
            else
            {
                pattern = System.String.Format("lib{0}.a", libname);
                Bam.Core.Log.DebugMessage("Searching {0} for {1}", searchPath, pattern);
                results = System.IO.Directory.GetFiles(searchPath, pattern);
                if (results.Length > 0)
                {
                    if (results.Length > 1)
                    {
                        throw new Bam.Core.Exception("Found {0} instances of {1} static libraries in {2}. Which was intended?", results.Length, libname, searchPath);
                    }
                    return CreateLinkableReferences(
                        target,
                        System.String.Format("usr/lib/lib{0}.a", libname),
                        XcodeBuilder.FileReference.EFileType.Archive,
                        XcodeBuilder.FileReference.ESourceTree.SDKRoot);
                }
            }
            return null;
        }

        public static void
        Convert(
            this C.ICommonLinkerSettings settings,
            Bam.Core.Module module,
            XcodeBuilder.Configuration configuration)
        {
            switch (settings.Bits)
            {
            case C.EBit.ThirtyTwo:
                {
                    configuration["VALID_ARCHS"] = new XcodeBuilder.UniqueConfigurationValue("i386");
                    configuration["ARCHS"] = new XcodeBuilder.UniqueConfigurationValue("$(ARCHS_STANDARD_32_BIT)");
                }
                break;

            case C.EBit.SixtyFour:
                {
                    configuration["VALID_ARCHS"] = new XcodeBuilder.UniqueConfigurationValue("x86_64");
                    configuration["ARCHS"] = new XcodeBuilder.UniqueConfigurationValue("$(ARCHS_STANDARD_64_BIT)");
                }
                break;

            default:
                throw new Bam.Core.Exception("Unknown bit depth, {0}", settings.Bits.ToString());
            }
            switch (settings.OutputType)
            {
            case C.ELinkerOutput.Executable:
                {
                    configuration["EXECUTABLE_PREFIX"] = new XcodeBuilder.UniqueConfigurationValue(string.Empty);
                    var ext = module.CreateTokenizedString("$(exeext)").Parse().TrimStart(new [] {'.'});
                    configuration["EXECUTABLE_EXTENSION"] = new XcodeBuilder.UniqueConfigurationValue(ext);
                }
                break;

            case C.ELinkerOutput.DynamicLibrary:
                {
                    if ((module is C.Plugin) || (module is C.Cxx.Plugin))
                    {
                        var prefix = module.CreateTokenizedString("$(pluginprefix)").Parse();
                        configuration["EXECUTABLE_PREFIX"] = new XcodeBuilder.UniqueConfigurationValue(prefix);
                        var ext = module.CreateTokenizedString("$(pluginext)").Parse().TrimStart(new [] {'.'});
                        configuration["EXECUTABLE_EXTENSION"] = new XcodeBuilder.UniqueConfigurationValue(ext);
                    }
                    else
                    {
                        var prefix = module.CreateTokenizedString("$(dynamicprefix)").Parse();
                        configuration["EXECUTABLE_PREFIX"] = new XcodeBuilder.UniqueConfigurationValue(prefix);
                        var ext = module.CreateTokenizedString("$(dynamicextonly)").Parse().TrimStart(new [] {'.'});
                        configuration["EXECUTABLE_EXTENSION"] = new XcodeBuilder.UniqueConfigurationValue(ext);
                    }
                    configuration["MACH_O_TYPE"] = new XcodeBuilder.UniqueConfigurationValue("mh_dylib");

                    var versionString = module.CreateTokenizedString("$(MajorVersion).$(MinorVersion)#valid(.$(PatchVersion))").Parse();
                    configuration["DYLIB_CURRENT_VERSION"] = new XcodeBuilder.UniqueConfigurationValue(versionString);
                    configuration["DYLIB_COMPATIBILITY_VERSION"] = new XcodeBuilder.UniqueConfigurationValue(versionString);
                }
                break;
            }
            if (settings.LibraryPaths.Count > 0)
            {
                var option = new XcodeBuilder.MultiConfigurationValue();
                foreach (var path in settings.LibraryPaths)
                {
                    option.Add(path.Parse());
                }
                configuration["LIBRARY_SEARCH_PATHS"] = option;
            }
            foreach (var path in settings.Libraries)
            {
                var workspace = Bam.Core.Graph.Instance.MetaData as XcodeBuilder.WorkspaceMeta;
                var encapsulating = module.GetEncapsulatingReferencedModule();
                var target = workspace.EnsureTargetExists(encapsulating);
                var libname = path.Replace("-l", string.Empty);

                // need to find where this library is because Xcode requires a path to it
                // first check all of the library paths
                var buildFile = FindLibraryInLibrarySearchPaths(target, settings, libname);
                if (null == buildFile)
                {
                    // no match, so try the current SDK path
                    buildFile = FindLibraryInSDKSearchPaths(target, settings, libname);
                    if (null == buildFile)
                    {
                        throw new Bam.Core.Exception("Unable to find library {0} on any search path or in the SDK", path);
                    }
                }
            }
            if (settings.DebugSymbols)
            {
                var option = new XcodeBuilder.MultiConfigurationValue();
                option.Add("-g");
                configuration["OTHER_LDFLAGS"] = option;
            }
        }
    }
}
