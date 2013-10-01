// <copyright file="XCBuildConfigurationSection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed class XCBuildConfigurationSection : IWriteableNode, System.Collections.IEnumerable
    {
        public XCBuildConfigurationSection()
        {
            this.BuildConfigurations = new System.Collections.Generic.List<XCBuildConfiguration>();
        }

        public void Add(XCBuildConfiguration target)
        {
            lock (this.BuildConfigurations)
            {
                this.BuildConfigurations.Add(target);
            }
        }

        public XCBuildConfiguration Get(string name, string moduleName)
        {
            lock(this.BuildConfigurations)
            {
                foreach (var buildConfiguration in this.BuildConfigurations)
                {
                    if ((buildConfiguration.Name == name) && (buildConfiguration.ModuleName == moduleName))
                    {
                        return buildConfiguration;
                    }
                }

                var newBuildConfiguration = new XCBuildConfiguration(name, moduleName);
                this.Add(newBuildConfiguration);
                return newBuildConfiguration;
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
            if (this.BuildConfigurations.Count == 0)
            {
                return;
            }

            var orderedList = new System.Collections.Generic.List<XCBuildConfiguration>(this.BuildConfigurations);
            orderedList.Sort(
                delegate(XCBuildConfiguration p1, XCBuildConfiguration p2)
                {
                    return p1.UUID.CompareTo(p2.UUID);
                }
            );

            writer.WriteLine("");
            writer.WriteLine("/* Begin XCBuildConfiguration section */");
            foreach (var buildConfiguration in orderedList)
            {
                (buildConfiguration as IWriteableNode).Write(writer);
            }
            writer.WriteLine("/* End XCBuildConfiguration section */");
        }
        #endregion

        #region IEnumerable implementation

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return this.BuildConfigurations.GetEnumerator();
        }

        #endregion
    }
}
