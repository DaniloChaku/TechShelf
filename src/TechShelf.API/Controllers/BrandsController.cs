using MediatR;
using Microsoft.AspNetCore.Mvc;
using TechShelf.Application.Features.Brands.Queries.GetAllBrands;
using TechShelf.Application.Features.Brands.Queries.Shared;

namespace TechShelf.API.Controllers;

public class BrandsController : BaseApiController
{
    private readonly IMediator _mediator;

    public BrandsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<BrandDto>>> GetAll()
    {
        var query = new GetAllBrandsQuery();
        var brands = await _mediator.Send(query);

        return brands;
    }
}
