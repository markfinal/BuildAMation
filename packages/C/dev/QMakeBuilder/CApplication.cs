// <copyright file="CApplication.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        public object Build(C.Application moduleToBuild, out bool success)
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
#if true
            data.DestDir = moduleToBuild.Locations[C.Application.OutputDirLocKey];
#else
            data.DestDir = options.OutputDirectoryPath;
#endif

            // find dependent library files
            if (null != node.ExternalDependents)
            {
                var keysToFilter = new Opus.Core.Array<Opus.Core.LocationKey>(
                    C.StaticLibrary.OutputFileLocKey,
                    C.DynamicLibrary.StaticImportLibraryLocationKey
                    );

                var dependentLibraryFiles = new Opus.Core.LocationArray();
                node.ExternalDependents.FilterOutputLocations(keysToFilter, dependentLibraryFiles);
                data.Libraries.AddRangeUnique(dependentLibraryFiles);
            }

            var optionsInterface = moduleToBuild.Options as C.ILinkerOptions;

            // find static library files
            data.ExternalLibraries.AddRangeUnique(optionsInterface.Libraries.ToStringArray());

            // find library paths
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
                    var headerFileCollection = field.GetValue(moduleToBuild) as Opus.Core.FileCollection;
                    data.Headers.AddRangeUnique(headerFileCollection.ToStringArray());
                }
            }

            if (optionsInterface is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineBuilder = new Opus.Core.StringArray();
                var target = node.Target;
                var commandLineOption = optionsInterface as CommandLineProcessor.ICommandLineSupport;
                var excludedOptionNames = new Opus.Core.StringArray();
                excludedOptionNames.Add("OutputType");
                excludedOptionNames.Add("LibraryPaths");
                excludedOptionNames.Add("GenerateMapFile"); // TODO: better way of extracting the map file?
                excludedOptionNames.Add("DebugSymbols"); // TODO: better way of extracting the PDB file?
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, excludedOptionNames);
                data.LinkFlags.AddRangeUnique(commandLineBuilder);
            }
            else
            {
                throw new Opus.Core.Exception("Linker options does not support command line translation");
            }

            success = true;
            return data;
        }
    }
}
