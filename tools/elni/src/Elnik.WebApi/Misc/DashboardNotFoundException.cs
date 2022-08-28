namespace Elnik.WebApi.Misc;

public class DashboardNotFoundException : Exception
{
    public DashboardNotFoundException(Exception innerException) : base(null, innerException) {
    }

    public static string Wrap<TException>(Func<string> func) where TException : Exception
    {
        try
        {
            return func();
        }
        catch (TException ex)
        {
            throw new DashboardNotFoundException(ex);
        }
    }
}