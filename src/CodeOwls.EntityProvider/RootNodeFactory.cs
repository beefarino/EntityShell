using System;
using System.Collections.Generic;
using System.Data.Metadata.Edm;
using System.Linq;
using System.Reflection;
using CodeOwls.PowerShell.Provider.PathNodes;

namespace CodeOwls.EntityProvider
{
    internal class RootNodeFactory : NodeFactoryBase
    {
        private readonly EntityDrive _drive;

        public RootNodeFactory(EntityDrive drive)
        {
            _drive = drive;            
        }

        public override IPathNode GetNodeValue()
        {
            return new ContainerPathNode(_drive, Name);
        }

        public override string Name
        {
            get { return String.Empty; }
        }

        public override IEnumerable<INodeFactory> GetNodeChildren(CodeOwls.PowerShell.Provider.PathNodeProcessors.IContext context)
        {
            var factories = new List<INodeFactory>();
            var entities = from entity in _drive.GetPOCOs() select entity;
            foreach (var entity in entities)
            {
                var factory = CreateNodeFactoryForEntity(entity);
                factories.Add(factory);
            }

            return factories;
        }

        private INodeFactory CreateNodeFactoryForEntity(EntityType entity)
        {
            var type = FindQualifiedTypeForEntity(entity);

            var factory = CreateClosedGenericNodeFactory(type);
            return factory;
        }

        private Type FindQualifiedTypeForEntity(EntityType entity)
        {
            var qname = QualifyEntityTypeName(entity);
            var type = FindType(qname);
            return type;
        }

        readonly IDictionary<string,string> _assemblyNames = new Dictionary<string, string>(); 
        private string QualifyEntityTypeName(EntityType entity)
        {
            var assemblyName = GetAssemblyNameFromEntity(entity);

            return entity.FullName +", " + assemblyName;
        }

        private string GetAssemblyNameFromEntity(EntityType entity)
        {
            var name = entity.FullName;
            string assemblyName = null;

            if (! _assemblyNames.TryGetValue(name, out assemblyName))
            {
                assemblyName = LoadEntityTypeAssemblyName(entity);
                _assemblyNames[name] = assemblyName;
            }
            return assemblyName;
        }

        private string LoadEntityTypeAssemblyName(EntityType entity)
        {
            var pi = typeof (EntityType).GetProperty("ClrType", BindingFlags.Instance | BindingFlags.NonPublic);
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

        private Type FindType(string fullName)
        {
            return Type.GetType(fullName);
        }


        INodeFactory CreateClosedGenericNodeFactory(Type genericParameterType)
        {
            var context = _drive.CurrentUnitOfWork;
            Type classType = typeof (EntityContainerNodeFactory<>).MakeGenericType( genericParameterType );
            MethodInfo methodInfo = context.GetType()
                                          .GetMethods(BindingFlags.Public | BindingFlags.Instance )
                                          .FirstOrDefault(m => m.Name == "Set" && m.IsGenericMethodDefinition);
            if (null == methodInfo)
            {
                throw new InvalidOperationException( "the entity framework context does not support a generic Set method as expected" );
            }
            
            methodInfo = methodInfo.MakeGenericMethod(genericParameterType);
            var set = methodInfo.Invoke(context, null);
            var factory = Activator.CreateInstance(classType, _drive, set);
            return factory as INodeFactory;
        }
    }
}