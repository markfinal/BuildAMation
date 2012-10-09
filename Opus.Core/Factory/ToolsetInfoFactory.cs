// <copyright file="ToolsetInfoFactory.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public static class ToolsetInfoFactory
    {
        private static System.Collections.Generic.Dictionary<System.Type, IToolsetInfo> map = new System.Collections.Generic.Dictionary<System.Type, IToolsetInfo>();

        public static IToolsetInfo CreateToolsetInfo(System.Type type)
        {
            if (map.ContainsKey(type))
            {
                return map[type];
            }

            if (!typeof(IToolsetInfo).IsAssignableFrom(type))
            {
                throw new Exception(System.String.Format("Type '{0}' does not implement the interface {1}", type.ToString(), typeof(IToolsetInfo).ToString()), false);
            }

            IToolsetInfo created = System.Activator.CreateInstance(type) as IToolsetInfo;
            map[type] = created;
            return created;
        }
    }
}