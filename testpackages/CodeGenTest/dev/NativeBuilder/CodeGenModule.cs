namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object
        Build(
            CodeGenTest.CodeGenModule moduleToBuild,
            out bool success)
        {
            var codeGenModuleModule = moduleToBuild as Opus.Core.BaseModule;
            var node = codeGenModuleModule.OwningNode;
            var target = node.Target;
            var codeGenModuleOptions = codeGenModuleModule.Options;
            var toolOptions = codeGenModuleOptions as CodeGenTest.CodeGenOptionCollection;
            var tool = target.Toolset.Tool(typeof(CodeGenTest.ICodeGenTool));

            // dependency checking
            {
                var inputLocations = new Opus.Core.LocationArray(
                    Opus.Core.FileLocation.Get(tool.Executable((Opus.Core.BaseTarget)target), Opus.Core.Location.EExists.WillExist)
                    );
                var outputLocations = moduleToBuild.Locations.FilterByType(Opus.Core.ScaffoldLocation.ETypeHint.File, Opus.Core.Location.EExists.WillExist);
                if (!RequiresBuilding(outputLocations, inputLocations))
                {
                    Opus.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                    success = true;
                    return null;
                }
            }

            // at this point, we know the node outputs need building

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Opus.Core.ScaffoldLocation.ETypeHint.Directory, Opus.Core.Location.EExists.WillExist);
            foreach (var dir in dirsToCreate)
            {
                var dirPath = dir.GetSinglePath();
                NativeBuilder.MakeDirectory(dirPath);
            }

            var commandLineBuilder = new Opus.Core.StringArray();
            if (toolOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = toolOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
            }
            else
            {
                throw new Opus.Core.Exception("CodeGen options does not support command line translation");
            }

            var exitCode = CommandLineProcessor.Processor.Execute(node, tool, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}