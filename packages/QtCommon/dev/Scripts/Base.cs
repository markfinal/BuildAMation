// <copyright file="Base.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    public abstract class Base : ThirdPartyModule
    {
        protected System.Type ToolsetType
        {
            get;
            set;
        }

        protected bool IncludeModule
        {
            get;
            set;
        }

        protected void AddIncludePath(System.Type toolsetType,
                                      C.ICCompilerOptions options,
                                      Opus.Core.Target target,
                                      string moduleName,
                                      bool includeModuleName)
        {
            var toolset = Opus.Core.ToolsetFactory.GetInstance(toolsetType) as Toolset;
            string includePath = toolset.GetIncludePath((Opus.Core.BaseTarget)target);
            options.IncludePaths.AddAbsoluteDirectory(includePath, true);
            if (includeModuleName)
            {
                includePath = System.IO.Path.Combine(includePath, moduleName);
                options.IncludePaths.AddAbsoluteDirectory(includePath, true);
            }
        }

        protected void AddLibraryPath(System.Type toolsetType,
                                      C.ILinkerOptions options,
                                      Opus.Core.Target target)
        {
            var toolset = Opus.Core.ToolsetFactory.GetInstance (toolsetType) as Toolset;
            string libraryPath = toolset.GetLibraryPath((Opus.Core.BaseTarget)target);
            options.LibraryPaths.AddAbsoluteDirectory(libraryPath, true);
        }

        protected void AddModuleLibrary(C.ILinkerOptions options,
                                        Opus.Core.Target target,
                                        string moduleName)
        {
            if (target.HasPlatform(Opus.Core.EPlatform.Windows))
            {
                if (target.HasConfiguration(Opus.Core.EConfiguration.Debug))
                {
                    options.Libraries.Add(System.String.Format("{0}d4.lib", moduleName));
                }
                else
                {
                    options.Libraries.Add(System.String.Format("{0}.lib", moduleName));
                }
            }
            else if (target.HasPlatform(Opus.Core.EPlatform.Unix))
            {
                options.Libraries.Add(System.String.Format("-l{0}", moduleName));
            }
            else
            {
                // TODO: framework
                throw new System.NotImplementedException();
            }
        }

        protected string GetModuleDynamicLibrary(System.Type toolsetType,
                                                 Opus.Core.Target target,
                                                 string moduleName)
        {
            var toolset = Opus.Core.ToolsetFactory.GetInstance(toolsetType);
            string binPath = toolset.BinPath((Opus.Core.BaseTarget)target);
            string dynamicLibraryName = null;
            if (target.HasPlatform(Opus.Core.EPlatform.Windows))
            {
                if (target.HasConfiguration(Opus.Core.EConfiguration.Debug))
                {
                    dynamicLibraryName = System.String.Format("{0}d4.dll", moduleName);
                }
                else
                {
                    dynamicLibraryName = System.String.Format("{0}4.dll", moduleName);
                }
            }
            else if (target.HasPlatform(Opus.Core.EPlatform.Unix))
            {
                dynamicLibraryName = System.String.Format("{0}.so", moduleName);
            }
            else
            {
                // TODO: framework
                throw new System.NotImplementedException();
            }
            string dynamicLibraryPath = System.IO.Path.Combine(binPath, dynamicLibraryName);
            return dynamicLibraryPath;
        }
    }
}
