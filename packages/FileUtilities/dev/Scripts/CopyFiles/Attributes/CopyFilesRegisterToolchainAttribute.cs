namespace FileUtilities
{
    [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple=true)]
    public sealed class CopyFilesRegisterToolchainAttribute : Opus.Core.RegisterToolchainAttribute
    {
        public CopyFilesRegisterToolchainAttribute(string name)
        {
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
        }
    }
}
