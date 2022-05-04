namespace Trakx.Utils.Weavers;

public class TestClass
{
    public async Task MethodWithoutConfigureAwait()
    {
        await Task.CompletedTask;
    }

    public async Task MethodWithConfigureAwait()
    {
        await Task.CompletedTask.ConfigureAwait(false);
    }
}
