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
            options.OutputSourceDirectory = owningNode.GetTargettedModuleBuildDirectoryLocation("src").GetSingleRawPath();
            options.OutputName = "function";

            if (!owningNode.Module.Locations[CodeGenModule.OutputDir].IsValid)
            {
                (owningNode.Module.Locations[CodeGenModule.OutputDir] as Opus.Core.ScaffoldLocation).SetReference(owningNode.GetTargettedModuleBuildDirectoryLocation("src"));
            }
        }

        public override void FinalizeOptions (Opus.Core.DependencyNode node)
        {
#if true
            if (!node.Module.Locations[CodeGenModule.OutputFile].IsValid)
            {
                var options = node.Module.Options as ICodeGenOptions;
                (node.Module.Locations[CodeGenModule.OutputFile] as Opus.Core.ScaffoldLocation).SpecifyStub(node.Module.Locations[CodeGenModule.OutputDir], options.OutputName + ".c", Opus.Core.Location.EExists.WillExist);
            }
#else
            if (this.Contains("OutputSourceDirectory") && this.Contains("OutputName"))
            {
                var options = node.Module.Options as ICodeGenOptions;
                string outputPath = System.IO.Path.Combine(options.OutputSourceDirectory, options.OutputName) + ".c";
                this.OutputPaths[OutputFileFlags.GeneratedSourceFile] = outputPath;
            }
#endif

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
        public static readonly Opus.Core.LocationKey OutputFile = new Opus.Core.LocationKey("GeneratedSource", Opus.Core.ScaffoldLocation.ETypeHint.File);
        public static readonly Opus.Core.LocationKey OutputDir = new Opus.Core.LocationKey("GeneratedSourceDir", Opus.Core.ScaffoldLocation.ETypeHint.Directory);

        [Opus.Core.RequiredModules]
        protected Opus.Core.TypeArray requiredModules = new Opus.Core.TypeArray(typeof(CodeGeneratorTool));

        #region IInjectModules Members

#if false
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
#endif

        string Opus.Core.IInjectModules.GetInjectedModuleNameSuffix(Opus.Core.BaseTarget baseTarget)
        {
            return "CodeGen2";
        }

        System.Type Opus.Core.IInjectModules.GetInjectedModuleType(Opus.Core.BaseTarget baseTarget)
        {
            return typeof(C.ObjectFile);
        }

        Opus.Core.DependencyNode Opus.Core.IInjectModules.GetInjectedParentNode(Opus.Core.DependencyNode node)
        {
            var dependentFor = node.ExternalDependentFor;
            var firstDependentFor = dependentFor[0];
            return firstDependentFor;
        }

        void Opus.Core.IInjectModules.ModuleCreationFixup(Opus.Core.DependencyNode node)
        {
            var dependent = node.ExternalDependents;
            var firstDependent = dependent[0];
            var dependentModule = firstDependent.Module;
            var module = node.Module as C.ObjectFile;
            var sourceFile = new Opus.Core.ScaffoldLocation(Opus.Core.ScaffoldLocation.ETypeHint.File);
            sourceFile.SetReference(dependentModule.Locations[CodeGenModule.OutputFile]);
            module.SourceFileLocation = sourceFile;
        }

        #endregion
    }
}