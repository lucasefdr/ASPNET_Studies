using Microsoft.AspNetCore.Mvc;
using Studies.Application.DTOs.Product;
using Studies.Application.Interfaces;

namespace Studies.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController(IProductService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ProductViewModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductDto model, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(model, cancellationToken);

        return result.IsSuccess ?
            CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : BadRequest(result.Errors);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await service.GetByIdAsync(id, cancellationToken);

        return result.IsSuccess ?
            Ok(result.Value)
            : NotFound(result.Errors);
    }
}
