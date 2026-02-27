using Studies.Application.DTOs.Product;
using Studies.Application.Interfaces;
using Studies.Domain.Entities;
using Studies.Domain.Errors;
using Studies.Domain.Interfaces;
using Studies.Domain.Shared;

namespace Studies.Application.Services;

public class ProductService(IUnitOfWork uof) : IProductService
{
    public async Task<Result<ProductViewModel>> CreateAsync(CreateProductDto model, CancellationToken cancellationToken = default)
    {
        var productResult = Product.Create(model.Description, model.Price);

        if (productResult.IsFailure)
            return Result.Failure<ProductViewModel>(productResult.Errors);

        await uof.Products.AddAsync(productResult.Value, cancellationToken);
        await uof.CommitAsync(cancellationToken);

        var response = new ProductViewModel(productResult.Value.Id, productResult.Value.Description, productResult.Value.Price);

        return Result.Success(response);
    }

    public async Task<Result<ProductViewModel>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var productResult = await uof.Products.GetByIdAsync(id, cancellationToken);

        if (productResult is null)
            return Result.Failure<ProductViewModel>(ProductError.ProductNotFound(id));

        var response = new ProductViewModel(productResult.Id, productResult.Description, productResult.Price);

        return Result.Success(response);
    }
}
