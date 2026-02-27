using Studies.Domain.Enums;

namespace Studies.Domain.Errors;

public static class ProductError
{
    public static readonly Error DescriptionIsNull
        = new("Product.Description", "Description cannot be null", ErrorType.Validation);

    public static readonly Error PriceIsLowerThan0
        = new("Product.Price", "The price must be greater than 0", ErrorType.Validation);

    public static Error ProductNotFound(int id)
        => new("Product.NotFound", $"Product with {id} not found", ErrorType.NotFound);
}
