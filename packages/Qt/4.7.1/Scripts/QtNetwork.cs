// <copyright file="QtNetwork.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Qt package</summary>
// <author>Mark Final</author>
namespace Qt
{
    public sealed class Network : QtCommon.Network
    {
        public Network()
            : base(typeof(Qt.Toolset), false)
        {
        }
    }
}
