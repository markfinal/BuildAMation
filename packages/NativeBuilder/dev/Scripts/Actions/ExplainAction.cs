// <copyright file="ExplainAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>NativeBuilder package</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(NativeBuilder.ExplainAction))]

namespace NativeBuilder
{
    [Opus.Core.PreambleAction]
    public sealed class ExplainAction : Opus.Core.IAction
    {
        public ExplainAction()
        {
            if (!Opus.Core.State.HasCategory("NativeBuilder"))
            {
                Opus.Core.State.AddCategory("NativeBuilder");
            }
            Opus.Core.State.Add<bool>("NativeBuilder", "Explain", false);
        }

        string Opus.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-explain";
            }
        }

        string Opus.Core.IAction.Description
        {
            get
            {
                return "Explain why builds occur due to dependency checking";
            }
        }

        bool Opus.Core.IAction.Execute()
        {
            Opus.Core.State.Set("NativeBuilder", "Explain", true);

            Opus.Core.Log.DebugMessage("Explained builds are enabled");

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
