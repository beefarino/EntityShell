# EntityShell

PowerShell Module for Entity Framework Tooling

# Building / Installation

	import-module psake
	invoke-psake install

# QuickStart

	# import the entity shell module
	PS C:\>	import-module entityshell
	
	# import the assembly containing your Entity Framework DBContext type
	PS C:\>	get-item MyProject.Model.dll | import-module
	
	# define a new entity drive from your DBContext type
	PS C:\> new-psdrive -name e -root '' -psp entityprovider -context [SuperAwesomeWebsite.Models.UserGroupContext]
	
	WARNING: column "CurrentLocation" does not fit into the display and was removed.
	
	Name           Used (GB)     Free (GB) Provider      Root                               
	----           ---------     --------- --------      ----                               
	e                                      EntityProv...                                    
	
	
	# list all entity collections
	PS C:\> dir e:/
	
	
	PSPath                    : EntityProvider::UserGroups
	PSParentPath              : 
	PSChildName               : UserGroups
	PSDrive                   : e
	PSProvider                : EntityProvider
	PSIsContainer             : True
	SSItemMode                : d+ <      
	GetNodeChildrenParameters : CodeOwls.EntityProvider.GetNodeChildrenParameters
	Name                      : UserGroups
	NewItemTypeNames          : 
	NewItemParameters         : SuperAwesomeWebsite.Models.RuntimeParameters.New.UserGroup
	ItemMode                  : d+ <      

	PSPath                    : EntityProvider::Members
	PSParentPath              : 
	PSChildName               : Members
	PSDrive                   : e
	PSProvider                : EntityProvider
	PSIsContainer             : True
	SSItemMode                : d+ <      
	GetNodeChildrenParameters : CodeOwls.EntityProvider.GetNodeChildrenParameters
	Name                      : Members
	NewItemTypeNames          : 
	NewItemParameters         : SuperAwesomeWebsite.Models.RuntimeParameters.New.Member
	ItemMode                  : d+ <      



	# list all usergroup entities
	PS C:\> dir e:/usergroups


	PSPath         : EntityProvider::usergroups\1
	PSParentPath   : EntityProvider::usergroups
	PSChildName    : 1
	PSDrive        : e
	PSProvider     : EntityProvider
	PSIsContainer  : False
	EntityDrive    : e
	EntityState    : Unchanged
	SSItemMode     :   ~<>     
	Members        : {}
	Id             : 1
	Name           : Charlotte PowerShell User Group
	_entityWrapper : System.Data.Objects.Internal.EntityWrapperWithoutRelationships`1[System.
					 Data.Entity.DynamicProxies.UserGroup_616034BE537E97D7EBEEE3001A5BA59C92C
					 DED76F4803BF49D77AB03D9195277]

	PSPath         : EntityProvider::usergroups\2
	PSParentPath   : EntityProvider::usergroups
	PSChildName    : 2
	PSDrive        : e
	PSProvider     : EntityProvider
	PSIsContainer  : False
	EntityDrive    : e
	EntityState    : Unchanged
	SSItemMode     :   ~<>     
	Members        : {}
	Id             : 2
	Name           : Arizona POwerShell User Group
	_entityWrapper : System.Data.Objects.Internal.EntityWrapperWithoutRelationships`1[System.
					 Data.Entity.DynamicProxies.UserGroup_616034BE537E97D7EBEEE3001A5BA59C92C
					 DED76F4803BF49D77AB03D9195277]



	# define a new user group entity
	PS C:\> new-item e:/usergroups -UserGroupName "Owomonomonoc PowerShell User Group"


	PSPath        : EntityProvider::usergroups\
	PSParentPath  : 
	PSChildName   : 
	PSDrive       : e
	PSProvider    : EntityProvider
	PSIsContainer : False
	Id            : 0
	Name          : Owomonomonoc PowerShell User Group
	Members       : 



	# view local (uncommitted) entity changes to the usergroups collection
	PS C:\> dir e:\usergroups -local


	PSPath        : EntityProvider::usergroups\0
	PSParentPath  : EntityProvider::usergroups
	PSChildName   : 0
	PSDrive       : e
	PSProvider    : EntityProvider
	PSIsContainer : False
	EntityDrive   : e
	EntityState   : Added
	SSItemMode    :   ~<>     
	Id            : 0
	Name          : Owomonomonoc PowerShell User Group
	Members       : 

	PSPath         : EntityProvider::usergroups\1
	PSParentPath   : EntityProvider::usergroups
	PSChildName    : 1
	PSDrive        : e
	PSProvider     : EntityProvider
	PSIsContainer  : False
	EntityDrive    : e
	EntityState    : Unchanged
	SSItemMode     :   ~<>     
	Members        : {}
	Id             : 1
	Name           : Charlotte PowerShell User Group
	_entityWrapper : System.Data.Objects.Internal.EntityWrapperWithoutRelationships`1[System.
					 Data.Entity.DynamicProxies.UserGroup_616034BE537E97D7EBEEE3001A5BA59C92C
					 DED76F4803BF49D77AB03D9195277]

	PSPath         : EntityProvider::usergroups\2
	PSParentPath   : EntityProvider::usergroups
	PSChildName    : 2
	PSDrive        : e
	PSProvider     : EntityProvider
	PSIsContainer  : False
	EntityDrive    : e
	EntityState    : Unchanged
	SSItemMode     :   ~<>     
	Members        : {}
	Id             : 2
	Name           : Arizona POwerShell User Group
	_entityWrapper : System.Data.Objects.Internal.EntityWrapperWithoutRelationships`1[System.
					 Data.Entity.DynamicProxies.UserGroup_616034BE537E97D7EBEEE3001A5BA59C92C
					 DED76F4803BF49D77AB03D9195277]


	# view committed (remote) entities
	PS C:\> dir e:\usergroups 


	PSPath         : EntityProvider::usergroups\1
	PSParentPath   : EntityProvider::usergroups
	PSChildName    : 1
	PSDrive        : e
	PSProvider     : EntityProvider
	PSIsContainer  : False
	EntityDrive    : e
	EntityState    : Unchanged
	SSItemMode     :   ~<>     
	Members        : {}
	Id             : 1
	Name           : Charlotte PowerShell User Group
	_entityWrapper : System.Data.Objects.Internal.EntityWrapperWithoutRelationships`1[System.
					 Data.Entity.DynamicProxies.UserGroup_616034BE537E97D7EBEEE3001A5BA59C92C
					 DED76F4803BF49D77AB03D9195277]

	PSPath         : EntityProvider::usergroups\2
	PSParentPath   : EntityProvider::usergroups
	PSChildName    : 2
	PSDrive        : e
	PSProvider     : EntityProvider
	PSIsContainer  : False
	EntityDrive    : e
	EntityState    : Unchanged
	SSItemMode     :   ~<>     
	Members        : {}
	Id             : 2
	Name           : Arizona POwerShell User Group
	_entityWrapper : System.Data.Objects.Internal.EntityWrapperWithoutRelationships`1[System.
					 Data.Entity.DynamicProxies.UserGroup_616034BE537E97D7EBEEE3001A5BA59C92C
					 DED76F4803BF49D77AB03D9195277]


	# commit the current unit of work associated with the e:/ entity drive
	PS C:\> complete-unitofwork e:/

	Confirm
	Are you sure you want to perform this action?

	Performing operation "complete" on Target "current unit of work".
	[Y] Yes  [A] Yes to All  [N] No  [L] No to All  [S] Suspend  [?] Help (default is "Y"): y

	Confirm
	Continue with this operation?
	[Y] Yes  [N] No  [S] Suspend  [?] Help (default is "Y"): y

	# view remote (committed) entities in the usergroups collection
	PS C:\> dir e:\usergroups 


	PSPath         : EntityProvider::usergroups\1
	PSParentPath   : EntityProvider::usergroups
	PSChildName    : 1
	PSDrive        : e
	PSProvider     : EntityProvider
	PSIsContainer  : False
	EntityDrive    : e
	EntityState    : Unchanged
	SSItemMode     :   ~<>     
	Members        : {}
	Id             : 1
	Name           : Charlotte PowerShell User Group
	_entityWrapper : System.Data.Objects.Internal.EntityWrapperWithoutRelationships`1[System.
					 Data.Entity.DynamicProxies.UserGroup_616034BE537E97D7EBEEE3001A5BA59C92C
					 DED76F4803BF49D77AB03D9195277]

	PSPath         : EntityProvider::usergroups\2
	PSParentPath   : EntityProvider::usergroups
	PSChildName    : 2
	PSDrive        : e
	PSProvider     : EntityProvider
	PSIsContainer  : False
	EntityDrive    : e
	EntityState    : Unchanged
	SSItemMode     :   ~<>     
	Members        : {}
	Id             : 2
	Name           : Arizona POwerShell User Group
	_entityWrapper : System.Data.Objects.Internal.EntityWrapperWithoutRelationships`1[System.
					 Data.Entity.DynamicProxies.UserGroup_616034BE537E97D7EBEEE3001A5BA59C92C
					 DED76F4803BF49D77AB03D9195277]

	PSPath         : EntityProvider::usergroups\3
	PSParentPath   : EntityProvider::usergroups
	PSChildName    : 3
	PSDrive        : e
	PSProvider     : EntityProvider
	PSIsContainer  : False
	EntityDrive    : e
	EntityState    : Unchanged
	SSItemMode     :   ~<>     
	Members        : {}
	Id             : 3
	Name           : Owomonomonoc PowerShell User Group
	_entityWrapper : System.Data.Objects.Internal.EntityWrapperWithoutRelationships`1[System.
					 Data.Entity.DynamicProxies.UserGroup_616034BE537E97D7EBEEE3001A5BA59C92C
					 DED76F4803BF49D77AB03D9195277]

