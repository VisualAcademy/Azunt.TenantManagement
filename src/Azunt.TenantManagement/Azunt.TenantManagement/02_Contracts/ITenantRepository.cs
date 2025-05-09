using Dul.Articles;

namespace Azunt.TenantManagement
{
    public interface ITenantRepository
    {
        Task<Tenant> AddAsync(Tenant model, string? connectionString = null);
        Task<List<Tenant>> GetAllAsync(string? connectionString = null);
        Task<Tenant> GetByIdAsync(long id, string? connectionString = null);
        Task<bool> UpdateAsync(Tenant model, string? connectionString = null);
        Task<bool> DeleteAsync(long id, string? connectionString = null);
        Task<ArticleSet<Tenant, int>> GetArticlesAsync<TParentIdentifier>(int pageIndex, int pageSize, string searchField, string searchQuery, string sortOrder, TParentIdentifier parentIdentifier, string? connectionString = null);
        Task<ArticleSet<Tenant, long>> GetByAsync<TParentIdentifier>(FilterOptions<TParentIdentifier> options, string? connectionString = null);
    }
}
