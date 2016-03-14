using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;

using System.Linq;
using System.Management.Automation;
using System.Reflection;

namespace CodeOwls.EntityProvider.Attributes
{
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

        public static object GetEntityFromPSPath(EngineIntrinsics engine, string s)
        {   
            var item = engine.InvokeProvider.Item.Get(s).FirstOrDefault();
            return item;
        }
    }
}