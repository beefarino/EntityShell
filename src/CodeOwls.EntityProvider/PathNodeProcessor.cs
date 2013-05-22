using CodeOwls.PowerShell.Paths.Processors;
using CodeOwls.PowerShell.Provider.PathNodes;

namespace CodeOwls.EntityProvider
{
    class PathNodeProcessor : PathNodeProcessorBase
    {
        private readonly EntityDrive _drive;

        public PathNodeProcessor( EntityDrive drive )
        {
            _drive = drive;
        }

        protected override INodeFactory Root
        {
            get { return new RootNodeFactory(_drive); }
        }
    }
}