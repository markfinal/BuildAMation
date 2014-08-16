// <copyright file="ForceBuildAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>NativeBuilder package</summary>
// <author>Mark Final</author>

[assembly: Bam.Core.RegisterAction(typeof(NativeBuilder.ForceBuildAction))]

namespace NativeBuilder
{
    [Bam.Core.PreambleAction]
    public sealed class ForceBuildAction :
        Bam.Core.IAction
    {
        public
        ForceBuildAction()
        {
            if (!Bam.Core.State.HasCategory("NativeBuilder"))
            {
                Bam.Core.State.AddCategory("NativeBuilder");
            }
            Bam.Core.State.Add<bool>("NativeBuilder", "ForceBuild", false);
        }

        string Bam.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-forcebuild";
            }
        }

        string Bam.Core.IAction.Description
        {
            get
            {
                return "Force a build by not performing any dependency checks";
            }
        }

        bool
        Bam.Core.IAction.Execute()
        {
            Bam.Core.State.Set("NativeBuilder", "ForceBuild", true);

            Bam.Core.Log.DebugMessage("Native builds are now forced");

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
