using Mapster;
using MediatR;
using TechShelf.Application.Features.Brands.Queries.Shared;
using TechShelf.Application.Interfaces.Data;
using TechShelf.Domain.Products;

namespace TechShelf.Application.Features.Brands.Queries.GetAllBrands;

public class GetAllBrandsQueryHandler
    : IRequestHandler<GetAllBrandsQuery, List<BrandDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllBrandsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<BrandDto>> Handle(GetAllBrandsQuery request, CancellationToken cancellationToken)
    {
        var brands = await _unitOfWork.Repository<Brand>().GetAllAsync(cancellationToken);

        var brandDtos = brands.Adapt<List<BrandDto>>();

        return brandDtos;
    }
}
