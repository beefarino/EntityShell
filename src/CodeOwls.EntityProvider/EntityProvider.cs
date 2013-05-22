using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Text;
using System.Threading.Tasks;
using CodeOwls.PowerShell.Paths.Processors;
using CodeOwls.PowerShell.Provider;
using CodeOwls.EntityProvider;

namespace CodeOwls.EntityProvider
{
    [CmdletProvider("EntityProvider", ProviderCapabilities.ShouldProcess | ProviderCapabilities.Filter)]
    public class EntityProvider : Provider
    {
        GenericEntityContextDrive Drive
        {
            get
            {                
                var drive = this.PSDriveInfo as GenericEntityContextDrive ?? this.ProviderInfo.Drives[0] as GenericEntityContextDrive;
                return drive;
            }
        }

        protected override PSDriveInfo NewDrive(PSDriveInfo drive)
        {
            if (drive is GenericEntityContextDrive)
            {
                return drive;
            }

            var p = this.DynamicParameters as GenericEntityContextDrive.NewParams;
            return new GenericEntityContextDrive( p.ContextType, drive );
        }

        protected override object NewDriveDynamicParameters()
        {
            return new GenericEntityContextDrive.NewParams();
        }

        protected override IPathNodeProcessor PathNodeProcessor
        {
            get { return new PathNodeProcessor(Drive); }
        }
    }
}
