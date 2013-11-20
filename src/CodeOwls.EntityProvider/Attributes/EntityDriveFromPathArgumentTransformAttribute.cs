using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CodeOwls.EntityProvider.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EntityDriveFromPathArgumentTransformAttribute : ArgumentTransformationAttribute
    {
        public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
        {
            if (null == inputData)
            {
                inputData = ".";
            }

            var path = inputData.ToString();

            var drive = MetadataHelpers.GetEntityDriveFromPSPath(engineIntrinsics.SessionState.Path, path);

            return drive;
        }
    }
    static class MetadataHelpers
    {
        public static EntityDrive GetEntityDriveFromPSPath(PathIntrinsics pathIntrinsics, string path)
        {
            var paths = pathIntrinsics.GetResolvedPSPathFromPSPath(path);
            var drive = (from p in paths
                         let e = p.Drive as EntityDrive
                         where null != e
                         select e).Distinct().FirstOrDefault();
            return drive;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class StringToTypeNameArgumentTransformAttribute : ArgumentTransformationAttribute
    {
        public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
        {
            if (null == inputData )
            {
                return null;
            }

            var typeName = inputData.ToString();
            var re = new Regex(@"^\[[\w\.]+\]$");

            if (!re.IsMatch( typeName ))
            {
                return inputData;
            }

            var results = engineIntrinsics.InvokeCommand.InvokeScript(typeName);
            if (! results.Any())
            {
                return null;
            }

            var type = results.First().BaseObject as Type;
            if (null == type)
            {
                throw new ArgumentException( String.Format(
                    "The string specified '{0}' could not be interpreted as a valid type name.  Please ensure you have loaded the assembly containing the type into your PowerShell session prior to calling this command.",
                    inputData
                    )
                );
            }

            return type;
        }

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ValidateEntityStateAttribute : ValidateArgumentsAttribute
    {
        protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
        {

            var o = arguments as PSObject;
            if (null == o)
            {
                return;
            }
            var stateProperty = o.Properties["EntityState"];
            if (null != stateProperty && StringComparer.InvariantCultureIgnoreCase.Equals("detatched",(string) stateProperty.Value))
            {
                throw new ValidationMetadataException("The specified entity is in a detatched state and is unusable for the specified operation.");
            }
        }
    }
}
