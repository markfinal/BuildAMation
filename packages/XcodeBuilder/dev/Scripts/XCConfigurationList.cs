// <copyright file="XCConfigurationList.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed class XCConfigurationList : XCodeNodeData, IWriteableNode
    {
        public XCConfigurationList(XCodeNodeData owner)
            : base(owner.Name)
        {
            this.Owner = owner;
            this.BuildConfigurations = new System.Collections.Generic.List<XCBuildConfiguration>();
        }

        public XCodeNodeData Owner
        {
            get;
            private set;
        }

        public void AddUnique(XCBuildConfiguration configuration)
        {
            lock (this.BuildConfigurations)
            {
                if (!this.BuildConfigurations.Contains(configuration))
                {
                    this.BuildConfigurations.Add(configuration);
                }
            }
        }

        private System.Collections.Generic.List<XCBuildConfiguration> BuildConfigurations
        {
            get;
            set;
        }

#region IWriteableNode implementation

        void IWriteableNode.Write (System.IO.TextWriter writer)
        {
            if (null == this.Owner)
            {
                throw new Opus.Core.Exception("Owner of this configuration list has not been set");
            }

            writer.WriteLine("\t\t{0} /* Build configuration list for {1} \"{2}\" */ = {{", this.UUID, this.Owner.GetType().Name, this.Owner.Name);
            writer.WriteLine("\t\t\tisa = XCConfigurationList;");
            writer.WriteLine("\t\t\tbuildConfigurations = (");
            foreach (var configuration in this.BuildConfigurations)
            {
                writer.WriteLine("\t\t\t\t{0} /* {1} */,", configuration.UUID, configuration.Name);
            }
            writer.WriteLine("\t\t\t);");
            writer.WriteLine("\t\t\tdefaultConfigurationIsVisible = 0;");
            writer.WriteLine("\t\t\tdefaultConfigurationName = {0};", this.BuildConfigurations[0].Name);
            writer.WriteLine("\t\t};");
        }

#endregion
    }
}
