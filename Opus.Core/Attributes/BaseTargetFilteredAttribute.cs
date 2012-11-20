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
            // NEW STYLE
#if true
            this.ToolsetTypes = null;
#else
            this.Toolchains = new string[] { ".*" };
#endif
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

        // NEW STYLE
#if true
        public System.Type[] ToolsetTypes
        {
            get;
            set;
        }
#else
        public string[] Toolchains
        {
            get;
            set;
        }
#endif

        public override string ToString()
        {
            // NEW STYLE
#if true
            string message = System.String.Format("Platform='{0}' Configuration='{1}' ToolsetTypes='", this.Platform.ToString(), this.Configuration.ToString());
            if (null == this.ToolsetTypes)
            {
                message += "none";
            }
            else
            {
                foreach (System.Type type in this.ToolsetTypes)
                {
                    message += type.ToString() + " ";
                }
            }
#else
            string message = System.String.Format("Platform='{0}' Configuration='{1}' Toolchains='", this.Platform.ToString(), this.Configuration.ToString());
            foreach (string toolchain in this.Toolchains)
            {
                message += toolchain + " ";
            }
#endif
            message.TrimEnd();
            message += "'";
            return message;
        }
    }
}
