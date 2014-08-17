// <copyright file="Configuration.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    public static class Configuration
    {
        public static EConfiguration
        FromString(
            string configurationName)
        {
            var configuration = EConfiguration.Invalid;
            if (0 == System.String.Compare(configurationName, "Debug", true))
            {
                configuration = EConfiguration.Debug;
            }
            else if (0 == System.String.Compare(configurationName, "Optimized", true))
            {
                configuration = EConfiguration.Optimized;
            }
            else if (0 == System.String.Compare(configurationName, "Profile", true))
            {
                configuration = EConfiguration.Profile;
            }
            else if (0 == System.String.Compare(configurationName, "Final", true))
            {
                configuration = EConfiguration.Final;
            }
            else
            {
                throw new Exception("Configuration name '{0}' not recognized", configurationName);
            }
            return configuration;
        }
    }
}
