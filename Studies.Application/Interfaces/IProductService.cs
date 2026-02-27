using Studies.Application.DTOs.Product;
using Studies.Domain.Shared;

namespace Studies.Application.Interfaces;

public interface IProductService
{
    Task<Result<ProductViewModel>> CreateAsync(CreateProductDto model, CancellationToken cancellationToken = default);
    Task<Result<ProductViewModel>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
