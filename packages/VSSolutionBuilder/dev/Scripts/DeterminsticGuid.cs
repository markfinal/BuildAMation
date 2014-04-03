// <copyright file="DeterminsticGuid.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public class DeterministicGuid
    {
        public DeterministicGuid(string input)
        {
            // ref: http://geekswithblogs.net/EltonStoneman/archive/2008/06/26/generating-deterministic-guids.aspx

            // use MD5 hash to get a 16-byte hash of the string
            System.Security.Cryptography.MD5CryptoServiceProvider provider = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] inputBytes = System.Text.Encoding.Default.GetBytes(input);
            byte[] hashBytes = provider.ComputeHash(inputBytes);

            // generate a guid from the hash
            System.Guid hashGuid = new System.Guid(hashBytes);

            this.Guid = hashGuid;
        }

        public System.Guid Guid
        {
            get;
            private set;
        }
    }
}