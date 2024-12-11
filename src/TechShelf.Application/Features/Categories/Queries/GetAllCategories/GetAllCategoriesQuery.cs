using MediatR;
using TechShelf.Application.Features.Categories.Queries.Shared;

namespace TechShelf.Application.Features.Categories.Queries.GetAllCategories;

public record GetAllCategoriesQuery : IRequest<List<CategoryDto>>;
