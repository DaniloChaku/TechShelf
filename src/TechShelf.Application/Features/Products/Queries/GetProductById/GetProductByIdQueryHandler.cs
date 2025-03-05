using ErrorOr;
using Mapster;
using MediatR;
using TechShelf.Application.Features.Products.Queries.Shared;
using TechShelf.Application.Interfaces.Data;
using TechShelf.Domain.Products;
using TechShelf.Domain.Products.Specs;

namespace TechShelf.Application.Features.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler
    : IRequestHandler<GetProductByIdQuery, ErrorOr<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = new ProductByIdSpec(request.Id);

        var product = await _unitOfWork.Repository<Product>()
            .FirstOrDefaultAsync(spec, cancellationToken);

        if (product == null)
        {
            return ProductErrors.NotFound(request.Id);
        }

        var productDto = product.Adapt<ProductDto>();

        return productDto;
    }
}
