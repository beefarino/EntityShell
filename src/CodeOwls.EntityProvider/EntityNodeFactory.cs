using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Management.Automation;
using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using CodeOwls.PowerShell.Provider.PathNodes;
using CodeOwls.EntityProvider.Adapters;

namespace CodeOwls.EntityProvider
{
    internal class EntityNodeFactory<T> : NodeFactoryBase, IRemoveItem, ISetItem
        where T:class, new()
    {
        private EntityDrive _drive;
        private T _entity;
        private readonly DbSet<T> _dbSet;
        static Dictionary<Type, string> KeyMap = new Dictionary<Type, string>();
        
        public EntityNodeFactory( EntityDrive drive, DbSet<T> dbSet, T entity )
        {
            _drive = drive;
            _dbSet = dbSet;
            _entity = entity;
            
            if (! KeyMap.ContainsKey(typeof (T)))
            {
                var entityType = drive.GetEntities().FirstOrDefault(e => e.Name == typeof (T).Name);
                if (null == entityType)
                {
                    throw new InvalidOperationException( "Unable to locate the specified entity type in the context metadata");
                }

                var name = entityType.KeyMembers[0].Name;
                KeyMap.Add( typeof(T), name );
            }

        }        

        public override IPathNode GetNodeValue()
        {           
            var o = PSObject.AsPSObject(_entity);
            o.Properties.Add(
                new PSNoteProperty("EntityDrive", _drive)
                );
            o.Properties.Add(new PSScriptProperty("EntityState", ScriptBlock.Create("$this.EntityDrive.GetEntityState($this)")));

            return new LeafPathNode( o, Name );
        }

        public override string Name
        {
            get { return GetName(); }
        }

        string GetName()
        {
            var type = typeof (T);
            var propertyName = KeyMap[type];
            return type.GetProperty(propertyName).GetValue(_entity).ToString();
        }

        public object RemoveItemParameters { get { return null; } }
        public void RemoveItem(IContext context, string path, bool recurse)
        {
            _dbSet.Remove(_entity);
        }

        private static object _entitySetItemParameters;
        public object SetItemParameters
        {
            get
            {
                if (null == _entitySetItemParameters)
                {
                    var adapter = new EntityRuntimeParameterAdapter();
                    var entityMetadata = _drive.GetEntityMetadata<T>();
                    _entitySetItemParameters = adapter.AsSetItemRuntimeParameter(new T(), entityMetadata);
                }

                return _entitySetItemParameters;
            }
        }
        public IPathNode SetItem(IContext context, string path, object value)
        {
            var adapter = context.DynamicParameters as IEntityAdapter<T>;
            adapter.CopyTo( _entity );

            return new LeafPathNode( _entity, Name );
        }

    }
}