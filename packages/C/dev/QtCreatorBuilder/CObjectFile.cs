// <copyright file="CObjectFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace QtCreatorBuilder
{
    public sealed partial class QtCreatorBuilder
    {
        public object Build(C.ObjectFile objectFile, Opus.Core.DependencyNode node, out bool success)
        {
            Opus.Core.Target target = node.Target;
            C.CompilerOptionCollection compilerOptions = objectFile.Options as C.CompilerOptionCollection;
            System.Text.StringBuilder commandLineBuilder = new System.Text.StringBuilder();
            if (compilerOptions is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = compilerOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);
            }
            else
            {
                throw new Opus.Core.Exception("Compiler options does not support command line translation");
            }

            NodeData nodeData = new NodeData();
            nodeData.AddVariable("SOURCES", objectFile.SourceFile.AbsolutePath);
            nodeData.AddUniqueVariable("CFLAGS", commandLineBuilder.ToString());
            nodeData.AddUniqueVariable("OBJECTS_DIR", compilerOptions.ObjectFilePath);
            success = true;
            return nodeData;
        }
    }
}