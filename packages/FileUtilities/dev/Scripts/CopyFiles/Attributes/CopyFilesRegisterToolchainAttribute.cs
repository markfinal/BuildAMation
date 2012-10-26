namespace FileUtilities
{
    [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple=true)]
    public sealed class CopyFilesRegisterToolchainAttribute : Opus.Core.RegisterToolchainAttribute
    {
        public CopyFilesRegisterToolchainAttribute(System.Type infoType)
        {
            if (!typeof(Opus.Core.IToolset).IsAssignableFrom(infoType))
            {
                throw new Opus.Core.Exception(System.String.Format("Toolset information type '{0}' does not implement the interface {1}", infoType.ToString(), typeof(Opus.Core.IToolset).ToString()), false);
            }

            string name = "FileUtilities.CopyFiles";

            // TODO: do something similar when we get an interface for the copyfiles tool
#if false
            if (!typeof(ISymLinkOptions).IsAssignableFrom(typeof(SymLinkOptionCollection)))
            {
                throw new Opus.Core.Exception(System.String.Format("C Compiler option type '{0}' does not implement the interface {1}", typeof(SymLinkOptionCollection).ToString(), typeof(ISymLinkOptions).ToString()), false);
            }
#endif

            {
                System.Collections.Generic.Dictionary<System.Type, ToolAndOptions> map = new System.Collections.Generic.Dictionary<System.Type, ToolAndOptions>();
                // TODO: there is a 1-1 mapping from tool to tool? Is that ok? We could simplify that to just the optionset type
                map[typeof(CopyFilesTool)] = new ToolAndOptions(typeof(CopyFilesTool), typeof(CopyFilesOptionCollection));

                if (!Opus.Core.State.HasCategory("ToolchainTypeMap"))
                {
                    Opus.Core.State.AddCategory("ToolchainTypeMap");
                }
                Opus.Core.State.Add("ToolchainTypeMap", name, map);
            }

            // TODO: we do this here because we know that this will be executed
            {
                if (!Opus.Core.State.HasCategory("Toolchains"))
                {
                    Opus.Core.State.AddCategory("Toolchains");
                }

                // NEW STYLE: mapping each type of tool to it's toolchain (this is the default)
                System.Collections.Generic.Dictionary<System.Type, string> map = null;
                if (Opus.Core.State.Has("Toolchains", "Map"))
                {
                    map = Opus.Core.State.Get("Toolchains", "Map") as System.Collections.Generic.Dictionary<System.Type, string>;
                }
                else
                {
                    map = new System.Collections.Generic.Dictionary<System.Type, string>();
                    Opus.Core.State.Add("Toolchains", "Map", map);
                }
                map[typeof(CopyFilesTool)] = name;
            }

            // define where toolset information can be located
            {
                if (!Opus.Core.State.HasCategory("Toolset"))
                {
                    Opus.Core.State.AddCategory("Toolset");
                }
                Opus.Core.State.Add("Toolset", name, Opus.Core.ToolsetFactory.CreateToolset(infoType));
            }
        }
    }
}
