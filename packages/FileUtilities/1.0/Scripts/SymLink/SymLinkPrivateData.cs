// <copyright file="SymLinkPrivateData.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    public sealed class SymLinkPrivateData : CommandLineProcessor.ICommandLineDelegate
    {
        public SymLinkPrivateData(CommandLineProcessor.Delegate commandLineDelegate)
        {
            this.CommandLineDelegate = commandLineDelegate;
        }

        public CommandLineProcessor.Delegate CommandLineDelegate
        {
            get;
            set;
        }
    }
}