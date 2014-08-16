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

        public static IBuilder
        CreateBuilder(
            System.Type builderType)
        {
            if (builders.ContainsKey(builderType))
            {
                return builders[builderType];
            }

            TypeUtilities.CheckTypeImplementsInterface(builderType, typeof(IBuilder));
            TypeUtilities.CheckTypeIsNotAbstract(builderType);

            var builder = System.Activator.CreateInstance(builderType) as IBuilder;
            builders[builderType] = builder;
            return builder;
        }
    }
}