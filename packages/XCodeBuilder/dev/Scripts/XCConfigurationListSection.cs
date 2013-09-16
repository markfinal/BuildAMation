// <copyright file="XCConfigurationListSection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed class XCConfigurationListSection : IWriteableNode, System.Collections.IEnumerable
    {
        public XCConfigurationListSection()
        {
            this.ConfigurationLists = new System.Collections.Generic.List<XCConfigurationList>();
        }

        public void Add(XCConfigurationList configurationList)
        {
            lock (this.ConfigurationLists)
            {
                this.ConfigurationLists.Add(configurationList);
            }
        }

        public XCConfigurationList Get(string name, XCodeNodeData owner)
        {
            lock (this.ConfigurationLists)
            {
                foreach (var configurationList in this.ConfigurationLists)
                {
                    if ((configurationList.Name == name) && (configurationList.Owner == owner))
                    {
                        return configurationList;
                    }
                }

                var newConfigurationList = new XCConfigurationList(name);
                this.ConfigurationLists.Add(newConfigurationList);
                return newConfigurationList;
            }
        }

        private System.Collections.Generic.List<XCConfigurationList> ConfigurationLists
        {
            get;
            set;
        }

        #region IWriteableNode implementation
        void IWriteableNode.Write (System.IO.TextWriter writer)
        {
            if (this.ConfigurationLists.Count == 0)
            {
                return;
            }

            writer.WriteLine("");
            writer.WriteLine("/* Begin XCConfigurationList section */");
            foreach (var configurationList in this.ConfigurationLists)
            {
                (configurationList as IWriteableNode).Write(writer);
            }
            writer.WriteLine("/* End XCConfigurationList section */");
        }
        #endregion

        #region IEnumerable implementation

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return this.ConfigurationLists.GetEnumerator();
        }

        #endregion
    }
}
