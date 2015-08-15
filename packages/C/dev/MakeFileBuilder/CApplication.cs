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
using Bam.Core.V2; // for EPlatform.PlatformExtensions
namespace C
{
namespace V2
{
    public sealed class MakeFileLinker :
        ILinkerPolicy
    {
        private static string
        GetLibraryPath(Bam.Core.V2.Module module)
        {
            if (module is C.V2.StaticLibrary)
            {
                return module.GeneratedPaths[C.V2.StaticLibrary.Key].ToString();
            }
            else if (module is C.V2.DynamicLibrary)
            {
                if (module.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                {
                    return module.GeneratedPaths[C.V2.DynamicLibrary.ImportLibraryKey].ToString();
                }
                else
                {
                    return module.GeneratedPaths[C.V2.DynamicLibrary.Key].ToString();
                }
            }
            else if (module is C.V2.CSDKModule)
            {
                // collection of libraries, none in particular
                return null;
            }
            else if (module is ExternalFramework)
            {
                // dealt with elsewhere
                return null;
            }
            else
            {
                throw new Bam.Core.Exception("Unknown module library type: {0}", module.GetType());
            }
        }

        void
        ILinkerPolicy.Link(
            ConsoleApplication sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.TokenizedString executablePath,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> objectFiles,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> headers,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> libraries,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> frameworks)
        {
            var linker = sender.Settings as C.V2.ICommonLinkerOptions;
            // TODO: could the lib search paths be in the staticlibrary base class as a patch?
            foreach (var library in libraries)
            {
                var fullLibraryPath = GetLibraryPath(library);
                if (null == fullLibraryPath)
                {
                    continue;
                }
                var dir = System.IO.Path.GetDirectoryName(fullLibraryPath);
                linker.LibraryPaths.AddUnique(Bam.Core.V2.TokenizedString.Create(dir, null));
            }

            var commandLineArgs = new Bam.Core.StringArray();
            var interfaceType = Bam.Core.State.ScriptAssembly.GetType("CommandLineProcessor.V2.IConvertToCommandLine");
            if (interfaceType.IsAssignableFrom(sender.Settings.GetType()))
            {
                var map = sender.Settings.GetType().GetInterfaceMap(interfaceType);
                map.InterfaceMethods[0].Invoke(sender.Settings, new[] { sender, commandLineArgs as object });
            }

            var meta = new MakeFileBuilder.V2.MakeFileMeta(sender);
            meta.Target = executablePath;
            foreach (var module in objectFiles)
            {
                if (module is Bam.Core.V2.IModuleGroup)
                {
                    foreach (var child in module.Children)
                    {
                        meta.Prequisities.Add(child, C.V2.ObjectFile.Key);
                    }
                }
                else
                {
                    meta.Prequisities.Add(module, C.V2.ObjectFile.Key);
                }
            }
            foreach (var module in libraries)
            {
                if (module is StaticLibrary)
                {
                    meta.Prequisities.Add(module, C.V2.StaticLibrary.Key);
                }
                else if (module is DynamicLibrary)
                {
                    if (module.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        meta.Prequisities.Add(module, C.V2.DynamicLibrary.ImportLibraryKey);
                    }
                    else
                    {
                        meta.Prequisities.Add(module, C.V2.DynamicLibrary.Key);
                    }
                }
                else if (module is CSDKModule)
                {
                    continue;
                }
                else if (module is ExternalFramework)
                {
                    continue;
                }
                else
                {
                    throw new Bam.Core.Exception("Unknown module library type: {0}", module.GetType());
                }
            }
            // TODO: frameworks
            var rule = new System.Text.StringBuilder();
            rule.AppendFormat(sender.Tool.Executable.ContainsSpace ? "\"{0}\" {1} $^" : "{0} {1} $^", sender.Tool.Executable, commandLineArgs.ToString(' '));
            meta.Recipe.Add(rule.ToString());

            var executableDir = System.IO.Path.GetDirectoryName(executablePath.ToString());
            meta.CommonMetaData.Directories.AddUnique(executableDir);
            meta.CommonMetaData.ExtendEnvironmentVariables(sender.Tool.EnvironmentVariables);
        }
    }
}
}
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object
        Build(
            C.Application moduleToBuild,
            out bool success)
        {
            var applicationModule = moduleToBuild as Bam.Core.BaseModule;
            var node = applicationModule.OwningNode;
            var target = node.Target;

            // dependents
            var inputVariables = new MakeFileVariableDictionary();
            var dataArray = new System.Collections.Generic.List<MakeFileData>();
            if (null != node.Children)
            {
                var keysToFilter = new Bam.Core.Array<Bam.Core.LocationKey>(
                    C.ObjectFile.OutputFile
                );

                foreach (var childNode in node.Children)
                {
                    if (null == childNode.Data)
                    {
                        continue;
                    }
                    var data = childNode.Data as MakeFileData;
                    inputVariables.Append(data.VariableDictionary.Filter(keysToFilter));
                    dataArray.Add(data);
                }
            }
            if (null != node.ExternalDependents)
            {
                var libraryKeysToFilter = new Bam.Core.Array<Bam.Core.LocationKey>(
                    C.StaticLibrary.OutputFileLocKey);
                if (target.HasPlatform(Bam.Core.EPlatform.Unix))
                {
                    // TODO: why is the symlink not present?
                    //libraryKeysToFilter.Add(C.PosixSharedLibrarySymlinks.LinkerSymlink);
                    libraryKeysToFilter.Add(C.DynamicLibrary.OutputFile);
                }
                else if (target.HasPlatform(Bam.Core.EPlatform.Windows))
                {
                    libraryKeysToFilter.Add(C.DynamicLibrary.ImportLibraryFile);
                }
                else if (target.HasPlatform(Bam.Core.EPlatform.OSX))
                {
                    libraryKeysToFilter.Add(C.DynamicLibrary.OutputFile);
                }

                foreach (var dependentNode in node.ExternalDependents)
                {
                    if (null == dependentNode.Data)
                    {
                        continue;
                    }
                    var data = dependentNode.Data as MakeFileData;
                    inputVariables.Append(data.VariableDictionary.Filter(libraryKeysToFilter));
                    dataArray.Add(data);
                }
            }

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Bam.Core.ScaffoldLocation.ETypeHint.Directory, Bam.Core.Location.EExists.WillExist);

            var applicationOptions = applicationModule.Options;
            var commandLineBuilder = new Bam.Core.StringArray();
            if (applicationOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = applicationOptions as CommandLineProcessor.ICommandLineSupport;
                if (target.HasPlatform(Bam.Core.EPlatform.Windows))
                {
                    // libraries are manually added later
                    var excludedOptions = new Bam.Core.StringArray("Libraries", "StandardLibraries");
                    commandLineOption.ToCommandLineArguments(commandLineBuilder, target, excludedOptions);
                }
                else
                {
                    var excludedOptions = new Bam.Core.StringArray();
                    excludedOptions.Add("RPath"); // $ORIGIN is handled differently
                    excludedOptions.Add("Libraries");
                    excludedOptions.Add("StandardLibraries");
                    commandLineOption.ToCommandLineArguments(commandLineBuilder, target, excludedOptions);

                    // handle RPath separately
                    // what needs to happen is that the $ must be doubled, and the entire path quoted
                    // http://stackoverflow.com/questions/230364/how-to-get-rpath-with-origin-to-work-on-codeblocks-gcc
                    var rPathCommandLine = new Bam.Core.StringArray();
                    var optionNames = new Bam.Core.StringArray();
                    optionNames.Add("RPath");
                    CommandLineProcessor.ToCommandLine.ExecuteForOptionNames(applicationOptions, rPathCommandLine, target, optionNames);
                    foreach (var rpath in rPathCommandLine)
                    {
                        var linkerCommand = rpath.Split(',');
                        var rpathDir = linkerCommand[linkerCommand.Length - 1];
                        rpathDir = rpathDir.Replace("$ORIGIN", "$$ORIGIN");
                        rpathDir = System.String.Format("'{0}'", rpathDir);
                        var linkerCommandArray = new Bam.Core.StringArray(linkerCommand);
                        linkerCommandArray.Remove(linkerCommand[linkerCommand.Length - 1]);
                        linkerCommandArray.Add(rpathDir);
                        var newRPathCommand = linkerCommandArray.ToString(',');
                        commandLineBuilder.Add(newRPathCommand);
                    }
                }
            }
            else
            {
                throw new Bam.Core.Exception("Linker options does not support command line translation");
            }

            var toolset = target.Toolset;
            var linkerTool = toolset.Tool(typeof(C.ILinkerTool)) as C.ILinkerTool;
            var executable = linkerTool.Executable((Bam.Core.BaseTarget)target);

            var recipeBuilder = new System.Text.StringBuilder();
            if (executable.Contains(" "))
            {
                recipeBuilder.AppendFormat("\"{0}\"", executable);
            }
            else
            {
                recipeBuilder.Append(executable);
            }

            var dependentLibraryCommandLine = new Bam.Core.StringArray();
            {
                recipeBuilder.AppendFormat(" {0} ", commandLineBuilder.ToString(' '));

                var dependentLibraries = new Bam.Core.StringArray("$^");
                C.LinkerUtilities.AppendLibrariesToCommandLine(dependentLibraryCommandLine, linkerTool, applicationOptions as C.ILinkerOptions, dependentLibraries);
            }

            recipeBuilder.Append(dependentLibraryCommandLine.ToString(' '));
            var recipe = recipeBuilder.ToString();
            // replace primary target with $@
            var primaryOutputKey = C.Application.OutputFile;
            var outputPath = moduleToBuild.Locations[primaryOutputKey].GetSinglePath();
            recipe = recipe.Replace(outputPath, "$@");
            var instanceName = MakeFile.InstanceName(node);
            var toolOutputLocKeys = (linkerTool as Bam.Core.ITool).OutputLocationKeys(moduleToBuild);
            var outputFileLocations = moduleToBuild.Locations.Keys(Bam.Core.ScaffoldLocation.ETypeHint.File, Bam.Core.Location.EExists.WillExist);
            var outputFileLocationsOfInterest = outputFileLocations.Intersect(toolOutputLocKeys);

            // replace non-primary outputs with their variable names
            foreach (var outputKey in outputFileLocations)
            {
                if (outputKey == primaryOutputKey)
                {
                    continue;
                }

                var outputLoc = moduleToBuild.Locations[outputKey];
                if (!outputLoc.IsValid)
                {
                    continue;
                }

                var variableName = System.String.Format("$({0}_{1}_Variable)", instanceName, outputKey.ToString());
                var outputLocPath = outputLoc.GetSinglePath();
                recipe = recipe.Replace(outputLocPath, variableName);
            }

            var recipes = new Bam.Core.StringArray();
            recipes.Add(recipe);

            var makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            var rule = new MakeFileRule(
                moduleToBuild,
                C.Application.OutputFile,
                node.UniqueModuleName,
                dirsToCreate,
                inputVariables,
                null,
                recipes);
            rule.OutputLocationKeys = outputFileLocationsOfInterest;
            makeFile.RuleArray.Add(rule);

            var makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));

            using (var makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            var exportedTargets = makeFile.ExportedTargets;
            var exportedVariables = makeFile.ExportedVariables;
            System.Collections.Generic.Dictionary<string, Bam.Core.StringArray> environment = null;
            if (linkerTool is Bam.Core.IToolEnvironmentVariables)
            {
                environment = (linkerTool as Bam.Core.IToolEnvironmentVariables).Variables((Bam.Core.BaseTarget)target);
            }
            var returnData = new MakeFileData(makeFilePath, exportedTargets, exportedVariables, environment);
            success = true;
            return returnData;
        }
    }
}
