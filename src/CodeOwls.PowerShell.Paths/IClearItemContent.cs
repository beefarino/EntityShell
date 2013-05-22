using CodeOwls.PowerShell.Provider.PathNodeProcessors;

namespace CodeOwls.PowerShell.Paths
{
    public interface IClearItemContent
    {
        void ClearContent(IContext context);
        object ClearContentDynamicParameters(IContext context);
    }
}