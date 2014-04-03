// <copyright file="NoResponseFileAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(CommandLineProcessor.NoResponseFileAction))]

namespace CommandLineProcessor
{
    [Opus.Core.PreambleAction]
    public sealed class NoResponseFileAction : Opus.Core.IAction
    {
        public NoResponseFileAction()
        {
            if (!Opus.Core.State.HasCategory("Build"))
            {
                Opus.Core.State.AddCategory("Build");
            }
            Opus.Core.State.Add<bool>("Build", "DisableResponseFiles", false);
        }

        string Opus.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-noresponsefiles";
            }
        }

        string Opus.Core.IAction.Description
        {
            get
            {
                return "Disable use of response files";
            }
        }

        bool Opus.Core.IAction.Execute()
        {
            Opus.Core.State.Set("Build", "DisableResponseFiles", true);

            Opus.Core.Log.DebugMessage("Response files have been disabled");

            return true;
        }

        #region ICloneable Members

        object System.ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}
