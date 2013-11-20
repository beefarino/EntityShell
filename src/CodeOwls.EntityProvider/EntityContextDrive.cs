using System;
using System.Data.Entity;
using System.Management.Automation;
using CodeOwls.EntityProvider.Attributes;

namespace CodeOwls.EntityProvider
{
    public abstract class EntityContextDrive<T> : EntityDrive where T : DbContext, new()
    {
        protected EntityContextDrive(PSDriveInfo driveInfo) : base(driveInfo)
        {
        }
        
        protected override DbContext CreateUnitOfWork()
        {
            return new T();
        }
    }

    public class GenericEntityContextDrive : EntityDrive
    {
        public class NewParams
        {
            [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
            [StringToTypeNameArgumentTransform]
            public Type ContextType { get; set; }
        }

        private readonly Type _dbContextType;
        public GenericEntityContextDrive( Type dbContextType, PSDriveInfo driveInfo ):base(driveInfo)
        {
            _dbContextType = dbContextType;
        }

        internal Type UnitOfWorkType { get { return _dbContextType; } }

        protected override DbContext CreateUnitOfWork()
        {
            var context = Activator.CreateInstance(_dbContextType);
            return context as DbContext;
        }
    }
}