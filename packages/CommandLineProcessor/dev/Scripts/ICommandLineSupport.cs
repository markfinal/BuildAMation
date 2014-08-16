// <copyright file="ICommandLineSupport.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CommandLineProcessor package</summary>
// <author>Mark Final</author>
namespace CommandLineProcessor
{
    public interface ICommandLineSupport
    {
        void
        ToCommandLineArguments(
            Bam.Core.StringArray commandLineBuilder,
            Bam.Core.Target target,
            Bam.Core.StringArray excludedOptionNames);
    }
}
