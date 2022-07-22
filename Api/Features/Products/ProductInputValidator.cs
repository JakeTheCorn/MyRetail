using static Api.Common.StringUtils;

namespace Api.Features.Products
{
    public class ProductInputValidator
    {
        public InputValidationError GetInputValidationError(CreateProductInput input)
        {
            var data = input.Data;

            if (string.IsNullOrWhiteSpace(data.Name))
                return new InputValidationError("Name must not be empty");

            if (data.Name.Trim().Length >= 30)
                return new InputValidationError("Valid product names are shorter than 30 characters");

            if (string.IsNullOrWhiteSpace(data.Description))
                return new InputValidationError("Description must not be empty");

            if (string.IsNullOrWhiteSpace(data.Sku))
                return new InputValidationError("Sku must not be empty");

            var sku = data.Sku.Trim().ToUpper();
            var skuContainsNonAlphanumericCharacters = !ContainsOnlyAlphanumeric(sku);

            if (skuContainsNonAlphanumericCharacters)
                return new InputValidationError("Sku must contain only numbers or letters");

            if (sku.Length != 16)
                return new InputValidationError("Sku must be exactly 16 characters long");

            return null;
        }
        
        public InputValidationError GetInputValidationError(GetProductsInput input)
        {
            if (string.IsNullOrWhiteSpace(input.Name))
                return new InputValidationError("name must not be empty");
            if (input.Name.Length >= 30)
                return new InputValidationError("Valid product names are shorter than 30 characters");
            return null;
        }

        public InputValidationError GetInputValidationError(UpdateProductInput input)
        {
            var data = input.Data;

            var updatesSupplied = false;

            if (data.Id is null)
                return new InputValidationError("Id must be present");

            if (data.Name is not null)
            {
                updatesSupplied = true;
                if (string.IsNullOrWhiteSpace(data.Name))
                    return new InputValidationError("Name must not be empty");
                if (data.Name.Trim().Length >= 30)
                    return new InputValidationError("Valid product names are shorter than 30 characters");
            }

            if (data.Description is not null)
            {
                updatesSupplied = true;
                if (string.IsNullOrWhiteSpace(data.Description))
                    return new InputValidationError("Description must not be empty");
            }

            if (data.Sku is not null)
            {
                updatesSupplied = true;
                if (string.IsNullOrWhiteSpace(data.Sku))
                    return new InputValidationError("Sku must not be empty");
                
                var sku = data.Sku.Trim().ToUpper();
                var skuContainsNonAlphanumericCharacters = !ContainsOnlyAlphanumeric(sku);

                if (skuContainsNonAlphanumericCharacters)
                    return new InputValidationError("Sku must contain only numbers or letters");
                
                if (sku.Length != 16)
                    return new InputValidationError("Sku must be exactly 16 characters long");   
            }

            if (data.AvailableOnline is not null)
                updatesSupplied = true;

            if (!updatesSupplied)
                return new InputValidationError("No updates were supplied");

            return null;
        }

        public InputValidationError GetInputValidationError(DeleteProductInput input)
        {
            if (input.Data.Id is null)
                return new InputValidationError("Id must be present");
            
            return null;
        }
    }
}