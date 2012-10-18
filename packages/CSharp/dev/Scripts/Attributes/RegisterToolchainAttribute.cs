namespace CSharp
{
    [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple=true)]
    public sealed class RegisterToolchainAttribute : Opus.Core.RegisterToolchainAttribute
    {
        public RegisterToolchainAttribute(string name, System.Type infoType)
        {
            if (!typeof(Opus.Core.IToolsetInfo).IsAssignableFrom(infoType))
            {
                throw new Opus.Core.Exception(System.String.Format("Toolset information type '{0}' does not implement the interface {1}", infoType.ToString(), typeof(Opus.Core.IToolsetInfo).ToString()), false);
            }

            if (!typeof(IOptions).IsAssignableFrom(typeof(OptionCollection)))
            {
                throw new Opus.Core.Exception(System.String.Format("C Compiler option type '{0}' does not implement the interface {1}", typeof(OptionCollection).ToString(), typeof(IOptions).ToString()), false);
            }

            {
                System.Collections.Generic.Dictionary<System.Type, ToolAndOptions> map = new System.Collections.Generic.Dictionary<System.Type, ToolAndOptions>();
                // TODO: there is a 1-1 mapping from tool to tool? Is that ok? We could simplify that to just the optionset type
                map[typeof(Csc)] = new ToolAndOptions(typeof(Csc), typeof(OptionCollection));

                if (!Opus.Core.State.HasCategory("ToolchainTypeMap"))
                {
                    Opus.Core.State.AddCategory("ToolchainTypeMap");
                }
                Opus.Core.State.Add("ToolchainTypeMap", name, map);
            }

            // define where toolset information can be located
            {
                if (!Opus.Core.State.HasCategory("ToolsetInfo"))
                {
                    Opus.Core.State.AddCategory("ToolsetInfo");
                }
                Opus.Core.State.Add("ToolsetInfo", name, Opus.Core.ToolsetInfoFactory.CreateToolsetInfo(infoType));
            }
        }
    }
}
