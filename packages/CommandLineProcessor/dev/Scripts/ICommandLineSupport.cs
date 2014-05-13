// <copyright file="ICommandLineSupport.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CommandLineProcessor package</summary>
// <author>Mark Final</author>
namespace CommandLineProcessor
{
    public interface ICommandLineSupport
    {
        void ToCommandLineArguments(Opus.Core.StringArray commandLineBuilder,
                                    Opus.Core.Target target,
                                    Opus.Core.StringArray excludedOptionNames);
    }
}
