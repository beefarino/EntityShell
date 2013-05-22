using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace CodeOwls.EntityProvider.Cmdlets
{
    [Cmdlet( VerbsLifecycle.Complete, Nouns.UnitOfWork, ConfirmImpact = ConfirmImpact.High, SupportsShouldProcess = true)]
    public class CommitUnitOfWorkCmdlet : UnitOfWorkCmdlet
    {
        protected override void ProcessRecord()
        {
            string errorId = "CompleteUnitOfWork";
            string action = "complete";

            if (!VerifyShouldProcess(errorId, action))
            {
                return;
            }
            
            Context.CommitCurrentUnitOfWork();            
        }
    }
}
