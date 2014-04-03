// <copyright file="PrivateData.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    public sealed class PrivateData : CommandLineProcessor.ICommandLineDelegate, VisualStudioProcessor.IVisualStudioDelegate
    {
        public PrivateData(CommandLineProcessor.Delegate commandLineDelegate, VisualStudioProcessor.Delegate visualStudioProjectDelegate)
        {
            this.CommandLineDelegate = commandLineDelegate;
            this.VisualStudioProjectDelegate = visualStudioProjectDelegate;
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