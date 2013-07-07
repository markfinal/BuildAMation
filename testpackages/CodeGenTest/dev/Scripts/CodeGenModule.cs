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
                this.OutputPaths[OutputFileFlags.GeneratedSourceFile] = outputPath;
            }

            base.FinalizeOptions (node);
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Opus.Core.StringArray commandLineStringBuilder, Opus.Core.Target target, Opus.Core.StringArray excludedOptionNames)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineStringBuilder, target, excludedOptionNames);
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

        [Opus.Core.DependentModules(Platform = Opus.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Opus.Core.TypeArray vcDependents = new Opus.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));
    }

    /// <summary>
    /// Code generation of C++ source
    /// </summary>
    [Opus.Core.ModuleToolAssignment(typeof(ICodeGenTool))]
    public abstract class CodeGenModule : Opus.Core.BaseModule, Opus.Core.IInjectModules
    {
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