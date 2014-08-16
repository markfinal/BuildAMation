// <copyright file="NoResponseFileAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>

[assembly: Bam.Core.RegisterAction(typeof(CommandLineProcessor.NoResponseFileAction))]

namespace CommandLineProcessor
{
    [Bam.Core.PreambleAction]
    public sealed class NoResponseFileAction :
        Bam.Core.IAction
    {
        public
        NoResponseFileAction()
        {
            if (!Bam.Core.State.HasCategory("Build"))
            {
                Bam.Core.State.AddCategory("Build");
            }
            Bam.Core.State.Add<bool>("Build", "DisableResponseFiles", false);
        }

        string Bam.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-noresponsefiles";
            }
        }

        string Bam.Core.IAction.Description
        {
            get
            {
                return "Disable use of response files";
            }
        }

        bool
        Bam.Core.IAction.Execute()
        {
            Bam.Core.State.Set("Build", "DisableResponseFiles", true);

            Bam.Core.Log.DebugMessage("Response files have been disabled");

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
