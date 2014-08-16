// <copyright file="WarmXcodeSchemeCacheAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>

[assembly: Bam.Core.RegisterAction(typeof(XcodeBuilder.WarmXcodeSchemeCacheAction))]

namespace XcodeBuilder
{
    [Bam.Core.PreambleAction]
    public sealed class WarmXcodeSchemeCacheAction :
        Bam.Core.IAction
    {
        public
        WarmXcodeSchemeCacheAction()
        {
            if (!Bam.Core.State.HasCategory("XcodeBuilder"))
            {
                Bam.Core.State.AddCategory("XcodeBuilder");
            }
            Bam.Core.State.Add<bool>("XcodeBuilder", "WarmSchemeCache", false);
        }

        string Bam.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-warmschemecache";
            }
        }

        string Bam.Core.IAction.Description
        {
            get
            {
                return "Warms Xcode project scheme caches, in order to use xcodebuild on a container workspace without loading it into the Xcode UI";
            }
        }

        bool
        Bam.Core.IAction.Execute()
        {
            Bam.Core.State.Set("XcodeBuilder", "WarmSchemeCache", true);

            Bam.Core.Log.DebugMessage("Xcode project scheme caches will be warmed at the end of the build");

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
