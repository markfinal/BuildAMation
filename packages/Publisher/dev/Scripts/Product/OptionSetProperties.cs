// Automatically generated file from OpusOptionCodeGenerator. DO NOT EDIT.
// Command line:
// -i=Interfaces/IPublishOptions.cs -n=Publisher -c=OptionSet -p -d -dd=../../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs -pv=PrivateData
namespace Publisher
{
    public partial class OptionSet
    {
        #region IPublishOptions Option properties
        bool IPublishOptions.OSXApplicationBundle
        {
            get
            {
                return this.GetValueTypeOption<bool>("OSXApplicationBundle", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("OSXApplicationBundle", value);
                this.ProcessNamedSetHandler("OSXApplicationBundleSetHandler", this["OSXApplicationBundle"]);
            }
        }
        #endregion
    }
}
