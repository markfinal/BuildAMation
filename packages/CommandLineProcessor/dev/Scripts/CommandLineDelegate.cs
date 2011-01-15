// <copyright file="CommandLineDelegate.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CommandLineProcessor package</summary>
// <author>Mark Final</author>
namespace CommandLineProcessor
{
    public delegate void Delegate(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target);
}