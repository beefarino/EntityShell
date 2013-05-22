using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using CodeOwls.EntityProvider.Adapters;

namespace CodeOwls.EntityProvider.Cmdlets
{
    [Cmdlet(VerbsData.Save, Nouns.DynamicParameterTypeAssembly, ConfirmImpact = ConfirmImpact.None)]
    public class SaveDynamicTypesCmdlet : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            var path = EntityRuntimeParameterAdapter.SaveAssembly();
            if (null == path)
            {
                WriteWarning( "No dynamic parameter assembly has been defined for the current session.");
                return;
            }
            WriteObject( new FileInfo(path) );
        }

    }
}
