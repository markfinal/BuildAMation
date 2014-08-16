// <copyright file="ShowDirectoryAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.ShowDirectoryAction))]

namespace Bam
{
    [Core.TriggerAction]
    internal class ShowDirectoryAction :
        Core.IAction
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-showdirectory";
            }
        }

        public string Description
        {
            get
            {
                return "Show the Opus directory";
            }
        }

        public bool
        Execute()
        {
            Core.Log.MessageAll(Core.State.OpusDirectory);

            return true;
        }

        #region ICloneable Members

        object
        System.ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}