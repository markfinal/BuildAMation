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

    public sealed partial class CodeGenOptionCollection : Opus.Core.BaseOptionCollection, CommandLineProcessor.ICommandLineSupport, ICodeGenOptions
    {
        public CodeGenOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode owningNode)
        {
            var options = this as ICodeGenOptions;
            options.OutputSourceDirectory = owningNode.GetTargettedModuleBuildDirectory("src");
            options.OutputName = "function";
        }

        public override void FinalizeOptions (Opus.Core.DependencyNode node)
        {
            if (this.Contains("OutputSourceDirectory") && this.Contains("OutputName"))
            {
                var options = node.Module.Options as ICodeGenOptions;
                string outputPath = System.IO.Path.Combine(options.OutputSourceDirectory, options.OutputName) + ".c";
                this.OutputPaths[OutputFileFlags.GeneratedSourceFile] = new Opus.Core.StringArray(outputPath);
            }

            base.FinalizeOptions (node);
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
    [Opus.Core.ModuleToolAssignment(typeof(ICodeGenTool))]
    public abstract class CodeGenModule : Opus.Core.BaseModule, Opus.Core.IInjectModules
    {
        [Opus.Core.RequiredModules]
        protected Opus.Core.TypeArray requiredModules = new Opus.Core.TypeArray(typeof(CodeGeneratorTool));

        Opus.Core.ModuleCollection Opus.Core.IInjectModules.GetInjectedModules(Opus.Core.Target target)
        {
            Opus.Core.IModule module = this as Opus.Core.IModule;
            ICodeGenOptions options = module.Options as ICodeGenOptions;
            string outputPath = System.IO.Path.Combine(options.OutputSourceDirectory, options.OutputName) + ".c";
            C.ObjectFile injectedFile = new C.ObjectFile();
            injectedFile.SourceFile.SetGuaranteedAbsolutePath(outputPath);

            Opus.Core.ModuleCollection moduleCollection = new Opus.Core.ModuleCollection();
            moduleCollection.Add(injectedFile);

            return moduleCollection;
        }
    }
}