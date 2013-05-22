using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using CodeOwls.PowerShell.Provider;

namespace CodeOwls.EntityProvider
{
    public abstract class EntityDrive : Drive 
    {
        protected EntityDrive( PSDriveInfo driveInfo) : base(driveInfo)
        {
        }

        private ObjectContext _objectContext;
        private DbContext _currentUnitOfWork;

        public bool HasUncompletedUnitOfWork { get { return null != _currentUnitOfWork; } }

        internal ObjectContext CurrentObjectContext
        {
            get
            {
                if (null == _objectContext)
                {
                    _objectContext = ((IObjectContextAdapter)CurrentUnitOfWork).ObjectContext;
                }
                return _objectContext;
            }
        }

        internal MetadataWorkspace CurrentMetadataWorkspace
        {
            get { return CurrentObjectContext.MetadataWorkspace; }
        }

        protected abstract DbContext CreateUnitOfWork();
        
        internal bool HasCurrentUnitOfWork
        {
            get { return null != _currentUnitOfWork; }
        }

        public EntityState GetEntityState(PSObject entity)
        {
            return GetEntityState(entity.BaseObject);
        }

        public EntityState GetEntityState(object entity)
        {
            return CurrentUnitOfWork.Entry(entity).State;
        }

        internal DbContext CurrentUnitOfWork
        {
            get
            {
                if (null == _currentUnitOfWork)
                {
                    _currentUnitOfWork = CreateUnitOfWork();
                }
                return _currentUnitOfWork;
            }
            private set
            {
                if (value != _currentUnitOfWork && null != _currentUnitOfWork )
                {
                    _currentUnitOfWork.Dispose();
                }
                _currentUnitOfWork = value;
                _objectContext = null;
            }
        }

        internal IEnumerable<EntityType> GetEntities()
        {
            ItemCollection ic;
            CurrentMetadataWorkspace.TryGetItemCollection(DataSpace.CSpace, out ic);
            return ic.GetItems<EntityType>();
        }

        internal IEnumerable<EntityType> GetPOCOs()
        {
            ItemCollection ic;
            CurrentMetadataWorkspace.TryGetItemCollection(DataSpace.OSpace, out ic);
            return ic.GetItems<EntityType>();
        }

        internal IEnumerable<MetadataItem> GetEntityObjectMappings()
        {
            ItemCollection ic;
            CurrentMetadataWorkspace.TryGetItemCollection(DataSpace.OCSpace, out ic);
            return ic;
        }

        internal EntityType GetEntityMetadata<T>()
        {
            var entityType = GetEntityObjectMappings()
                .ToList().FirstOrDefault(a => Regex.IsMatch( a.ToString(), Regex.Escape(typeof (T).FullName) + @":.+"));
            if (null == entityType)
            {
                return null;
            }
            var entityTypeName = Regex.Replace(entityType.ToString(), ".+:", "");
            
            var md = from entity in GetEntities() 
                     where entity.FullName == entityTypeName 
                     select entity;
            return md.FirstOrDefault();            
        }

        internal string GetTableName<T>() where T : class
        {
            string sql = CurrentObjectContext.CreateObjectSet<T>().ToTraceString();
            Regex regex = new Regex("FROM (\\[.+\\]\\.)?\\[?(?<table>.*?)\\]? AS");
            Match match = regex.Match(sql);

            string table = match.Groups["table"].Value;
            return table;
        }

        internal void StartNewUnitOfWork()
        {
            CurrentUnitOfWork = CreateUnitOfWork();
        }

        internal void CommitCurrentUnitOfWork()
        {
            CurrentUnitOfWork.SaveChanges();
            CurrentUnitOfWork = null;
        }

        internal void UndoCurrentUnitOfWork()
        {
            CurrentUnitOfWork = null;
        }
    }
}