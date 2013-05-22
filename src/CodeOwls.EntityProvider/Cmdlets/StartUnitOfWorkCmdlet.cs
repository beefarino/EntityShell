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
    [Cmdlet(VerbsLifecycle.Start, Nouns.UnitOfWork, ConfirmImpact = ConfirmImpact.High, SupportsShouldProcess = true, SupportsTransactions = false)]
    public class StartUnitOfWorkCmdlet : UnitOfWorkCmdlet
    {
        class UnitOfWorkSession : IDisposable
        {
            private readonly EntityDrive _drive;

            public UnitOfWorkSession( EntityDrive drive )
            {
                _drive = drive;
                drive.StartNewUnitOfWork();
            }

            public void Dispose()
            {
                if (null == _drive)
                {
                    return;
                }
                _drive.CommitCurrentUnitOfWork();
            }
        }

        public static IDisposable CreateSession(EntityDrive drive)
        {
            return new UnitOfWorkSession( drive );
        }

        protected override void ProcessRecord()
        {
            string errorId = "StartUnitOfWork";
            string action = "start";

            if (! AssertValidContext(errorId, null))
            {
                return;
            }

            if(Context.HasCurrentUnitOfWork)
            {
                if (!VerifyShouldProcess(errorId, action))
                {
                    return;
                }
            }

            Context.StartNewUnitOfWork();
        }
    }
}
