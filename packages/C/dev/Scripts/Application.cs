// <copyright file="Application.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    /// <summary>
    /// C/C++ console application
    /// </summary>
    [Opus.Core.ModuleToolAssignment(typeof(ILinkerTool))]
    public class Application :
        Opus.Core.BaseModule,
        Opus.Core.INestedDependents,
        Opus.Core.ICommonOptionCollection,
        Opus.Core.IPostActionModules
    {
        public static readonly Opus.Core.LocationKey OutputFile = new Opus.Core.LocationKey("ExecutableBinaryFile", Opus.Core.ScaffoldLocation.ETypeHint.File);
        public static readonly Opus.Core.LocationKey OutputDir = new Opus.Core.LocationKey("ExecutableBinaryDir", Opus.Core.ScaffoldLocation.ETypeHint.Directory);

        public static readonly Opus.Core.LocationKey MapFile = new Opus.Core.LocationKey("MapFile", Opus.Core.ScaffoldLocation.ETypeHint.File);
        public static readonly Opus.Core.LocationKey MapFileDir = new Opus.Core.LocationKey("MapFileDir", Opus.Core.ScaffoldLocation.ETypeHint.Directory);

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

        [LocalCompilerOptionsDelegate]
        private static void ApplicationSetConsolePreprocessor(Opus.Core.IModule module, Opus.Core.Target target)
        {
            if (Opus.Core.OSUtilities.IsWindows(target))
            {
                var compilerOptions = module.Options as ICCompilerOptions;
                compilerOptions.Defines.Add("_CONSOLE");
            }
        }

        [LocalLinkerOptionsDelegate]
        private static void ApplicationSetConsoleSubSystem(Opus.Core.IModule module, Opus.Core.Target target)
        {
            if (Opus.Core.OSUtilities.IsWindows(target))
            {
                var linkerOptions = module.Options as ILinkerOptions;
                linkerOptions.SubSystem = C.ESubsystem.Console;
            }
        }

        Opus.Core.BaseOptionCollection Opus.Core.ICommonOptionCollection.CommonOptionCollection
        {
            get;
            set;
        }

        #region IPostActionModules Members

        Opus.Core.TypeArray Opus.Core.IPostActionModules.GetPostActionModuleTypes(Opus.Core.BaseTarget target)
        {
#if true
            return null;
#else
            if (target.HasPlatform(Opus.Core.EPlatform.Windows))
            {
                var postActionModules = new Opus.Core.TypeArray(
                    typeof(Win32Manifest));
                return postActionModules;
            }
            else if (target.HasPlatform(Opus.Core.EPlatform.Unix))
            {
                var postActionModules = new Opus.Core.TypeArray(
                    typeof(PosixSharedLibrarySymlinks));
                return postActionModules;
            }
            else
            {
                return null;
            }
#endif
        }

        #endregion
    }
}