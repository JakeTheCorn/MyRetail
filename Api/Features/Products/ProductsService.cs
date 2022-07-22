using System;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Api.Features.Products
{
    public interface IProductsService
    {
        GetProductsOutput GetProducts(GetProductsInput input);
        CreateProductOutput CreateProduct(CreateProductInput input);
        UpdateProductOutput UpdateProduct(UpdateProductInput input);
        DeleteProductOutput DeleteProduct(DeleteProductInput input);
    }
    
    public class ProductsService : IProductsService
    {
        private readonly IProductsDataService _dataService;
        private readonly ILogger<ProductsService> _logger;

        public ProductsService(IProductsDataService dataService, ILogger<ProductsService> logger)
        {
            _dataService = dataService;
            _logger = logger;
        }

        public GetProductsOutput GetProducts(GetProductsInput input)
        {
            var inputValidationError = new ProductInputValidator().GetInputValidationError(input);

            if (inputValidationError is not null)
                return new GetProductsOutput
                {
                    Error = inputValidationError,
                };

            var getProductDataOutput = _dataService.GetProductData(new GetProductDataInput
            {
                Name = input.Name,
            });

            return GetOutput(getProductDataOutput);
        }

        public CreateProductOutput CreateProduct(CreateProductInput input)
        {
            var inputValidationOutput = GetInputValidationOutput(input);

            if (inputValidationOutput is not null)
                return inputValidationOutput;

            var result = _dataService.CreateProductData(ToCreateProductDataInput(input));

            return GetOutput(result);
        }

        

        public UpdateProductOutput UpdateProduct(UpdateProductInput input)
        {
            var inputValidationOutput = GetInputValidationOutput(input);

            if (inputValidationOutput is not null)
                return inputValidationOutput;
            
            var updateProductDataOutput = _dataService.UpdateProductData(ToUpdateProductDataInput(input));

            return GetOutput(updateProductDataOutput);
        }

        private UpdateProductOutput GetOutput(UpdateProductDataOutput updateProductDataOutput)
        {
            if (updateProductDataOutput.HasError())
            {
                _logger.LogWarning($"Error occurred while updating product - '{updateProductDataOutput.Error.Message}'");
                return new UpdateProductOutput
                {
                    Error = new DataUpdateError()
                };
            }

            return new UpdateProductOutput
            {
                Product = updateProductDataOutput.Product,
            };
        }
        
        private CreateProductOutput GetOutput(CreateProductDataOutput result)
        {
            if (result.HasError())
            {
                _logger.LogWarning($"Error occurred while creating product - '{result.Error.Message}'");
                return new CreateProductOutput
                {
                    Error = new DataCreationError(),
                };
            }

            return new CreateProductOutput
            {
                Product = result.Product,
            };
        }
        
        private static GetProductsOutput GetOutput(GetProductDataOutput getProductDataOutput)
        {
            if (getProductDataOutput.HasError())
            {
                return new GetProductsOutput
                {
                    Error = new DataRetrievalError(getProductDataOutput.Error.Message),
                };
            }

            return new GetProductsOutput
            {
                Products = getProductDataOutput.Products.Select(ToProduct).ToList(),
            };
        }

        public DeleteProductOutput DeleteProduct(DeleteProductInput input)
        {
            var inputValidationOutput = GetInputValidationOutput(input);

            if (inputValidationOutput is not null)
                return inputValidationOutput;

            var deleteProductDataOutput = _dataService.DeleteProductData(new DeleteProductDataInput
            {
                Id = Convert.ToInt32(input.Data.Id),
            });

            return GetOutput(deleteProductDataOutput);
        }

        private static DeleteProductOutput GetOutput(DeleteProductDataOutput deleteProductDataOutput)
        {
            if (deleteProductDataOutput.HasError())
                return new DeleteProductOutput
                {
                    Error = new DataDeletionError(),
                };

            return new DeleteProductOutput();
        }

        private static DeleteProductOutput GetInputValidationOutput(DeleteProductInput input)
        {
            var inputValidationError = new ProductInputValidator().GetInputValidationError(input);

            if (inputValidationError is not null)
                return new DeleteProductOutput
                {
                    Error = inputValidationError,
                };

            return null;
        }

        private CreateProductOutput GetInputValidationOutput(CreateProductInput input)
        {
            var inputValidationError = new ProductInputValidator().GetInputValidationError(input);
            
            if (inputValidationError is not null)
                return new CreateProductOutput
                {
                    Error = inputValidationError,
                };
            return null;
        }
        
        private UpdateProductOutput GetInputValidationOutput(UpdateProductInput input)
        {
            var inputValidationError = new ProductInputValidator().GetInputValidationError(input);
            if (inputValidationError is not null)
                return new UpdateProductOutput
                {
                    Error = inputValidationError,
                };

            return null;
        }

        private CreateProductDataInput ToCreateProductDataInput(CreateProductInput input)
        {
            var d = input.Data;
            return new CreateProductDataInput
            {
                Name = d.Name,
                Description = d.Description,
                Sku = d.Sku,
                AvailableOnline = d.AvailableOnline,
            };
        }
        
        private static UpdateProductDataInput ToUpdateProductDataInput(UpdateProductInput input)
        {
            var d = input.Data;
            return new UpdateProductDataInput()
            {
                Id = Convert.ToInt32(d.Id),
                Name = d.Name,
                Description = d.Description,
                Sku = d.Sku,
                AvailableOnline = d.AvailableOnline,
            };
        }

        private static Product ToProduct(ProductDataItem x)
        {
            return new Product
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Sku = x.Sku,
                AvailableOnline = x.AvailableOnline,
            };
        }

        public CreateProductOutput DeleteProduct(UpdateProductInput updateProductInput)
        {
            throw new NotImplementedException();
        }
    }
}