namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object
        Build(
            CodeGenTest.CodeGenModule moduleToBuild,
            out bool success)
        {
            var codeGenModuleModule = moduleToBuild as Bam.Core.BaseModule;
            var node = codeGenModuleModule.OwningNode;
            var target = node.Target;
            var codeGenModuleOptions = codeGenModuleModule.Options;
            var toolOptions = codeGenModuleOptions as CodeGenTest.CodeGenOptionCollection;
            var tool = target.Toolset.Tool(typeof(CodeGenTest.ICodeGenTool));

            // dependency checking
            {
                var inputLocations = new Bam.Core.LocationArray(
                    Bam.Core.FileLocation.Get(tool.Executable((Bam.Core.BaseTarget)target), Bam.Core.Location.EExists.WillExist)
                    );
                var outputLocations = moduleToBuild.Locations.FilterByType(Bam.Core.ScaffoldLocation.ETypeHint.File, Bam.Core.Location.EExists.WillExist);
                if (!RequiresBuilding(outputLocations, inputLocations))
                {
                    Bam.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                    success = true;
                    return null;
                }
            }

            // at this point, we know the node outputs need building

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Bam.Core.ScaffoldLocation.ETypeHint.Directory, Bam.Core.Location.EExists.WillExist);
            foreach (var dir in dirsToCreate)
            {
                var dirPath = dir.GetSinglePath();
                NativeBuilder.MakeDirectory(dirPath);
            }

            var commandLineBuilder = new Bam.Core.StringArray();
            if (toolOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = toolOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
            }
            else
            {
                throw new Bam.Core.Exception("CodeGen options does not support command line translation");
            }

            var exitCode = CommandLineProcessor.Processor.Execute(node, tool, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}