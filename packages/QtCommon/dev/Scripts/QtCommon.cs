// <copyright file="QtCommon.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    public abstract class QtCommon : C.ThirdPartyModule
    {
        protected static string installPath;
        protected Opus.Core.StringArray includePaths = new Opus.Core.StringArray();

        public string BinPath
        {
            get;
            protected set;
        }

        public string LibPath
        {
            get;
            protected set;
        }

        protected QtCommon(System.Type toolsetType, Opus.Core.Target target)
        {
            // TODO: not sure if this is valid any more?
            if (Opus.Core.OSUtilities.IsOSXHosting)
            {
                return;
            }

            // The target here will have a null IToolset because this module is a Thirdparty
            // module, which has no need for a tool, so no toolset is configured
            // However, we do need to know where Qt was installed, which is in the Toolset
            // so just grab the instance
            Opus.Core.IToolset toolset = Opus.Core.ToolsetFactory.GetInstance(toolsetType);
            string installPath = toolset.InstallPath((Opus.Core.BaseTarget)target);
            if (Opus.Core.OSUtilities.IsOSXHosting)
            {
                this.BinPath = installPath;
            }
            else
            {
                this.BinPath = System.IO.Path.Combine(installPath, "bin");
            }

            this.LibPath = System.IO.Path.Combine(installPath, "lib");
            this.includePaths.Add(System.IO.Path.Combine(installPath, "include"));

            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtCommon_IncludePaths);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtCommon_LibraryPaths);
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(QtCommon_VisualCWarningLevel);
        }

        [C.ExportLinkerOptionsDelegate]
        void QtCommon_LibraryPaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            if (Opus.Core.OSUtilities.IsOSXHosting)
            {
                return;
            }

            C.ILinkerOptions linkerOptions = module.Options as C.ILinkerOptions;
            linkerOptions.LibraryPaths.AddAbsoluteDirectory(this.LibPath, true);
        }

        [C.ExportCompilerOptionsDelegate]
        void QtCommon_IncludePaths(Opus.Core.IModule module, Opus.Core.Target target)
        {
            if (Opus.Core.OSUtilities.IsOSXHosting)
            {
                return;
            }

            C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
            foreach (string includePath in this.includePaths)
            {
                compilerOptions.IncludePaths.AddAbsoluteDirectory(includePath, true);
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void QtCommon_VisualCWarningLevel(Opus.Core.IModule module, Opus.Core.Target target)
        {
            VisualCCommon.ICCompilerOptions compilerOptions = module.Options as VisualCCommon.ICCompilerOptions;
            if (null != compilerOptions)
            {
                compilerOptions.WarningLevel = VisualCCommon.EWarningLevel.Level3;
            }
        }

        public override Opus.Core.StringArray Libraries(Opus.Core.Target target)
        {
            throw new System.NotImplementedException();
        }
    }
}