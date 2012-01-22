// <copyright file="ToolChainOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public abstract class ToolchainOptionCollection : Opus.Core.BaseOptionCollection, CommandLineProcessor.ICommandLineSupport
    {
        protected virtual void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            IToolchainOptions options = this as IToolchainOptions;
            options.CharacterSet = ECharacterSet.NotSet;
            options.IsCPlusPlus = false;
        }

        public ToolchainOptionCollection()
            : base()
        {
        }

        public ToolchainOptionCollection(Opus.Core.DependencyNode node)
        {
            this.InitializeDefaults(node);
            this.SetDelegates(node);
        }

        // TODO: does this need updating for the new event and delegate model?
        private static ToolchainOptionCollection FromNode(Opus.Core.DependencyNode node)
        {
            Opus.Core.Target target = node.Target;

            System.Type toolchainOptionsType = Opus.Core.State.Get(target.Toolchain, ClassNames.ToolchainOptions) as System.Type;

            // Cannot use the OptionUtilities here, as there is no Node to assign these options to
            System.Reflection.MethodInfo method = typeof(Opus.Core.OptionCollectionFactory).GetMethod("CreateOptionCollection", new System.Type[] { typeof(Opus.Core.DependencyNode) });
            System.Reflection.MethodInfo genericMethod = method.MakeGenericMethod(new System.Type[] { toolchainOptionsType });
            ToolchainOptionCollection instance = genericMethod.Invoke(null, new object[] { node }) as ToolchainOptionCollection;
            return instance;
        }

        public static ToolchainOptionCollection GetSharedFromNode(Opus.Core.DependencyNode node)
        {
            ToolchainOptionCollection sharedOptionCollection;

            if (null == node.Parent || !node.IsModuleNested)
            {
                sharedOptionCollection = ToolchainOptionCollection.FromNode(node);
            }
            else
            {
                Opus.Core.DependencyNode parentNode = node.Parent;
                while (null == parentNode.Module.Options)
                {
                    if (null == parentNode.Parent)
                    {
                        throw new Opus.Core.Exception(System.String.Format("Unable to locate '{0}' parent's OptionCollection (source module was '{1}')", parentNode.ModuleName, node.ModuleName));
                    }
                    parentNode = parentNode.Parent;
                }
                System.Type optionCollectionType = parentNode.Module.Options.GetType();
                System.Reflection.PropertyInfo toolchainOptionCollectionProperty = optionCollectionType.GetProperty("ToolchainOptionCollection");
                if (null == toolchainOptionCollectionProperty)
                {
                    throw new Opus.Core.Exception(System.String.Format("OptionCollection '{0}' has no property called 'ToolchainOptionCollection'", optionCollectionType.FullName), false);
                }
                if (!toolchainOptionCollectionProperty.CanRead)
                {
                    throw new Opus.Core.Exception(System.String.Format("OptionCollection '{0}' has no get method in the 'ToolchainOptionCollection' property", optionCollectionType.FullName), false);
                }
                System.Reflection.MethodInfo toolchainOptionCollectionPropertyGet = toolchainOptionCollectionProperty.GetGetMethod();

                sharedOptionCollection = toolchainOptionCollectionPropertyGet.Invoke(parentNode.Module.Options, null) as ToolchainOptionCollection;
            }

            return sharedOptionCollection;
        }

        public ICCompilerOptions CCompilerOptionsInterface
        {
            get;
            set;
        }

        public IArchiverOptions ArchiverOptionsInterface
        {
            get;
            set;
        }

        public ILinkerOptions LinkerOptionsInterface
        {
            get;
            set;
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Opus.Core.StringArray commandLineBuilder, Opus.Core.Target target)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineBuilder, target);
        }

        public abstract Opus.Core.DirectoryCollection DirectoriesToCreate();
    }
}