using System.Net.Http;
using System.Threading.Tasks;

namespace Trakx.Utils.Apis;

public interface ICredentialsProvider
{
    void AddCredentials(HttpRequestMessage msg);
    Task AddCredentialsAsync(HttpRequestMessage msg);
}

public class NoCredentialsProvider : ICredentialsProvider
{
    #region Implementation of ICredentialsProvider

    /// <inheritdoc />
    public void AddCredentials(HttpRequestMessage msg)
    {
        //don't add any credentials
    }

    public Task AddCredentialsAsync(HttpRequestMessage msg)
    {
        return Task.CompletedTask;
    }

    #endregion
}