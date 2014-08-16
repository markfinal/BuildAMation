// <copyright file="CommandLineDelegate.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CommandLineProcessor package</summary>
// <author>Mark Final</author>
namespace CommandLineProcessor
{
    public delegate void
    Delegate(
        object sender,
        Bam.Core.StringArray commandLineBuilder,
        Bam.Core.Option option,
        Bam.Core.Target target);
}
