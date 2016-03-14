using System;
using System.Collections.Generic;
//using System.Data.Metadata.Edm;
using System.Linq;
using System.Reflection;
using CodeOwls.EntityProvider.Attributes;
using CodeOwls.PowerShell.Provider.PathNodes;
using System.Data.Entity.Core.Metadata.Edm;

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
            var type = MetadataHelpers.FindQualifiedTypeForEntity(entity);

            var factory = CreateClosedGenericNodeFactory(type);
            return factory;
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