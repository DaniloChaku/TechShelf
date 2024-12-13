using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TechShelf.API.Requests.Products;
using TechShelf.Application.Common.Pagination;
using TechShelf.Application.Features.Products.Queries.GetProductById;
using TechShelf.Application.Features.Products.Queries.SearchProducts;
using TechShelf.Application.Features.Products.Queries.Shared;

namespace TechShelf.API.Controllers;

public class ProductsController : BaseApiController
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("search")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(PagedResult<ProductDto>))]
    [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest, type: typeof(ValidationProblemDetails))]
    public async Task<IActionResult> SearchProducts(
        [FromQuery] SearchProductsRequest request)
    {
        var query = request.Adapt<SearchProductsQuery>();
        var result = await _mediator.Send(query);

        return result.Match(
            products => Ok(products),
            errors => Problem(errors));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(PagedResult<ProductDto>))]
    [ProducesResponseType(statusCode: StatusCodes.Status404NotFound, type: typeof(ProblemDetails))]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetProductByIdQuery(id);
        var result = await _mediator.Send(query);

        return result.Match(
            product => Ok(product),
            errors => Problem(errors));
    }
}
