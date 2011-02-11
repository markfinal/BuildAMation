// <copyright file="BaseTargetFilteredAttribute.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    [System.AttributeUsage(System.AttributeTargets.All)]
    public abstract class BaseTargetFilteredAttribute : System.Attribute, ITargetFilters
    {
        public BaseTargetFilteredAttribute()
        {
            this.Platform = EPlatform.All;
            this.Configuration = EConfiguration.All;
            this.Toolchains = new string[] { ".*" };
        }

        public EPlatform Platform
        {
            get;
            set;
        }

        public EConfiguration Configuration
        {
            get;
            set;
        }

        public string[] Toolchains
        {
            get;
            set;
        }

        public override string ToString()
        {
            string message = System.String.Format("Platform='{0}' Configuration='{1}' Toolchains='", this.Platform.ToString(), this.Configuration.ToString());
            foreach (string toolchain in this.Toolchains)
            {
                message += toolchain + " ";
            }
            message.TrimEnd();
            message += "'";
            return message;
        }
    }
}
