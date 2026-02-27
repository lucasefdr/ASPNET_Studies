using Studies.Domain.Errors;
using Studies.Domain.Shared;

namespace Studies.Domain.Entities;

public class Product : EntityBase, IAggregateRoot
{
    public string Description { get; private set; }
    public decimal Price { get; private set; }

    protected Product() { }

    private Product(string description, decimal price)
    {
        Description = description;
        Price = price;
    }

    public static Result<Product> Create(string description, decimal price)
    {
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(description))
            errors.Add(ProductError.DescriptionIsNull);

        if (price < 0)
            errors.Add(ProductError.PriceIsLowerThan0);

        return errors.Count != 0
            ? Result.Failure<Product>(errors)
            : Result.Success(new Product(description, price));
    }
}
