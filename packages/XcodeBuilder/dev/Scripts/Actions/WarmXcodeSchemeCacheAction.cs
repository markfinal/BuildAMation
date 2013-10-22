// <copyright file="WarmXcodeSchemeCacheAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(XcodeBuilder.WarmXcodeSchemeCacheAction))]

namespace XcodeBuilder
{
    [Opus.Core.PreambleAction]
    public sealed class WarmXcodeSchemeCacheAction : Opus.Core.IAction
    {
        public WarmXcodeSchemeCacheAction()
        {
            if (!Opus.Core.State.HasCategory("XcodeBuilder"))
            {
                Opus.Core.State.AddCategory("XcodeBuilder");
            }
            Opus.Core.State.Add<bool>("XcodeBuilder", "WarmSchemeCache", false);
        }

        string Opus.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-warmschemecache";
            }
        }

        string Opus.Core.IAction.Description
        {
            get
            {
                return "Warms Xcode project scheme caches, in order to use xcodebuild on a container workspace without loading it into the Xcode UI";
            }
        }

        bool Opus.Core.IAction.Execute()
        {
            Opus.Core.State.Set("XcodeBuilder", "WarmSchemeCache", true);

            Opus.Core.Log.DebugMessage("Xcode project scheme caches will be warmed at the end of the build");

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
