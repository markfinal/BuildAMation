// <copyright file="UpdateOptionCollectionDelegateArray.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public class UpdateOptionCollectionDelegateArray : Array<UpdateOptionCollectionDelegate>
    {
        public UpdateOptionCollectionDelegateArray(params UpdateOptionCollectionDelegate[] delegates)
            : base(delegates)
        {
        }
    }
}