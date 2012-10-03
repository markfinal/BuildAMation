namespace FileUtilities
{
    [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple=true)]
    public sealed class SymLinkRegisterToolchainAttribute : Opus.Core.RegisterToolchainAttribute
    {
        public SymLinkRegisterToolchainAttribute(string name)
        {
            if (!typeof(ISymLinkOptions).IsAssignableFrom(typeof(SymLinkOptionCollection)))
            {
                throw new Opus.Core.Exception(System.String.Format("C Compiler option type '{0}' does not implement the interface {1}", typeof(SymLinkOptionCollection).ToString(), typeof(ISymLinkOptions).ToString()), false);
            }

            {
                System.Collections.Generic.Dictionary<System.Type, ToolAndOptions> map = new System.Collections.Generic.Dictionary<System.Type, ToolAndOptions>();
                // TODO: there is a 1-1 mapping from tool to tool? Is that ok? We could simplify that to just the optionset type
                map[typeof(SymLinkTool)] = new ToolAndOptions(typeof(SymLinkTool), typeof(SymLinkOptionCollection));

                if (!Opus.Core.State.HasCategory("ToolchainTypeMap"))
                {
                    Opus.Core.State.AddCategory("ToolchainTypeMap");
                }
                Opus.Core.State.Add("ToolchainTypeMap", name, map);
            }
        }
    }
}
