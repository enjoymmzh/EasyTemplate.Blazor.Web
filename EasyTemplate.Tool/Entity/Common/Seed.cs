namespace EasyTemplate.Tool.Entity;

public interface ISeedData<TEntity> where TEntity : class, new()
{
    IEnumerable<TEntity> Generate();
}
