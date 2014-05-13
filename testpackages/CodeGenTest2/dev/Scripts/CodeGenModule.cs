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

        protected override void SetDefaultOptionValues(Opus.Core.DependencyNode owningNode)
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
            var sourceDir = this.PackageLocation.SubDirectory("source");
            var codegentoolSourceDir = sourceDir.SubDirectory("codegentool");
            this.source = Opus.Core.FileLocation.Get(codegentoolSourceDir, "main.cs");
        }

        [Opus.Core.SourceFiles]
        Opus.Core.Location source;
    }

    /// <summary>
    /// Code generation of C++ source
    /// </summary>
    [Opus.Core.ModuleToolAssignment(typeof(ICodeGenTool))]
    public abstract class CodeGenModule : Opus.Core.BaseModule, Opus.Core.IInjectModules
    {
        public static readonly Opus.Core.LocationKey GeneratedSourceFileLocationKey = new Opus.Core.LocationKey("GeneratedSource");

        [Opus.Core.RequiredModules]
        protected Opus.Core.TypeArray requiredModules = new Opus.Core.TypeArray(typeof(CodeGeneratorTool));

        Opus.Core.ModuleCollection Opus.Core.IInjectModules.GetInjectedModules(Opus.Core.Target target)
        {
            var module = this as Opus.Core.IModule;
            var options = module.Options as ICodeGenOptions;
            var outputPath = System.IO.Path.Combine(options.OutputSourceDirectory, options.OutputName) + ".c";
            var injectedFile = new C.ObjectFile();
            injectedFile.SourceFileLocation = Opus.Core.FileLocation.Get(outputPath, Opus.Core.Location.EExists.WillExist);

            var moduleCollection = new Opus.Core.ModuleCollection();
            moduleCollection.Add(injectedFile);

            return moduleCollection;
        }
    }
}