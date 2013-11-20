using System.Management.Automation;
using CodeOwls.EntityProvider.Attributes;

namespace CodeOwls.EntityProvider.Cmdlets
{
    public abstract class UnitOfWorkCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = false, Position = 0, ValueFromPipeline = true)]
        [EntityDriveFromPathArgumentTransform]
        public EntityDrive Context { get; set; }

        
        [Parameter(Mandatory = false)]
        public SwitchParameter Force { get; set; }

        
        protected bool AssertValidContext(string errorId, object target)
        {
            
            Context = Context ?? MetadataHelpers.GetEntityDriveFromPSPath(this.SessionState.Path, ".");
            if (null != Context)
            {
                return true;
            }

            var exception = new InvalidUnitOfWorkContextException(
                "The context (drive) for the unit of work could not be determined from the current location; please include a valid entity drive path in the -Context parameter"
                );
            var errorRecord = new ErrorRecord(exception,
                                              errorId + ".InvalidEntityContext",
                                              ErrorCategory.InvalidArgument,
                                              target);
            WriteError(errorRecord);
            return false;

        }

        protected bool VerifyShouldProcess(string action)
        {
            if ( !ShouldProcess("current unit of work", action))
            {
                return false;
            }
            if (!(Force || ShouldContinue(string.Empty, string.Empty)))
            {
                return false;
            }
            return true;
        }

        protected bool VerifyShouldProcess(string errorId, string action)
        {
            return VerifyShouldProcess(errorId, action, null);
        }

        protected bool VerifyShouldProcess(string errorId, string action, object target)
        {
            if (!(AssertValidContext(errorId, target) &&
                   VerifyShouldProcess(action)))
            {
                return false;
            }
            return true;
        }
    }
}