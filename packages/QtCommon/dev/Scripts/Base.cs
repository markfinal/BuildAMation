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

        protected void AddIncludePath(C.ICCompilerOptions options,
                                      Opus.Core.Target target,
                                      string moduleName)
        {
            var includePath = this.QtToolset.GetIncludePath((Opus.Core.BaseTarget)target);
            if (!string.IsNullOrEmpty(includePath))
            {
                options.IncludePaths.Add(includePath);
                if (this.QtToolset.IncludePathIncludesQtModuleName)
                {
                    includePath = System.IO.Path.Combine(includePath, moduleName);
                    options.IncludePaths.Add(includePath);
                }
            }
        }

        protected void AddLibraryPath(C.ILinkerOptions options,
                                      Opus.Core.Target target)
        {
            var libraryPath = this.QtToolset.GetLibraryPath((Opus.Core.BaseTarget)target);
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

        protected Opus.Core.Location
        GetModuleDynamicLibrary(
            Opus.Core.Target target,
            string moduleName)
        {
            string dynamicLibraryName = null;
            if (target.HasPlatform(Opus.Core.EPlatform.Windows))
            {
                var binPath = (this.QtToolset as Opus.Core.IToolset).BinPath((Opus.Core.BaseTarget)target);
                if (target.HasConfiguration(Opus.Core.EConfiguration.Debug))
                {
                    dynamicLibraryName = System.String.Format("{0}d4.dll", moduleName);
                }
                else
                {
                    dynamicLibraryName = System.String.Format("{0}4.dll", moduleName);
                }
                var dynamicLibraryPath = System.IO.Path.Combine(binPath, dynamicLibraryName);
                return Opus.Core.FileLocation.Get(dynamicLibraryPath);
            }
            else if (target.HasPlatform(Opus.Core.EPlatform.Unix))
            {
                var libPath = this.QtToolset.GetLibraryPath((Opus.Core.BaseTarget)target);
                dynamicLibraryName = System.String.Format("lib{0}.so", moduleName);
                var dynamicLibraryPath = System.IO.Path.Combine(libPath, dynamicLibraryName);
                return Opus.Core.FileLocation.Get(dynamicLibraryPath);
            }
            else if (target.HasPlatform(Opus.Core.EPlatform.OSX))
            {
                // TODO: this needs some rework with publishing, as it ought to be a framework
                #if false
                return Opus.Core.FileLocation.Get(moduleName);
                #endif
                return null;
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }

#if OPUSPACKAGE_PUBLISHER_DEV
        [Publisher.PublishModuleDependency]
        protected Opus.Core.Array<Opus.Core.LocationKey> publishKeys = new Opus.Core.Array<Opus.Core.LocationKey>(
            C.DynamicLibrary.OutputFile);
#endif
    }
}
