// <copyright file="BuilderFactory.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public static class BuilderFactory
    {
        public static IBuilder CreateBuilder(System.Type builderType)
        {
            if (!typeof(IBuilder).IsAssignableFrom(builderType))
            {
                throw new Exception(System.String.Format("Type '{0}' does not implement the interface {1}", builderType.ToString(), typeof(IBuilder).ToString()), false);
            }

            IBuilder builder = System.Activator.CreateInstance(builderType) as IBuilder;
            return builder;
        }
    }
}