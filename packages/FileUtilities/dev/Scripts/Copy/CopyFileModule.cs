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

        private System.Type SourceModuleType
        {
            get;
            set;
        }

        public CopyFile()
        {
            this.SourceFile = new Opus.Core.File();
        }

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

        public void Set(System.Type moduleType, object outputFileEnum)
        {
            this.SourceModuleType = moduleType;
            this.UpdateOptions += delegate(Opus.Core.IModule module, Opus.Core.Target target) {
                var options = module.Options as ICopyFileOptions;
                options.SourceModuleType = moduleType;
                options.SourceModuleOutputEnum = outputFileEnum as System.Enum;
            };
        }

        private void GetBesideModule(out BesideModuleAttribute attribute, out System.Type dependentModule)
        {
            attribute = null;
            dependentModule = null;

            System.Reflection.BindingFlags bindingFlags =
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance;
            System.Reflection.FieldInfo[] fields = this.GetType().GetFields(bindingFlags);
            foreach (System.Reflection.FieldInfo field in fields)
            {
                var attributes = field.GetCustomAttributes(typeof(BesideModuleAttribute), false);
                if (1 == attributes.Length)
                {
                    if (null != attribute)
                    {
                        throw new Opus.Core.Exception("Cannot set more than one BesideModule");
                    }

                    attribute = attributes[0] as BesideModuleAttribute;
                    var value = field.GetValue(this);
                    if (value is System.Type)
                    {
                        dependentModule = field.GetValue(this) as System.Type;
                    }
                    else
                    {
                        throw new Opus.Core.Exception("Expected BesideModule field '{0}' to be of type System.Type", field.Name);
                    }
                }
            }
        }

        private Opus.Core.TypeArray GetDestinationDependents(Opus.Core.Target target)
        {
            BesideModuleAttribute besideModule;
            System.Type dependentModule;
            this.GetBesideModule(out besideModule, out dependentModule);
            if (null == besideModule)
            {
                return null;
            }

            if (!Opus.Core.TargetUtilities.MatchFilters(target, besideModule))
            {
                return null;
            }

            this.UpdateOptions += delegate(Opus.Core.IModule module, Opus.Core.Target delegateTarget) {
                var options = module.Options as ICopyFileOptions;
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

            if (null != this.SourceModuleType)
            {
                deps.Add(this.SourceModuleType);
            }

            return deps;
        }

        #endregion
    }
}
