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
    public class CopyFiles : Opus.Core.IModule, Opus.Core.INestedDependents
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

        public Opus.Core.ModuleCollection SourceModules
        {
            get;
            private set;
        }

        public Opus.Core.ModuleCollection DestinationModules
        {
            get;
            private set;
        }

        public System.Enum SourceOutputFlags
        {
            get;
            private set;
        }

        public EDirectoryChoice DirectoryChoiceFlags
        {
            get;
            private set;
        }

        public Opus.Core.ModuleCollection GetNestedDependents(Opus.Core.Target target)
        {
            Opus.Core.Target incompleteTarget = new Opus.Core.Target(target.Platform, target.Configuration);

            Opus.Core.ModuleCollection sourceModules = new Opus.Core.ModuleCollection();
            Opus.Core.ModuleCollection destinationModules = new Opus.Core.ModuleCollection();

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
                    foreach (System.Type sourceModuleType in sourceModuleTypes)
                    {
                        Opus.Core.IModule sourceModule = Opus.Core.ModuleUtilities.GetModule(sourceModuleType, incompleteTarget);
                        if (null == sourceModule)
                        {
                            throw new Opus.Core.Exception("Can't find source module");
                        }

                        this.SourceOutputFlags = sourceModuleAttribute.OutputFlags;

                        sourceModules.Add(sourceModule);
                    }
                }

                var destinationModuleAttributes = field.GetCustomAttributes(typeof(DestinationDirectoryAttribute), false);
                if (1 == destinationModuleAttributes.Length)
                {
                    DestinationDirectoryAttribute destinationModuleAttribute = destinationModuleAttributes[0] as DestinationDirectoryAttribute;

                    Opus.Core.TypeArray destinationModuleTypes = field.GetValue(this) as Opus.Core.TypeArray;
                    foreach (System.Type destinationModuleType in destinationModuleTypes)
                    {
                        Opus.Core.IModule destinationModule = Opus.Core.ModuleUtilities.GetModule(destinationModuleType, incompleteTarget);
                        if (null == destinationModule)
                        {
                            throw new Opus.Core.Exception("Can't find destination module");
                        }

                        this.DirectoryChoiceFlags = destinationModuleAttribute.DirectoryChoice;

                        destinationModules.Add(destinationModule);
                    }
                }
            }

            Opus.Core.ModuleCollection moduleCollection = new Opus.Core.ModuleCollection();
            moduleCollection.AddRange(sourceModules);
            moduleCollection.AddRange(destinationModules);

            this.SourceModules = sourceModules;
            this.DestinationModules = destinationModules;

            return moduleCollection;
        }
    }
}