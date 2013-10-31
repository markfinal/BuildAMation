// <copyright file="CopyFileModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    [Opus.Core.ModuleToolAssignment(typeof(ICopyFileTool))]
    public class CopyFile : Opus.Core.BaseModule, Opus.Core.IIdentifyExternalDependencies
    {
        public Opus.Core.File SourceFile
        {
            get;
            private set;
        }

        public BesideModuleAttribute BesideModuleAttribute
        {
            get;
            private set;
        }

        public System.Type BesideModuleType
        {
            get;
            private set;
        }

        private Opus.Core.TypeArray AdditionalDependentModules
        {
            get;
            set;
        }

        public CopyFile()
        {
            this.SourceFile = new Opus.Core.File();
            this.AdditionalDependentModules = new Opus.Core.TypeArray();
        }

        public CopyFile(BesideModuleAttribute attribute, System.Type besideModuleType)
            : this()
        {
            this.BesideModuleAttribute = attribute;
            this.BesideModuleType = besideModuleType;
        }

#if true
        public void Include(Opus.Core.Location baseLocation, string pattern)
        {
            this.SourceFile.Include(baseLocation, pattern);
        }
#else
        public void SetRelativePath(object owner, params string[] pathSegments)
        {
            this.SourceFile.SetRelativePath(owner, pathSegments);
        }

        public void SetPackageRelativePath(Opus.Core.PackageInformation package, params string[] pathSegments)
        {
            this.SourceFile.SetPackageRelativePath(package, pathSegments);
        }

        public void SetAbsolutePath(string absolutePath)
        {
            this.SourceFile.SetAbsolutePath(absolutePath);
        }

        public void SetGuaranteedAbsolutePath(string absolutePath)
        {
            this.SourceFile.SetGuaranteedAbsolutePath(absolutePath);
        }
#endif

        public void Set(System.Type moduleType, object outputFileEnum)
        {
            this.AdditionalDependentModules.Add(moduleType);
            this.UpdateOptions += delegate(Opus.Core.IModule module, Opus.Core.Target target) {
                var options = module.Options as ICopyFileOptions;
                options.SourceModuleType = moduleType;
                options.SourceModuleOutputEnum = outputFileEnum as System.Enum;
            };
        }

        private Opus.Core.TypeArray GetDestinationDependents(Opus.Core.Target target)
        {
            if ((null != this.BesideModuleAttribute) && (null != this.BesideModuleType))
            {
                this.UpdateOptions += delegate(Opus.Core.IModule module, Opus.Core.Target delegateTarget)
                {
                    var options = module.Options as ICopyFileOptions;
                    options.DestinationModuleType = this.BesideModuleType;
                    options.DestinationModuleOutputEnum = this.BesideModuleAttribute.OutputFileFlag;
                    options.DestinationRelativePath = this.BesideModuleAttribute.RelativePath;
                };

                return new Opus.Core.TypeArray(this.BesideModuleType);
            }

            BesideModuleAttribute besideModule;
            System.Type dependentModuleType;
            Utilities.GetBesideModule(this, target, out besideModule, out dependentModuleType);
            if (null == besideModule)
            {
                return null;
            }

            if (null == this.BesideModuleType)
            {
                this.BesideModuleType = dependentModuleType;
            }
            else if (this.BesideModuleType != dependentModuleType)
            {
                throw new Opus.Core.Exception("Inconsistent beside module types");
            }

            this.UpdateOptions += delegate(Opus.Core.IModule module, Opus.Core.Target delegateTarget) {
                var options = module.Options as ICopyFileOptions;
                options.DestinationModuleType = dependentModuleType;
                options.DestinationModuleOutputEnum = besideModule.OutputFileFlag;
                options.DestinationRelativePath = besideModule.RelativePath;
            };

            return new Opus.Core.TypeArray(dependentModuleType);
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

        public override string ToString()
        {
            var description = new System.Text.StringBuilder();
            description.AppendFormat("{0} -> ", this.SourceFile.ToString());
            if (null != this.BesideModuleType)
            {
                description.AppendFormat("{0} {1}", this.BesideModuleType.ToString(), this.BesideModuleAttribute.OutputFileFlag);
            }
            else
            {
                var options = this.Options as ICopyFileOptions;
                description.Append(options.DestinationDirectory);
            }
            return description.ToString();
        }
    }
}
