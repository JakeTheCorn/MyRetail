using System.Collections.Generic;

namespace Api.Features.Products
{
    public class PartialProduct
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Sku { get; set; }
        public bool AvailableOnline { get; set; }
    }
    

    public class Product : PartialProduct
    {
        public int Id { get; init; }
    }

    public class ProductDataItem : Product
    {
    }

    public class MutateProductInput<TProduct>
    {
        public TProduct Data { get; set; }
    }

    public class CreateProductInput : MutateProductInput<PartialProduct>
    { }

    public class ProductUpdateSubmission
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Sku { get; set; }
        public bool? AvailableOnline { get; set; }
    }

    public class UpdateProductInput : MutateProductInput<ProductUpdateSubmission>
    { }

    public class OnlyNullableId
    {
        public int? Id { get; init; }
    }

    public class DeleteProductInput : MutateProductInput<OnlyNullableId>
    { }
    
    
    public class UpdateProductResponseDto : CreateProductResponseDto
    {}
    
    public class CreateProductResponseDto
    {
        public ProductDto Product { get; init; }
    }
    
    public class GetProductsInput
    {
        public string Name { get; init; }
    }

    public class ErrorableOutput<TError> where TError : Error
    {
        public TError Error { get; init; }

        public bool HasError()
        {
            return Error is not null;
        }
    }

    public class CreateProductOutput : ErrorableOutput<Error>
    {
        public Product Product { get; init; }
    }
    
    public class UpdateProductOutput : CreateProductOutput
    { }

    public class DeleteProductOutput : ErrorableOutput<Error>
    { }

    public class Error
    {
        public string Message { get; }

        public Error(string message)
        {
            Message = message;
        }

        public Error()
        {
        }
    }

    public class InputValidationError : Error
    {
        public InputValidationError(string message) : base(message)
        { }
    }

    public class DataRetrievalError : Error
    {
        public DataRetrievalError(string message) : base(message)
        {
        }
    }

    public class DataCreationError : Error
    { }

    public class DataUpdateError : Error
    {
    }

    public class DataDeletionError : Error
    { }


    public class DataNotFoundError : Error
    {
    }
    
    public class GetProductsResponseDto
    {
        public List<ProductDto> Products { get; init; }
    }

    public class ProductDto : Product
    {
    }

    public class UpdateProductInputDto : UpdateProductInput
    { }

    public class GetProductDataOutput : ErrorableOutput<Error>
    {
        public List<ProductDataItem> Products { get; init; } = new ();
    }
    
    public class CreateProductInputDto : CreateProductInput
    { }
    

    public class GetProductsOutput : ErrorableOutput<Error>
    {
        public List<Product> Products { get; init; }
    }
    
    public class GetProductDataInput
    {
        public string Name { get; init; }
    }

    public class CreateProductDataInput : PartialProduct
    { }

    public class CreateProductDataOutput : ErrorableOutput<Error>
    {
        public Product Product { get; init; }
    }
    
    public class UpdateProductDataInput : ProductUpdateSubmission
    {
        public new int Id { get; set; }
    }

    public class UpdateProductDataOutput : ErrorableOutput<Error>
    {
        public Product Product { get; init; }
    }

    public class DeleteProductDataInput
    {
        public int Id { get; set; }
    }

    public class DeleteProductDataOutput : ErrorableOutput<Error>
    { }
    
    public class ProductLocationJunction
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int LocationId { get; set; }
    }

    public class Location
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AddressStreetLine1 { get; set; }
        public string AddressCity { get; set; }
        public string AddressState { get; set; }
        public string AddressZip { get; set; }
    }
}