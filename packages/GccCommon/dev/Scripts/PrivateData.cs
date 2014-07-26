// <copyright file="PrivateData.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    public sealed class PrivateData :
        CommandLineProcessor.ICommandLineDelegate,
        XcodeProjectProcessor.IXcodeProjectDelegate
    {
        // this constructor is for the non-Xcode Gcc code paths
        public
        PrivateData(
            CommandLineProcessor.Delegate commandLineDelegate)
        {
            this.CommandLineDelegate = commandLineDelegate;
            this.XcodeProjectDelegate = null;
        }

        public
        PrivateData(
            CommandLineProcessor.Delegate commandLineDelegate,
            XcodeProjectProcessor.Delegate xcodeProjectDelegate)
        {
            this.CommandLineDelegate = commandLineDelegate;
            this.XcodeProjectDelegate = xcodeProjectDelegate;
        }

        public CommandLineProcessor.Delegate CommandLineDelegate
        {
            get;
            set;
        }

        public XcodeProjectProcessor.Delegate XcodeProjectDelegate
        {
            get;
            set;
        }
    }
}
