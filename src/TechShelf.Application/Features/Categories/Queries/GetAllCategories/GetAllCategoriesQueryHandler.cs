using Mapster;
using MediatR;
using TechShelf.Application.Features.Categories.Queries.Shared;
using TechShelf.Application.Interfaces.Data;
using TechShelf.Domain.Products;

namespace TechShelf.Application.Features.Categories.Queries.GetAllCategories;

public class GetAllCategoriesQueryHandler
    : IRequestHandler<GetAllCategoriesQuery, List<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllCategoriesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<CategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _unitOfWork.Repository<Category>().GetAllAsync(cancellationToken);

        var categoryDtos = categories.Adapt<List<CategoryDto>>();

        return categoryDtos;
    }
}
