using Studies.Domain.Entities;
using Studies.Domain.Interfaces;
using Studies.Infrastructure.Persistence.Context;

namespace Studies.Infrastructure.Persistence.Repositories;

public class ProductRepository(AppDbContext context) : Repository<Product>(context), IProductRepository
{
}
