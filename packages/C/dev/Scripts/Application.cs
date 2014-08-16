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
    [Bam.Core.ModuleToolAssignment(typeof(ILinkerTool))]
    public class Application :
        Bam.Core.BaseModule,
        Bam.Core.INestedDependents,
        Bam.Core.ICommonOptionCollection,
        Bam.Core.IPostActionModules
    {
        public static readonly Bam.Core.LocationKey OutputFile = new Bam.Core.LocationKey("ExecutableBinaryFile", Bam.Core.ScaffoldLocation.ETypeHint.File);
        public static readonly Bam.Core.LocationKey OutputDir = new Bam.Core.LocationKey("ExecutableBinaryDir", Bam.Core.ScaffoldLocation.ETypeHint.Directory);

        public static readonly Bam.Core.LocationKey MapFile = new Bam.Core.LocationKey("MapFile", Bam.Core.ScaffoldLocation.ETypeHint.File);
        public static readonly Bam.Core.LocationKey MapFileDir = new Bam.Core.LocationKey("MapFileDir", Bam.Core.ScaffoldLocation.ETypeHint.Directory);

        Bam.Core.ModuleCollection
        Bam.Core.INestedDependents.GetNestedDependents(
            Bam.Core.Target target)
        {
            var collection = new Bam.Core.ModuleCollection();

            var type = this.GetType();
            var fieldInfoArray = type.GetFields(System.Reflection.BindingFlags.NonPublic |
                                                System.Reflection.BindingFlags.Public |
                                                System.Reflection.BindingFlags.Instance);
            foreach (var fieldInfo in fieldInfoArray)
            {
                var attributes = fieldInfo.GetCustomAttributes(typeof(Bam.Core.SourceFilesAttribute), false);
                if (attributes.Length > 0)
                {
                    var targetFilters = attributes[0] as Bam.Core.ITargetFilters;
                    if (!Bam.Core.TargetUtilities.MatchFilters(target, targetFilters))
                    {
                        Bam.Core.Log.DebugMessage("Source file field '{0}' of module '{1}' with filters '{2}' does not match target '{3}'", fieldInfo.Name, type.ToString(), targetFilters.ToString(), target.ToString());
                        continue;
                    }

                    var module = fieldInfo.GetValue(this) as Bam.Core.IModule;
                    if (null == module)
                    {
                        throw new Bam.Core.Exception("Field '{0}', marked with Bam.Core.SourceFiles attribute, must be derived from type Core.IModule", fieldInfo.Name);
                    }
                    collection.Add(module);
                }
            }

            return collection;
        }

        [LocalCompilerOptionsDelegate]
        private static void
        ApplicationSetConsolePreprocessor(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            if (Bam.Core.OSUtilities.IsWindows(target))
            {
                var compilerOptions = module.Options as ICCompilerOptions;
                compilerOptions.Defines.Add("_CONSOLE");
            }
        }

        [LocalLinkerOptionsDelegate]
        private static void
        ApplicationSetConsoleSubSystem(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            if (Bam.Core.OSUtilities.IsWindows(target))
            {
                var linkerOptions = module.Options as ILinkerOptions;
                linkerOptions.SubSystem = C.ESubsystem.Console;
            }
        }

        Bam.Core.BaseOptionCollection Bam.Core.ICommonOptionCollection.CommonOptionCollection
        {
            get;
            set;
        }

        #region IPostActionModules Members

        Bam.Core.TypeArray Bam.Core.IPostActionModules.GetPostActionModuleTypes(Bam.Core.BaseTarget target)
        {
            // TODO: currently disabled - only really needs to be in versions earlier than VS2010
            // not sure if it's needed for mingw
#if true
            return null;
#else
            if (target.HasPlatform(Bam.Core.EPlatform.Windows))
            {
                var postActionModules = new Bam.Core.TypeArray(
                    typeof(Win32Manifest));
                return postActionModules;
            }
            return null;
#endif
        }

        #endregion
    }
}