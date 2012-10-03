namespace QtCommon
{
    [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple=true)]
    public sealed class RegisterToolchainAttribute : Opus.Core.RegisterToolchainAttribute
    {
        public RegisterToolchainAttribute()
        {
            string name = "QtCommon.MocTool";

            if (!typeof(IMocOptions).IsAssignableFrom(typeof(MocOptionCollection)))
            {
                throw new Opus.Core.Exception(System.String.Format("C Compiler option type '{0}' does not implement the interface {1}", typeof(MocOptionCollection).ToString(), typeof(IMocOptions).ToString()), false);
            }

            {
                System.Collections.Generic.Dictionary<System.Type, ToolAndOptions> map = new System.Collections.Generic.Dictionary<System.Type, ToolAndOptions>();
                // TODO: there is a 1-1 mapping from tool to tool? Is that ok? We could simplify that to just the optionset type
                map[typeof(MocTool)] = new ToolAndOptions(typeof(MocTool), typeof(MocOptionCollection));

                if (!Opus.Core.State.HasCategory("ToolchainTypeMap"))
                {
                    Opus.Core.State.AddCategory("ToolchainTypeMap");
                }
                Opus.Core.State.Add("ToolchainTypeMap", name, map);
            }
        }
    }
}
