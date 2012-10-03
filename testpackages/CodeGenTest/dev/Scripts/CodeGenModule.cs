namespace CodeGenTest
{
    public class ExportCodeGenOptionsDelegateAttribute : System.Attribute
    {
    }

    public class LocalCodeGenOptionsDelegateAttribute : System.Attribute
    {
    }

    public sealed class PrivateData : CommandLineProcessor.ICommandLineDelegate
    {
        public PrivateData(CommandLineProcessor.Delegate commandLineDelegate)
        {
            this.CommandLineDelegate = commandLineDelegate;
        }
    

        public CommandLineProcessor.Delegate CommandLineDelegate
        {
            get;
            set;
        }
    }

    public sealed partial class CodeGenOptions : Opus.Core.BaseOptionCollection, CommandLineProcessor.ICommandLineSupport, ICodeGenOptions
    {
        public CodeGenOptions(Opus.Core.DependencyNode node)
            : base()
        {
            this.SetDefaults(node);
            this.SetDelegates(node);
        }

        private void SetGeneratedFilePath()
        {
            if (this.Contains("OutputSourceDirectory") && this.Contains("OutputName"))
            {
                ICodeGenOptions options = this as ICodeGenOptions;
                string outputPath = System.IO.Path.Combine(options.OutputSourceDirectory, options.OutputName) + ".c";
                this.OutputPaths[OutputFileFlags.GeneratedSourceFile] = outputPath;
            }
        }

        private void SetDefaults(Opus.Core.DependencyNode node)
        {
            ICodeGenOptions options = this as ICodeGenOptions;
            options.OutputSourceDirectory = node.GetTargettedModuleBuildDirectory("src");
            options.OutputName = "function";
        }

        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            this["OutputSourceDirectory"].PrivateData = new PrivateData(OutputSourceDirectoryCommandLine);
            this["OutputName"].PrivateData = new PrivateData(OutputNameCommandLine);
        }

        private static void OutputSourceDirectorySetHandler(object sender, Opus.Core.Option option)
        {
            CodeGenOptions options = sender as CodeGenOptions;
            options.SetGeneratedFilePath();
        }

        private static void OutputSourceDirectoryCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            commandLineBuilder.Add(stringOption.Value);
        }

        private static void OutputNameSetHandler(object sender, Opus.Core.Option option)
        {
            CodeGenOptions options = sender as CodeGenOptions;
            options.SetGeneratedFilePath();
        }

        private static void OutputNameCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            commandLineBuilder.Add(stringOption.Value);
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Opus.Core.StringArray commandLineStringBuilder, Opus.Core.Target target)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineStringBuilder, target);
        }

        Opus.Core.DirectoryCollection CommandLineProcessor.ICommandLineSupport.DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection dirsToCreate = new Opus.Core.DirectoryCollection();

            ICodeGenOptions options = this as ICodeGenOptions;
            if (null != options.OutputSourceDirectory)
            {
                dirsToCreate.AddAbsoluteDirectory(options.OutputSourceDirectory, false);
            }

            return dirsToCreate;
        }
    }

    class CodeGeneratorTool : C.Application
    {
        public static string VersionString
        {
            get
            {
                return "dev";
            }
        }

        public CodeGeneratorTool()
        {
            this.source.SetRelativePath(this, "source", "codegentool", "main.c");
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(CodeGeneratorTool_UpdateOptions);
        }

        void CodeGeneratorTool_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ILinkerOptions options = module.Options as C.ILinkerOptions;
            options.DoNotAutoIncludeStandardLibraries = false;
        }

        [Opus.Core.SourceFiles]
        C.ObjectFile source = new C.ObjectFile();

        [Opus.Core.DependentModules(Platform = Opus.Core.EPlatform.Windows, Toolchains = new string[] { "visualc" })]
        Opus.Core.TypeArray vcDependents = new Opus.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));
    }

    /// <summary>
    /// Code generation of C++ source
    /// </summary>
    [Opus.Core.AssignToolForModule(typeof(CodeGenTool),
                                   typeof(ExportCodeGenOptionsDelegateAttribute),
                                   typeof(LocalCodeGenOptionsDelegateAttribute),
                                   typeof(CodeGenOptions))]
    [Opus.Core.ModuleToolAssignment(typeof(CodeGenTool))]
    public abstract class CodeGenModule : Opus.Core.IModule, Opus.Core.IInjectModules
    {
        void Opus.Core.IModule.ExecuteOptionUpdate(Opus.Core.Target target)
        {
            if (null != this.UpdateOptions)
            {
                this.UpdateOptions(this, target);
            }
        }

        Opus.Core.BaseOptionCollection Opus.Core.IModule.Options
        {
            get;
            set;
        }

        Opus.Core.DependencyNode Opus.Core.IModule.OwningNode
        {
            get;
            set;
        }

        public Opus.Core.ProxyModulePath ProxyPath
        {
            get;
            set;
        }

        public event Opus.Core.UpdateOptionCollectionDelegate UpdateOptions;

        [Opus.Core.RequiredModules]
        protected Opus.Core.TypeArray requiredModules = new Opus.Core.TypeArray(new System.Type[]{
            typeof(CodeGeneratorTool)
        });

        Opus.Core.ModuleCollection Opus.Core.IInjectModules.GetInjectedModules(Opus.Core.Target target)
        {
            Opus.Core.IModule module = this as Opus.Core.IModule;
            ICodeGenOptions options = module.Options as ICodeGenOptions;
            string outputPath = System.IO.Path.Combine(options.OutputSourceDirectory, options.OutputName) + ".c";
            C.ObjectFile injectedFile = new C.ObjectFile();
            injectedFile.SetGuaranteedAbsolutePath(outputPath);

            Opus.Core.ModuleCollection moduleCollection = new Opus.Core.ModuleCollection();
            moduleCollection.Add(injectedFile);

            return moduleCollection;
        }
    }
}