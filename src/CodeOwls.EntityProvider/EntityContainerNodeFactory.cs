using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Management.Automation;
using CodeOwls.EntityProvider.Attributes;
using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using CodeOwls.PowerShell.Provider.PathNodes;
using CodeOwls.EntityProvider.Adapters;

namespace CodeOwls.EntityProvider
{
    internal class EntityContainerNodeFactory<T> : NodeFactoryBase,
        INewItem
        where T:class, new()
    {
        private readonly EntityDrive _drive;
        private readonly DbSet<T> _dbset;

        public EntityContainerNodeFactory( EntityDrive drive, DbSet<T> dbset )
        {
            _drive = drive;
            _dbset = dbset;
        }

        public override object GetNodeChildrenParameters
        {
            get
            {
                return new GetNodeChildrenParameters();
            }
        }

        public override IEnumerable<INodeFactory> GetNodeChildren(IContext context)
        {
            var factories = new List<INodeFactory>();
            var p = context.DynamicParameters as GetNodeChildrenParameters ?? new GetNodeChildrenParameters();
            IEnumerable<T> @set = null;
            
            if (p.Local.ToBool())
            {
                @set = _dbset.Local;
            }
            else if (null != context.Filter)
            {
                @set = _drive.CurrentObjectContext.CreateObjectSet<T>().Where(context.Filter);
            }
            else
            {
                @set = _dbset;
            }

            @set.ToList().ConvertAll( e=> (INodeFactory)new EntityNodeFactory<T>(_drive, _dbset, e)).ForEach( factories.Add );

            return factories;
        }

        public override IPathNode GetNodeValue()
        {
            return new ContainerPathNode( this, Name );
        }

        public override string Name
        {
            get { return _drive.GetTableName<T>(); }
        }

        public IEnumerable<string> NewItemTypeNames { get; private set; }
        public object NewItemParameters { get { return EntityNewItemParameters;  } }
        public IPathNode NewItem(IContext context, string path, string itemTypeName, object newItemValue)
        {
            var adapter = context.DynamicParameters as IEntityAdapter<T>;
            
            var newEntity = adapter.ToNewEntity( _drive.CurrentUnitOfWork );
            var newItem = _dbset.Add(newEntity);

            return new LeafPathNode( newItem, string.Empty);
        }

        private static object _entityNewItemParameters;
        object EntityNewItemParameters
        {
            get
            {
                if (null == _entityNewItemParameters)
                {
                    var pocoTypes =
                        _drive.GetPOCOs().ToList().ConvertAll(MetadataHelpers.FindQualifiedTypeForEntity);
                    var adapter = new EntityRuntimeParameterAdapter(pocoTypes);
                    var entityMetadata = _drive.GetEntityMetadata<T>();
                    
                    _entityNewItemParameters = adapter.AsNewItemRuntimeParameter(new T(), entityMetadata);
                }

                return _entityNewItemParameters;
            }
        }
    }

    public class GetNodeChildrenParameters
    {
        [Parameter()]
        public SwitchParameter Local { get; set; }

    }
}