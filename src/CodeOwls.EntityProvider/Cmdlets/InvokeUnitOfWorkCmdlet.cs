using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using CodeOwls.EntityProvider.Attributes;

namespace CodeOwls.EntityProvider.Cmdlets
{
    [Cmdlet( VerbsLifecycle.Invoke, Nouns.UnitOfWork, ConfirmImpact = ConfirmImpact.High, SupportsShouldProcess = true, SupportsTransactions = false)]
    public class InvokeUnitOfWorkCmdlet : UnitOfWorkCmdlet
    {
        [Parameter(Mandatory = true, Position = 1)]
        public ScriptBlock Script { get; set; }

        protected override void ProcessRecord()
        {
            if (! VerifyShouldProcess("InvokeUnitOfWork", "invoke", Script))
            {
                return;
            }

            using (StartUnitOfWorkCmdlet.CreateSession(Context))
            {
                Script.Invoke();
            }
        }
    }
}
