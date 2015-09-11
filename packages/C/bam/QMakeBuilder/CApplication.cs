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
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        public object
        Build(
            C.Application moduleToBuild,
            out bool success)
        {
            var node = moduleToBuild.OwningNode;
            var options = moduleToBuild.Options as C.LinkerOptionCollection;

            var data = new QMakeData(node);
            foreach (var child in node.Children)
            {
                var childData = child.Data as QMakeData;
                if (null != childData)
                {
                    data.Merge(childData);
                }
            }
            if (null != node.ExternalDependents)
            {
                foreach (var dependent in node.ExternalDependents)
                {
                    var depData = dependent.Data as QMakeData;
                    if (null != depData)
                    {
                        data.Merge(depData, QMakeData.OutputType.StaticLibrary | QMakeData.OutputType.DynamicLibrary | QMakeData.OutputType.HeaderLibrary);
                    }
                }
            }

            data.Target = options.OutputName;
            data.Output = QMakeData.OutputType.Application;
            data.DestDir = moduleToBuild.Locations[C.Application.OutputDir];

            // find dependent library files
            if (null != node.ExternalDependents)
            {
                var target = node.Target;
                var libraryKeysToFilter = new Bam.Core.Array<Bam.Core.LocationKey>(
                    C.StaticLibrary.OutputFileLocKey
                );
                if (target.HasPlatform(Bam.Core.EPlatform.Linux))
                {
                    libraryKeysToFilter.Add(C.PosixSharedLibrarySymlinks.LinkerSymlink);
                }
                else if (target.HasPlatform(Bam.Core.EPlatform.Windows))
                {
                    libraryKeysToFilter.Add(C.DynamicLibrary.ImportLibraryFile);
                }
                else if (target.HasPlatform(Bam.Core.EPlatform.OSX))
                {
                    libraryKeysToFilter.Add(C.DynamicLibrary.OutputFile);
                }

                var dependentLibraryFiles = new Bam.Core.LocationArray();
                node.ExternalDependents.FilterOutputLocations(libraryKeysToFilter, dependentLibraryFiles);
                data.Libraries.AddRangeUnique(dependentLibraryFiles);
            }

            var optionsInterface = moduleToBuild.Options as C.ILinkerOptions;

            // find static library files
            data.ExternalLibraries.AddRangeUnique(optionsInterface.Libraries.ToStringArray());

            // find library paths
            // TODO: convert to var, or Location
            foreach (string libPath in optionsInterface.LibraryPaths)
            {
                if (libPath.Contains(" "))
                {
                    data.ExternalLibraries.Add("-L$$quote(" + libPath + ")");
                }
                else
                {
                    data.ExternalLibraries.Add("-L" + libPath);
                }
            }

            // find headers
            var fieldBindingFlags = System.Reflection.BindingFlags.Instance |
                                    System.Reflection.BindingFlags.Public |
                                    System.Reflection.BindingFlags.NonPublic;
            var fields = moduleToBuild.GetType().GetFields(fieldBindingFlags);
            foreach (var field in fields)
            {
                var headerFileAttributes = field.GetCustomAttributes(typeof(C.HeaderFilesAttribute), false);
                if (headerFileAttributes.Length > 0)
                {
                    var headerFileCollection = field.GetValue(moduleToBuild) as Bam.Core.FileCollection;
                    data.Headers.AddRangeUnique(headerFileCollection.ToStringArray());
                }
            }

            if (optionsInterface is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineBuilder = new Bam.Core.StringArray();
                var target = node.Target;
                var commandLineOption = optionsInterface as CommandLineProcessor.ICommandLineSupport;
                var excludedOptionNames = new Bam.Core.StringArray();
                excludedOptionNames.Add("OutputType");
                excludedOptionNames.Add("LibraryPaths");
                excludedOptionNames.Add("GenerateMapFile"); // TODO: better way of extracting the map file? yes, locations
                excludedOptionNames.Add("DebugSymbols");
                if (target.HasPlatform(Bam.Core.EPlatform.NotWindows))
                {
                    excludedOptionNames.Add("RPath");
                }
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, excludedOptionNames);
                data.LinkFlags.AddRangeUnique(commandLineBuilder);

                // library paths and libs
                {
                    var linkerOptions = options as C.ILinkerOptions;
                    foreach (var linkerSearchPath in linkerOptions.LibraryPaths)
                    {
                        // Note: the -L prefix is for all platforms
                        // Note: this is not the best way to use a DirectoryLocation
                        data.Libraries.AddUnique(Bam.Core.DirectoryLocation.Get(System.String.Format("-L{0}", linkerSearchPath), Bam.Core.Location.EExists.WillExist));
                    }
                    // TODO: convert to var
                    foreach (Bam.Core.FileLocation libraryPath in linkerOptions.Libraries)
                    {
                        data.Libraries.AddUnique(libraryPath);
                    }
                }

                // debug symbols
                {
                    var commandLine = new Bam.Core.StringArray();
                    var optionNames = new Bam.Core.StringArray("DebugSymbols");
                    CommandLineProcessor.ToCommandLine.ExecuteForOptionNames(options, commandLine, target, optionNames);
                    if (!data.CustomPathVariables.ContainsKey("QMAKE_LFLAGS_DEBUG"))
                    {
                        data.CustomPathVariables["QMAKE_LFLAGS_DEBUG"] = new Bam.Core.StringArray();
                    }
                    foreach (var option in commandLine)
                    {
                        data.CustomPathVariables["QMAKE_LFLAGS_DEBUG"].AddUnique(option);
                    }
                }

                // rpath
                if (target.HasPlatform(Bam.Core.EPlatform.NotWindows))
                {
                    var commandLine = new Bam.Core.StringArray();
                    var optionNames = new Bam.Core.StringArray("RPath");
                    CommandLineProcessor.ToCommandLine.ExecuteForOptionNames(options, commandLine, target, optionNames);
                    foreach (var option in commandLine)
                    {
                        var linkerCommand = option.Split(',');
                        var rpathDir = linkerCommand[linkerCommand.Length - 1];
                        // unable to insert $ORIGIN into QMAKE_RPATHDIR, that is suggested at
                        // http://www.opensource.apple.com/source/WebKit/WebKit-7534.56.5/qt/declarative/declarative.pro
                        // so use a workaround
                        rpathDir = rpathDir.Replace("$ORIGIN", "$$DESTDIR");
                        data.RPathDir.AddUnique(rpathDir);
                    }
                }
            }
            else
            {
                throw new Bam.Core.Exception("Linker options does not support command line translation");
            }

            success = true;
            return data;
        }
    }
}
