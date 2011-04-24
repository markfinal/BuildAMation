namespace CodeGenTest2
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

    public partial class CodeGenOptions : Opus.Core.BaseOptionCollection, CommandLineProcessor.ICommandLineSupport
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
                string outputPath = System.IO.Path.Combine(this.OutputSourceDirectory, this.OutputName) + ".c";
                this.OutputPaths[OutputFileFlags.GeneratedSourceFile] = outputPath;
            }
        }

        private void SetDefaults(Opus.Core.DependencyNode node)
        {
            this.OutputSourceDirectory = node.GetTargettedModuleBuildDirectory("src");
            this.OutputName = "function";
        }

        private void SetDelegates(Opus.Core.DependencyNode node)
        {
            this["OutputSourceDirectory"].PrivateData = new PrivateData(OutputSourceDirectoryCommandLine);
            this["OutputName"].PrivateData = new PrivateData(OutputNameCommandLine);
        }

        private static void OutputSourceDirectorySetHandler(object sender, Opus.Core.Option option)
        {
            CodeGenOptions options = sender as CodeGenOptions;
            options.SetGeneratedFilePath();
        }

        protected static void OutputSourceDirectoryCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            commandLineBuilder.Add(stringOption.Value);
        }

        private static void OutputNameSetHandler(object sender, Opus.Core.Option option)
        {
            CodeGenOptions options = sender as CodeGenOptions;
            options.SetGeneratedFilePath();
        }

        protected static void OutputNameCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
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

            if (null != this.OutputSourceDirectory)
            {
                dirsToCreate.Add(null, this.OutputSourceDirectory);
            }

            return dirsToCreate;
        }
    }

    class CodeGeneratorTool : CSharp.Executable
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
            this.source.SetRelativePath(this, "source", "codegentool", "main.cs");
        }

        [Opus.Core.SourceFiles]
        Opus.Core.File source = new Opus.Core.File();
    }

    /// <summary>
    /// Code generation of C++ source
    /// </summary>
    [Opus.Core.AssignToolForModule(typeof(CodeGenTool),
                                   typeof(ExportCodeGenOptionsDelegateAttribute),
                                   typeof(LocalCodeGenOptionsDelegateAttribute),
                                   typeof(CodeGenOptions))]
    public abstract class CodeGenModule : Opus.Core.IModule, Opus.Core.IInjectModules
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

        public Opus.Core.DependencyNode OwningNode
        {
            get;
            set;
        }

        public event Opus.Core.UpdateOptionCollectionDelegate UpdateOptions;

        [Opus.Core.RequiredModules]
        protected Opus.Core.TypeArray requiredModules = new Opus.Core.TypeArray(typeof(CodeGeneratorTool));

        public Opus.Core.ModuleCollection GetInjectedModules(Opus.Core.Target target)
        {
            CodeGenOptions options = this.Options as CodeGenOptions;
            string outputPath = System.IO.Path.Combine(options.OutputSourceDirectory, options.OutputName) + ".c";
            C.ObjectFile injectedFile = new C.ObjectFile();
            injectedFile.SourceFile.SetGuaranteedAbsolutePath(outputPath);

            Opus.Core.ModuleCollection moduleCollection = new Opus.Core.ModuleCollection();
            moduleCollection.Add(injectedFile);

            return moduleCollection;
        }
    }
}