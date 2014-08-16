// <copyright file="ToolsetFactory.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    public static class ToolsetFactory
    {
        private static System.Collections.Generic.Dictionary<System.Type, IToolset> map = new System.Collections.Generic.Dictionary<System.Type, IToolset>();

        public static IToolset
        GetInstance(
            System.Type type)
        {
            if (map.ContainsKey(type))
            {
                return map[type];
            }

            TypeUtilities.CheckTypeImplementsInterface(type, typeof(IToolset));
            TypeUtilities.CheckTypeIsNotAbstract(type);

            var created = System.Activator.CreateInstance(type) as IToolset;
            map[type] = created;
            return created;
        }
    }
}