// <copyright file="IPublishProductModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace Publisher
{
    public interface IPublishOptions
    {
        /// <summary>
        /// On OSX, should the publish step create an application bundle
        /// </summary>
        /// <value><c>true</c> if OSX application bundle; otherwise, <c>false</c>.</value>
        // StateOnly
        bool OSXApplicationBundle
        {
            get;
            set;
        }
    }
}
