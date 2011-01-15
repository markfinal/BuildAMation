// <copyright file="ICommandLineSupport.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CommandLineProcessor package</summary>
// <author>Mark Final</author>
namespace CommandLineProcessor
{
    public interface ICommandLineSupport
    {
        void ToCommandLineArguments(System.Text.StringBuilder commandLineStringBuilder, Opus.Core.Target target);

        Opus.Core.DirectoryCollection DirectoriesToCreate();
    }
}
