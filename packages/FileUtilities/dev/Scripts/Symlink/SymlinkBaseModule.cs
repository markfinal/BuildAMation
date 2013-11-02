// <copyright file="SymlinkBaseModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    [Opus.Core.ModuleToolAssignment(typeof(ISymlinkTool))]
    public abstract class SymlinkBase : Opus.Core.BaseModule, Opus.Core.IIdentifyExternalDependencies
    {
        public Opus.Core.File SourceFile
        {
            get;
            private set;
        }

        public System.Type BesideModuleType
        {
            get;
            set;
        }

        private Opus.Core.TypeArray AdditionalDependentModules
        {
            get;
            set;
        }

        public SymlinkBase()
        {
            this.SourceFile = new Opus.Core.File();
            this.AdditionalDependentModules = new Opus.Core.TypeArray();
        }

        public void Set(System.Type moduleType, object outputFileEnum)
        {
            this.AdditionalDependentModules.Add(moduleType);
            this.UpdateOptions += delegate(Opus.Core.IModule module, Opus.Core.Target target) {
                var options = module.Options as ISymlinkOptions;
                options.SourceModuleType = moduleType;
                options.SourceModuleOutputEnum = outputFileEnum as System.Enum;
            };
        }

        private Opus.Core.TypeArray GetDestinationDependents(Opus.Core.Target target)
        {
            BesideModuleAttribute besideModule;
            System.Type dependentModule;
            Utilities.GetBesideModule(this, target, out besideModule, out dependentModule);
            if (null == besideModule)
            {
                return null;
            }

            if (null == this.BesideModuleType)
            {
                this.BesideModuleType = dependentModule;
            }
            else if (this.BesideModuleType != dependentModule)
            {
                throw new Opus.Core.Exception("Inconsistent beside module types");
            }

            this.UpdateOptions += delegate(Opus.Core.IModule module, Opus.Core.Target delegateTarget) {
                var options = module.Options as ISymlinkOptions;
                options.DestinationModuleType = dependentModule;
                options.DestinationModuleOutputEnum = besideModule.OutputFileFlag;
            };

            return new Opus.Core.TypeArray(dependentModule);
        }

        #region IIdentifyExternalDependencies implementation

        Opus.Core.TypeArray Opus.Core.IIdentifyExternalDependencies.IdentifyExternalDependencies(Opus.Core.Target target)
        {
            Opus.Core.TypeArray deps = new Opus.Core.TypeArray();

            Opus.Core.TypeArray destinationDeps = this.GetDestinationDependents(target);
            if (null != destinationDeps)
            {
                deps.AddRange(destinationDeps);
            }

            if (null != this.AdditionalDependentModules)
            {
                deps.AddRange(this.AdditionalDependentModules);
            }

            return deps;
        }

        #endregion
    }
}
