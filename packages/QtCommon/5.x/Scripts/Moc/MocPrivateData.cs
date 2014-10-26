#region License
// <copyright>
//  Mark Final
// </copyright>
// <author>Mark Final</author>
#endregion // License
namespace QtCommon
{
    public sealed class MocPrivateData :
        CommandLineProcessor.ICommandLineDelegate
    {
        public
        MocPrivateData(
            CommandLineProcessor.Delegate commandLineDelegate)
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
