// <copyright file="QtTest.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    public abstract class Test :
        Base
    {
        public
        Test()
        {
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtTest_IncludePaths);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtTest_VisualCWarningLevel);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtTest_LinkerOptions);
        }

        public override void
        RegisterOutputFiles(
            Opus.Core.BaseOptionCollection options,
            Opus.Core.Target target,
            string modulePath)
        {
            this.GetModuleDynamicLibrary(target, "QtTest");
            base.RegisterOutputFiles(options, target, modulePath);
        }

        [C.ExportLinkerOptionsDelegate]
        void
        QtTest_LinkerOptions(
            Opus.Core.IModule module,
            Opus.Core.Target target)
        {
            var options = module.Options as C.ILinkerOptions;
            if (null != options)
            {
                this.AddLibraryPath(options, target);
                this.AddModuleLibrary(options, target, "QtTest");
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void
        QtTest_VisualCWarningLevel(
            Opus.Core.IModule module,
            Opus.Core.Target target)
        {
            var options = module.Options as VisualCCommon.ICCompilerOptions;
            if (null != options)
            {
                // QtTest headers do not compile at warning level 4
                options.WarningLevel = VisualCCommon.EWarningLevel.Level3;
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void
        QtTest_IncludePaths(
            Opus.Core.IModule module,
            Opus.Core.Target target)
        {
            var options = module.Options as C.ICCompilerOptions;
            if (null != options)
            {
                this.AddIncludePath(options, target, "QtTest");
            }
        }
    }
}
