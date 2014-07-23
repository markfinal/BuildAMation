// <copyright file="BaseTargetFilteredAttribute.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    [System.AttributeUsage(System.AttributeTargets.All)]
    public abstract class BaseTargetFilteredAttribute :
        System.Attribute,
        ITargetFilters
    {
        public
        BaseTargetFilteredAttribute()
        {
            this.Platform = EPlatform.All;
            this.Configuration = EConfiguration.All;
            this.ToolsetTypes = null;
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

        public System.Type[] ToolsetTypes
        {
            get;
            set;
        }

        public override string
        ToString()
        {
            string message = System.String.Format("Platform='{0}' Configuration='{1}' ToolsetTypes='", this.Platform.ToString(), this.Configuration.ToString());
            if (null == this.ToolsetTypes)
            {
                message += "none";
            }
            else
            {
                var toolsetTypes = new StringArray();
                foreach (var type in this.ToolsetTypes)
                {
                    toolsetTypes.Add(type.ToString());
                }
                message += toolsetTypes.ToString(' ');
            }
            message += "'";
            return message;
        }
    }
}
