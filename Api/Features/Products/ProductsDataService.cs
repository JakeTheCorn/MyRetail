using System.Collections.Generic;
using System.Linq;
using Api.Common;

namespace Api.Features.Products
{
    public interface IProductsDataService
    {
        GetProductDataOutput GetProductData(GetProductDataInput input);
        CreateProductDataOutput CreateProductData(CreateProductDataInput input);
        UpdateProductDataOutput UpdateProductData(UpdateProductDataInput input);
        DeleteProductDataOutput DeleteProductData(DeleteProductDataInput input);
    }

    public class ProductsDataService : IProductsDataService
    {
        private readonly IDbClient _db;

        public ProductsDataService(IDbClient db)
        {
            _db = db;
        }

        public GetProductDataOutput GetProductData(GetProductDataInput input)
        {
            var products = _db.Query<ProductDataItem>($"select * from Products where name = '{input.Name}'");

            return new GetProductDataOutput
            {
                Products = products.ToList(),
            };
        }

        public CreateProductDataOutput CreateProductData(CreateProductDataInput input)
        {
            var product = new Product
            {
                Name = input.Name,
                Description = input.Description,
                Sku = input.Sku,
                AvailableOnline = input.AvailableOnline,
            };

            _db.Insert(product);

            return new CreateProductDataOutput
            {
                Product = product,
            };
        }

        public UpdateProductDataOutput UpdateProductData(UpdateProductDataInput input)
        {
            var updates = new List<string>();

            if (input.Name is not null)
                updates.Add($"Name = '{input.Name}'");
            
            if (input.Sku is not null)
                updates.Add($"Sku = '{input.Sku.Trim()}'");

            _db.Execute($"update Products set {string.Join(",", updates)} where Id = {input.Id}");
            
            return new UpdateProductDataOutput()
            {
                Product = new Product(), // todo 
            };
        }

        public DeleteProductDataOutput DeleteProductData(DeleteProductDataInput input)
        {
            _db.Delete(new Product { Id = input.Id });

            return new DeleteProductDataOutput();
        }
    }
}