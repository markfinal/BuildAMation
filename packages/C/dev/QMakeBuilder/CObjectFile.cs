// <copyright file="CObjectFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        public object Build(C.ObjectFile objectFile, Opus.Core.DependencyNode node, out bool success)
        {
            Opus.Core.Target target = node.Target;

            C.CompilerOptionCollection compilerOptions = objectFile.Options as C.CompilerOptionCollection;
            Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
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
            nodeData.Configuration = GetQtConfiguration(target);
            nodeData.AddVariable("SOURCES", objectFile.SourceFile.AbsolutePath);
            nodeData.AddUniqueVariable("CFLAGS", commandLineBuilder);
            nodeData.AddUniqueVariable("OBJECTS_DIR", new Opus.Core.StringArray(compilerOptions.OutputDirectoryPath));
            success = true;
            return nodeData;
        }
    }
}