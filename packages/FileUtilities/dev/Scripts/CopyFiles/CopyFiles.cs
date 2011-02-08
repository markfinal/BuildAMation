// <copyright file="CopyFiles.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ExportOptionsDelegateAttribute : System.Attribute
    {
    }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class LocalOptionsDelegateAttribute : System.Attribute
    {
    }

    [Opus.Core.AssignToolForModule(typeof(CopyFilesTool),
                                   typeof(ExportOptionsDelegateAttribute),
                                   typeof(LocalOptionsDelegateAttribute),
                                   typeof(CopyFilesOptionCollection))]
    // TODO: kind of need a different interface to nested dependents which allows
    // the system to inspect a module
    public class CopyFiles : Opus.Core.IModule, Opus.Core.IIdentifyExternalDependencies
    {
        public void ExecuteOptionUpdate(Opus.Core.Target target)
        {
            if (null != this.UpdateOptions)
            {
                this.UpdateOptions(this, target);
            }
        }

        public Opus.Core.BaseOptionCollection Options
        {
            get;
            set;
        }

        public event Opus.Core.UpdateOptionCollectionDelegate UpdateOptions;

        public Opus.Core.FileCollection SourceFiles
        {
            get;
            private set;
        }

        public Opus.Core.ModuleCollection SourceModules
        {
            get;
            private set;
        }

        public Opus.Core.IModule DestinationModule
        {
            get;
            private set;
        }

        public System.Enum SourceOutputFlags
        {
            get;
            private set;
        }

        public System.Enum DirectoryOutputFlags
        {
            get;
            private set;
        }

        public string DestinationDirectory
        {
            get;
            private set;
        }

        public Opus.Core.TypeArray IdentifyExternalDependencies(Opus.Core.Target target)
        {
            Opus.Core.TypeArray externalDependents = new Opus.Core.TypeArray();

            Opus.Core.Target incompleteTarget = new Opus.Core.Target(target.Platform, target.Configuration);

            Opus.Core.ModuleCollection sourceModules = new Opus.Core.ModuleCollection();
            Opus.Core.IModule destinationModule = null;

            System.Reflection.BindingFlags bindingFlags = System.Reflection.BindingFlags.NonPublic |
                                                          System.Reflection.BindingFlags.Public |
                                                          System.Reflection.BindingFlags.Instance;
            System.Reflection.FieldInfo[] fields = this.GetType().GetFields(bindingFlags);
            foreach (System.Reflection.FieldInfo field in fields)
            {
                var sourceModuleAttributes = field.GetCustomAttributes(typeof(SourceModulesAttribute), false);
                if (1 == sourceModuleAttributes.Length)
                {
                    SourceModulesAttribute sourceModuleAttribute = sourceModuleAttributes[0] as SourceModulesAttribute;

                    Opus.Core.TypeArray sourceModuleTypes = field.GetValue(this) as Opus.Core.TypeArray;
                    externalDependents.AddRange(sourceModuleTypes);
                    foreach (System.Type sourceModuleType in sourceModuleTypes)
                    {
                        Opus.Core.IModule sourceModule = Opus.Core.ModuleUtilities.GetModule(sourceModuleType, incompleteTarget);
                        if (null == sourceModule)
                        {
                            throw new Opus.Core.Exception(System.String.Format("Can't find source module of type '{0}' in module '{1}", sourceModuleType.FullName, this.GetType().FullName), false);
                        }

                        this.SourceOutputFlags = sourceModuleAttribute.OutputFlags;

                        sourceModules.Add(sourceModule);
                    }
                }

                var sourceFilesAttributes = field.GetCustomAttributes(typeof(Opus.Core.SourceFilesAttribute), false);
                if (1 == sourceFilesAttributes.Length)
                {
                    Opus.Core.SourceFilesAttribute sourceFilesAttribute = sourceFilesAttributes[0] as Opus.Core.SourceFilesAttribute;

                    Opus.Core.FileCollection sourceFileCollection = field.GetValue(this) as Opus.Core.FileCollection;
                    foreach (string file in sourceFileCollection)
                    {
                        if (!System.IO.File.Exists(file))
                        {
                            throw new Opus.Core.Exception(System.String.Format("Source file '{0}' for module '{1}' does not exist", file, this.GetType().FullName), false);
                        }
                    }

                    this.SourceFiles = sourceFileCollection;
                }

                var destinationModuleAttributes = field.GetCustomAttributes(typeof(DestinationModuleDirectoryAttribute), false);
                if (1 == destinationModuleAttributes.Length)
                {
                    DestinationModuleDirectoryAttribute destinationModuleAttribute = destinationModuleAttributes[0] as DestinationModuleDirectoryAttribute;

                    Opus.Core.TypeArray destinationModuleTypes = field.GetValue(this) as Opus.Core.TypeArray;
                    externalDependents.AddRange(destinationModuleTypes);
                    foreach (System.Type destinationModuleType in destinationModuleTypes)
                    {
                        if (null != destinationModule)
                        {
                            throw new Opus.Core.Exception(System.String.Format("Only one destination module may be provided for module '{0}'", this.GetType().FullName), false);
                        }

                        destinationModule = Opus.Core.ModuleUtilities.GetModule(destinationModuleType, incompleteTarget);
                        if (null == destinationModule)
                        {
                            throw new Opus.Core.Exception(System.String.Format("Can't find destination module of type '{0}' in module '{1}", destinationModuleType.FullName, this.GetType().FullName), false);
                        }

                        this.DirectoryOutputFlags = destinationModuleAttribute.OutputFlags;
                    }
                }

                var destinationDirectoryAttributes = field.GetCustomAttributes(typeof(DestinationDirectoryPathAttribute), false);
                if (1 == destinationDirectoryAttributes.Length)
                {
                    DestinationDirectoryPathAttribute destinationDirectoryAttribute = destinationDirectoryAttributes[0] as DestinationDirectoryPathAttribute;

                    Opus.Core.DirectoryCollection destinationDirectoryPaths = field.GetValue(this) as Opus.Core.DirectoryCollection;
                    this.DestinationDirectory = destinationDirectoryPaths[0].GetAbsolutePath();

                    this.UpdateOptions += delegate(Opus.Core.IModule dModule, Opus.Core.Target dTarget)
                    {
                        CopyFilesOptionCollection options = dModule.Options as CopyFilesOptionCollection;
                        options.DestinationDirectory = this.DestinationDirectory;
                    };
                }
            }

            this.SourceModules = sourceModules;
            this.DestinationModule = destinationModule;

            return externalDependents;
        }
    }
}