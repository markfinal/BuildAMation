// <copyright file="PrivateData.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
namespace CSharp
{
    public sealed class PrivateData : CommandLineProcessor.ICommandLineDelegate, VisualStudioProcessor.IVisualStudioDelegate
    {
        public PrivateData(CommandLineProcessor.Delegate commandLineDelegate, VisualStudioProcessor.Delegate visualStudioDelegate)
        {
            this.CommandLineDelegate = commandLineDelegate;
            this.VisualStudioProjectDelegate = visualStudioDelegate;
        }

        public CommandLineProcessor.Delegate CommandLineDelegate
        {
            get;
            set;
        }

        public VisualStudioProcessor.Delegate VisualStudioProjectDelegate
        {
            get;
            set;
        }
    }
}