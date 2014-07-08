// <copyright file="StaticLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    /// <summary>
    /// C/C++ static library
    /// </summary>
    [Opus.Core.ModuleToolAssignment(typeof(IArchiverTool))]
    public class StaticLibrary : Opus.Core.BaseModule, Opus.Core.INestedDependents, Opus.Core.IForwardDependenciesOn, Opus.Core.ICommonOptionCollection
    {
        public static readonly Opus.Core.LocationKey OutputFileLocKey = new Opus.Core.LocationKey("StaticLibraryFile", Opus.Core.ScaffoldLocation.ETypeHint.File);
        public static readonly Opus.Core.LocationKey OutputDirLocKey = new Opus.Core.LocationKey("StaticLibraryOutputDirectory", Opus.Core.ScaffoldLocation.ETypeHint.Directory);

        private string PreprocessorDefine
        {
            get;
            set;
        }

        [ExportCompilerOptionsDelegate]
        protected void StaticLibrarySetPreprocessorDefine(Opus.Core.IModule module, Opus.Core.Target target)
        {
            if (null == this.PreprocessorDefine)
            {
                var packageName = this.OwningNode.Package.Name.ToUpper();
                var moduleName = this.OwningNode.ModuleName.ToUpper();
                var preprocessorName = new System.Text.StringBuilder();
                preprocessorName.AppendFormat("D_{0}_{1}_STATICAPI", packageName, moduleName);
                this.PreprocessorDefine = preprocessorName.ToString();
            }

            var compilerOptions = module.Options as ICCompilerOptions;
            compilerOptions.Defines.Add(this.PreprocessorDefine);
        }

        Opus.Core.ModuleCollection Opus.Core.INestedDependents.GetNestedDependents(Opus.Core.Target target)
        {
            var collection = new Opus.Core.ModuleCollection();

            var type = this.GetType();
            var fieldInfoArray = type.GetFields(System.Reflection.BindingFlags.NonPublic |
                                                System.Reflection.BindingFlags.Public |
                                                System.Reflection.BindingFlags.Instance);
            foreach (var fieldInfo in fieldInfoArray)
            {
                var attributes = fieldInfo.GetCustomAttributes(typeof(Opus.Core.SourceFilesAttribute), false);
                if (attributes.Length > 0)
                {
                    var targetFilters = attributes[0] as Opus.Core.ITargetFilters;
                    if (!Opus.Core.TargetUtilities.MatchFilters(target, targetFilters))
                    {
                        Opus.Core.Log.DebugMessage("Source file field '{0}' of module '{1}' with filters '{2}' does not match target '{3}'", fieldInfo.Name, type.ToString(), targetFilters.ToString(), target.ToString());
                        continue;
                    }

                    var module = fieldInfo.GetValue(this) as Opus.Core.IModule;
                    if (null == module)
                    {
                        throw new Opus.Core.Exception("Field '{0}', marked with Opus.Core.SourceFiles attribute, must be derived from type Core.IModule", fieldInfo.Name);
                    }
                    collection.Add(module);
                }
            }

            return collection;
        }

        Opus.Core.BaseOptionCollection Opus.Core.ICommonOptionCollection.CommonOptionCollection
        {
            get;
            set;
        }
    }
}