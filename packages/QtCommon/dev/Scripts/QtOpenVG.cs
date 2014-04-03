// <copyright file="QtOpenVG.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    public abstract class OpenVG : Base
    {
        public OpenVG()
        {
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtOpenVG_IncludePaths);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtOpenVG_VisualCWarningLevel);
        }

        [C.ExportCompilerOptionsDelegate]
        void QtOpenVG_VisualCWarningLevel(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var options = module.Options as VisualCCommon.ICCompilerOptions;
            if (null != options)
            {
                // QtOpenVG headers do not compile at warning level 4
                options.WarningLevel = VisualCCommon.EWarningLevel.Level3;
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void QtOpenVG_IncludePaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var options = module.Options as C.ICCompilerOptions;
            if (null != options)
            {
                this.AddIncludePath(options, target, "QtOpenVG");
            }
        }
    }
}
