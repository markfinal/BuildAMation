namespace Opus.Core
{
    public class StringOption : Option
    {
        public StringOption(string value)
        {
            this.Value = value;
        }

        public string Value
        {
            get;
            set;
        }

        public override object Clone()
        {
            StringOption clonedOption = new StringOption(this.Value.Clone() as string);

            // we can share private data
            clonedOption.PrivateData = this.PrivateData;

            return clonedOption;
        }
    }
}