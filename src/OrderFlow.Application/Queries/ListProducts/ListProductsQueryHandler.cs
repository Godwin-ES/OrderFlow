namespace OrderFlow.Application.Queries.ListProducts;

using MediatR;
using OrderFlow.Application.DTOs;
using OrderFlow.Domain.Interfaces;

public sealed class ListProductsQueryHandler(IProductRepository productRepository)
    : IRequestHandler<ListProductsQuery, IReadOnlyList<ProductResponse>>
{
    public async Task<IReadOnlyList<ProductResponse>> Handle(ListProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await productRepository.ListAsync(cancellationToken);

        return products
            .Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Sku = p.Sku,
                UnitPrice = p.UnitPrice,
                StockQuantity = p.StockQuantity
            })
            .ToList();
    }
}
