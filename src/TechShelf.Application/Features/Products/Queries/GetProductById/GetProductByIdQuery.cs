using ErrorOr;
using MediatR;
using TechShelf.Application.Features.Products.Queries.Shared;

namespace TechShelf.Application.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery(int Id)
    : IRequest<ErrorOr<ProductDto>>;
