using MediatR;
using TechShelf.Application.Features.Brands.Queries.Shared;

namespace TechShelf.Application.Features.Brands.Queries.GetAllBrands;

public record GetAllBrandsQuery : IRequest<List<BrandDto>>;
