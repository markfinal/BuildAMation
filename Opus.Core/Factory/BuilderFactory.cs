// <copyright file="BuilderFactory.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public static class BuilderFactory
    {
        private static System.Collections.Generic.Dictionary<System.Type, IBuilder> builders = new System.Collections.Generic.Dictionary<System.Type, IBuilder>();

        public static IBuilder CreateBuilder(System.Type builderType)
        {
            if (builders.ContainsKey(builderType))
            {
                return builders[builderType];
            }

            if (!typeof(IBuilder).IsAssignableFrom(builderType))
            {
                throw new Exception(System.String.Format("Type '{0}' does not implement the interface {1}", builderType.ToString(), typeof(IBuilder).ToString()), false);
            }

            if (builderType.IsAbstract)
            {
                throw new Exception(System.String.Format("Type '{0}' is abstract", builderType.ToString()), false);
            }

            IBuilder builder = System.Activator.CreateInstance(builderType) as IBuilder;
            builders[builderType] = builder;
            return builder;
        }
    }
}