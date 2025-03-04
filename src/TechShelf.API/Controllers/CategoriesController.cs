using MediatR;
using Microsoft.AspNetCore.Mvc;
using TechShelf.Application.Features.Categories.Queries.GetAllCategories;
using TechShelf.Application.Features.Categories.Queries.Shared;

namespace TechShelf.API.Controllers;

public class CategoriesController : BaseApiController
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll(CancellationToken cancellationToken = default)
    {
        var query = new GetAllCategoriesQuery();
        var categories = await _mediator.Send(query, cancellationToken);

        return categories;
    }
}
