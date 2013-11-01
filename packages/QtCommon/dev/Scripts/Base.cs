// <copyright file="Base.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    public abstract class Base : ThirdPartyModule
    {
        protected Base()
        {
            this.QtToolset = Opus.Core.State.Get("Toolset", "Qt") as Toolset;
        }

        private Toolset QtToolset
        {
            get;
            set;
        }

        protected bool IncludeModule
        {
            get;
            set;
        }

        protected void AddIncludePath(C.ICCompilerOptions options,
                                      Opus.Core.Target target,
                                      string moduleName,
                                      bool includeModuleName)
        {
            string includePath = this.QtToolset.GetIncludePath((Opus.Core.BaseTarget)target);
            if (!string.IsNullOrEmpty(includePath))
            {
                options.IncludePaths.Add(includePath);
                if (includeModuleName)
                {
                    includePath = System.IO.Path.Combine(includePath, moduleName);
                    options.IncludePaths.Add(includePath);
                }
            }
        }

        protected void AddLibraryPath(C.ILinkerOptions options,
                                      Opus.Core.Target target)
        {
            string libraryPath = this.QtToolset.GetLibraryPath((Opus.Core.BaseTarget)target);
            if (!string.IsNullOrEmpty(libraryPath))
            {
                options.LibraryPaths.Add(libraryPath);
            }
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
                    options.Libraries.Add(System.String.Format("{0}4.lib", moduleName));
                }
            }
            else if (target.HasPlatform(Opus.Core.EPlatform.Unix))
            {
                options.Libraries.Add(System.String.Format("-l{0}", moduleName));
            }
            else if (target.HasPlatform(Opus.Core.EPlatform.OSX))
            {
                var osxLinkerOptions = options as C.ILinkerOptionsOSX;
                osxLinkerOptions.Frameworks.Add(moduleName);
            }
            else
            {
                // TODO: framework
                throw new System.NotImplementedException();
            }
        }

        protected string GetModuleDynamicLibrary(Opus.Core.Target target,
                                                 string moduleName)
        {
            string binPath = (this.QtToolset as Opus.Core.IToolset).BinPath((Opus.Core.BaseTarget)target);
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
            else if (target.HasPlatform(Opus.Core.EPlatform.OSX))
            {
                dynamicLibraryName = moduleName;
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
