using System.Data.Entity;
using System.Management.Automation;
using CodeOwls.EntityProvider.Attributes;

namespace CodeOwls.EntityProvider.Cmdlets
{
    [Cmdlet(VerbsCommon.Undo, Nouns.UnitOfWork, ConfirmImpact = ConfirmImpact.High)]
    public class RollbackUnitOfWorkCmdlet : UnitOfWorkCmdlet
    {
        protected override void ProcessRecord()
        {
            const string errorId = "RollbackUnitOfWork";
            const string action = "rollback";

            if (!VerifyShouldProcess(errorId, action))
            {
                return;
            }
            
            Context.UndoCurrentUnitOfWork();
        }
    }
}