using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Api.Features.Products
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : Controller
    {
        private readonly IProductsService _productsService;

        public ProductsController(IProductsService productsService)
        {
            _productsService = productsService;
        }
        
        [HttpGet]
        public IActionResult SearchProducts(string name)
        {
            var getProductsResult = _productsService.GetProducts(ToGetProductsInput(name));

            return GetResponse(getProductsResult);
        }

        [HttpPost]
        public IActionResult CreateProduct(CreateProductInputDto dto)
        {
            var createProductResponse = _productsService.CreateProduct(dto);

            return GetResponse(createProductResponse);
        }

        public IActionResult UpdateProduct(UpdateProductInputDto inputDto)
        {
            var updateProductResponse = _productsService.UpdateProduct(inputDto);

            return GetResponse(updateProductResponse);
        }
        
        [HttpDelete]
        public IActionResult DeleteProduct(int? id)
        {
            var deleteProductOutput = _productsService.DeleteProduct(ToDeleteProductInput(id));

            return GetResponse(deleteProductOutput);
        }

        private IActionResult GetResponse(DeleteProductOutput deleteProductOutput)
        {
            if (deleteProductOutput.HasError())
            {
                var error = deleteProductOutput.Error;

                if (error is InputValidationError)
                    return BadRequest($"Input validation error occurred - '{deleteProductOutput.Error.Message}'");

                if (error is DataNotFoundError)
                    return NotFound("Product not found");

                return StatusCode(500);
            }

            return Ok();
        }

        private IActionResult GetResponse(UpdateProductOutput updateProductResponse)
        {
            var error = updateProductResponse.Error;
            
            if (error is InputValidationError)
                return BadRequest($"Input validation error occurred - '{error.Message}'");
            
            if (error is DataNotFoundError)
                return NotFound("Unable to update non existent entity");

            return Ok(new UpdateProductResponseDto
            {
                Product = ToProductDto(updateProductResponse.Product),
            });
        }
        
        private IActionResult GetResponse(CreateProductOutput createProductResponse)
        {
            // todo: handle other error types...
            if (createProductResponse.HasError())
                if (createProductResponse.Error is InputValidationError)
                    return BadRequest($"Input validation error occurred - '{createProductResponse.Error.Message}'");

            return Ok(new CreateProductResponseDto
            {
                Product = ToProductDto(createProductResponse.Product),
            });
        }
        
        private IActionResult GetResponse(GetProductsOutput getProductsResult)
        {
            if (getProductsResult.HasError())
            {
                var error = getProductsResult.Error;

                if (error is InputValidationError)
                    return BadRequest($"Input validation error occurred - '{error.Message}'");

                if (error is DataRetrievalError)
                    return StatusCode(500);
            }

            if (!getProductsResult.Products.Any())
                return NotFound("No products were found");

            return Ok(ToGetProductsResponseDto(getProductsResult));
        }

        private GetProductsResponseDto ToGetProductsResponseDto(GetProductsOutput output)
        {
            return new GetProductsResponseDto
            {
                Products = output.Products.Select(ToProductDto).ToList(),
            };
        }

        private ProductDto ToProductDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Sku = product.Sku,
                AvailableOnline = product.AvailableOnline
            };
        }
        
        private static DeleteProductInput ToDeleteProductInput(int? id)
        {
            return new DeleteProductInput
            {
                Data = new OnlyNullableId
                {
                    Id = id,
                },
            };
        }
        
        private static GetProductsInput ToGetProductsInput(string name)
        {
            return new GetProductsInput
            {
                Name = name,
            };
        }
    }
}

// todo: unrecognized errors send 500