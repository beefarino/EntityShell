using System;
using System.Collections.Generic;
using System.Data.Metadata.Edm;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
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
        static public Type FindQualifiedTypeForEntity(EntityType entity)
        {
            var qname = QualifyEntityTypeName(entity);
            var type = FindType(qname);
            return type;
        }

        static readonly IDictionary<string, string> _assemblyNames = new Dictionary<string, string>();
        static private string QualifyEntityTypeName(EntityType entity)
        {
            var assemblyName = GetAssemblyNameFromEntity(entity);

            return entity.FullName + ", " + assemblyName;
        }

        static private string GetAssemblyNameFromEntity(EntityType entity)
        {
            var name = entity.FullName;
            string assemblyName = null;

            if (!_assemblyNames.TryGetValue(name, out assemblyName))
            {
                assemblyName = LoadEntityTypeAssemblyName(entity);
                _assemblyNames[name] = assemblyName;
            }
            return assemblyName;
        }

        static private string LoadEntityTypeAssemblyName(EntityType entity)
        {
            var pi = typeof(EntityType).GetProperty("ClrType", BindingFlags.Instance | BindingFlags.NonPublic);
            if (null == pi)
            {
                throw new InvalidOperationException(
                    "the entity metadata does not support an internal ClrType property as expected");
            }

            var type = pi.GetValue(entity) as Type;
            if (null == type)
            {
                throw new InvalidOperationException("failed to obtain a valid entity type reference from the specified entity metadata");
            }

            var assemblyName = new AssemblyName(type.Assembly.FullName).Name;

            return assemblyName;
        }

        static private Type FindType(string fullName)
        {
            return Type.GetType(fullName);
        }

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
