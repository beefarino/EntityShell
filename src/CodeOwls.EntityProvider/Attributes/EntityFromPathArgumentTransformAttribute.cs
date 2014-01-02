using System;
using System.Management.Automation;

namespace CodeOwls.EntityProvider.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EntityFromPathArgumentTransformAttribute : ArgumentTransformationAttribute
    {
        public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
        {
            if (null == inputData || typeof(string) != inputData.GetType())
            {
                return inputData;
            }

            var path = inputData.ToString();

            var entity = MetadataHelpers.GetEntityFromPSPath(engineIntrinsics, path);

            return entity ?? inputData;
        }
    }
}