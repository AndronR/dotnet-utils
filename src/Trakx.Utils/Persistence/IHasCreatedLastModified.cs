namespace Trakx.Utils.Persistence;

public interface IHasCreatedLastModified
{
    DateTimeOffset Created { get; set; }
    DateTimeOffset LastModified { get; set; }
}