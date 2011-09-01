// <copyright file="ForceBuildAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>NativeBuilder package</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(NativeBuilder.ForceBuildAction))]

namespace NativeBuilder
{
    public sealed class ForceBuildAction : Opus.Core.IAction
    {
        public ForceBuildAction()
        {
            Opus.Core.State.AddCategory("NativeBuilder");
            Opus.Core.State.Add<bool>("NativeBuilder", "ForceBuild", false);
        }

        string Opus.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-forcebuild";
            }
        }

        string Opus.Core.IAction.Description
        {
            get
            {
                return "Force a build by not performing any dependency checks";
            }
        }

        bool Opus.Core.IAction.Execute()
        {
            Opus.Core.State.Set("NativeBuilder", "ForceBuild", true);

            Opus.Core.Log.DebugMessage("Native builds are now forced");

            return true;
        }
    }
}
