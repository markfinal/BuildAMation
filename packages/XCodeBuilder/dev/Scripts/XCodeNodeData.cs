// <copyright file="XCodeNodeData.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XCodeBuilder package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public abstract class XCodeNodeData
    {
        protected XCodeNodeData(string name)
        {
            this.Name = name;
            this.UUID = this.Generate96BitUUID();
        }

        public string Name
        {
            get;
            private set;
        }

        public string UUID
        {
            get;
            private set;
        }

        private string Generate96BitUUID()
        {
            var guid = System.Guid.NewGuid();
            var toString = guid.ToString("N").ToUpper(); // this is 32 characters long

            // cannot create a 24-char (96 bit) GUID, so just chop off the front and rear 4 characters
            // this ought to be random enough
            var toString24Chars = toString.Substring(4, 24);
            return toString24Chars;
        }
    }
}
