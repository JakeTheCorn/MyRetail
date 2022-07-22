using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Api.Features.Products;
using NUnit.Framework;
using static Api.UnitTests.TestUtils;

namespace Api.UnitTests.Features.Products
{
    public class ProductsServiceTests_DeleteProduct : ProductsServiceTests
    {
        [Test]
        public void It_Returns_InputValidationError_If_Id_Is_Not_Present()
        {
            Act(GetInput(id: null));
            
            AssertInputValidationMessage("Id must be present");
        }

        [Test]
        public void It_Returns_A_DataDeletionError_If_DataService_Returns_An_Error()
        {
            var error = new Error("Could not delete...");
            
            DeleteProductDataReturns(new DeleteProductDataOutput
            {
                Error = error,
            });
            
            Act(GetInput(17));
        
            AssertResponseHasError<DataDeletionError>();
        }
        
        [Test]
        public void It_Returns_No_Error_If_The_Operation_Was_Successful()
        {
            DeleteProductDataReturns(new());
            
            Act(GetInput(17));
        
            Assert.IsNull(Output.Error);
        }
        
        private void Act(DeleteProductInput input)
        {
            Output = Service.DeleteProduct(new DeleteProductInput
            {
                Data = input.Data,
            });
        }
        
        private DeleteProductInput GetInput(int? id)
        {
            return new DeleteProductInput
            {
                Data = new()
                {
                    Id = id,
                }
            };
        }
        
        private void DeleteProductDataReturns(DeleteProductDataOutput output)
        {
            DataService
                .Setup(x => x.DeleteProductData(It.IsAny<DeleteProductDataInput>()))
                .Returns(output);
        }
    }

    public class ProductsServiceTests_UpdateProduct : ProductsServiceTests
    {
        [DatapointSource] public UpdateProductInputValidationTestCase[] Values = {
            new("Name must not be empty", x => { x.Data.Name = ""; }),
            new("Valid product names are shorter than 30 characters", x => { x.Data.Name = BuildStringOfLength(30); }),
            new("Description must not be empty", x => { x.Data.Description = ""; }),
            new("Sku must contain only numbers or letters", x => { x.Data.Sku = "*...*...*...*..."; }),
            new("Sku must be exactly 16 characters long", x => { x.Data.Sku = GetValidTestSku() + "morechars"; }),
            new("Sku must not be empty", x => { x.Data.Sku = ""; }),
            new("Id must be present", x => { x.Data.Id = null; }),
            new("No updates were supplied", x => { x.Data = GetUpdateProductInputWithoutUpdates().Data; }),
        };
        
        [Theory]
        public void It_Returns_InputValidationError_When_Input_Is_Invalid(UpdateProductInputValidationTestCase testCase)
        {
            Act(testCase.Input);
            
            AssertInputValidationMessage(testCase.ExpectedMessage);
        }
        
        [DatapointSource] public UpdateInputValidationPassThroughTestCase[] PassThroughValues = {
            new("Object with only valid name should pass", x => { x.Data.Name = "Valid Name"; }),
            new("Object with only valid description should pass", x => { x.Data.Description = "Valid Description"; }),
            new("Object with only valid sku should pass", x => { x.Data.Sku = GetValidTestSku(); }),
            new("Object with only valid available online should pass", x => { x.Data.AvailableOnline = true; }),
            new("Object with only valid available online should pass", x => { x.Data.AvailableOnline = false; }),
        };

        [Theory]
        public void It_Allows_Partial_Input_Through(UpdateInputValidationPassThroughTestCase testCase)
        {
            UpdateProductDataReturns(TestProduct(34));
            
            Act(testCase.Input);
            
            Assert.IsNotInstanceOf<InputValidationError>(Output.Error, testCase.Reason);
        }

        [Test]
        public void It_Returns_A_DataUpdateError_If_DataService_Returns_An_Error()
        {
            var error = new Error("Could not update...");
            
            UpdateProductDataReturns(error);
            
            Act(ValidUpdateProductInput);
        
            AssertResponseHasError<DataUpdateError>();
        }
        
        [Test]
        public void It_Logs_An_Error_If_DataService_Returns_An_Error()
        {
            var error = new Error("Connection Interrupted");
        
            UpdateProductDataReturns(error);
            
            Act(ValidUpdateProductInput);
        
            AssertLogsContain($"Error occurred while updating product - '{error.Message}'", LogLevel.Warning);
        }
        //
        // [Test]
        // public void It_Returns_The_Newly_Updated_Product()
        // {
        //     var updatedProduct = TestProduct(29);
        //     UpdateProductDataReturns(new UpdateProductDataOutput
        //     {
        //         Product = updatedProduct,
        //     });
        //     
        //     Act(ValidUpdateProductInput);
        //
        //     AssertResponseProductEquals(updatedProduct);
        // }

        private void Act(UpdateProductInput input)
        {
            Output = Service.UpdateProduct(input);
        }

        private void UpdateProductDataReturns(Product product)
        {
            UpdateProductDataReturns(new UpdateProductDataOutput
            {
                Product = product
            });
        }

        private void UpdateProductDataReturns(Error error)
        {
            UpdateProductDataReturns(new UpdateProductDataOutput
            {
                Error = error,
            });
        }

        private void UpdateProductDataReturns(UpdateProductDataOutput output)
        {
            DataService
                .Setup(x => x.UpdateProductData(It.IsAny<UpdateProductDataInput>()))
                .Returns(output);
        }
    }

    public class ProductsServiceTests_CreateProduct : ProductsServiceTests
    {
        [DatapointSource] public CreateProductInputValidationTestCase[] Values = {
            new("Name must not be empty", x => { x.Data.Name = ""; }),
            new("Valid product names are shorter than 30 characters", x => { x.Data.Name = BuildStringOfLength(30); }),
            new("Name must not be empty", x => { x.Data.Name = null; }),
            new("Description must not be empty", x => { x.Data.Description = null; }),
            new("Description must not be empty", x => { x.Data.Description = ""; }),
            new("Sku must contain only numbers or letters", x => { x.Data.Sku = "*...*...*...*..."; }),
            new("Sku must be exactly 16 characters long", x => { x.Data.Sku = GetValidTestSku() + "morechars"; }),
            new("Sku must not be empty", x => { x.Data.Sku = null; }),
            new("Sku must not be empty", x => { x.Data.Sku = ""; }),
        };
        
        [Theory]
        public void It_Returns_InputValidationError_When_Input_Is_Invalid(CreateProductInputValidationTestCase testCase)
        {
            Act(testCase.Input);
            
            AssertInputValidationMessage(testCase.ExpectedMessage);
        }

        [Test]
        public void It_Returns_A_DataCreationError_If_DataService_Returns_An_Error()
        {
            CreateProductDataReturns(new Error());
            
            Act(GetValidCreateProductInput());

            AssertResponseHasError<DataCreationError>();
        }
        
        [Test]
        public void It_Logs_An_Error_If_DataService_Returns_An_Error()
        {
            var error = new Error("Connection Interrupted");
            
            CreateProductDataReturns(error);
            
            Act(GetValidCreateProductInput());

            AssertLogsContain($"Error occurred while creating product - '{error.Message}'", LogLevel.Warning);
        }

        [Test]
        public void It_Returns_The_Newly_Created_Product()
        {
            var createdProduct = TestProduct(12);

            CreateProductDataReturns(createdProduct);
            
            Act(GetValidCreateProductInput());
            
            AssertResponseProductEquals(createdProduct);
        }

        // todo: sanitization -- ie... trimming the values before passing to data service

        private void CreateProductDataReturns(Error error)
        {
            CreateProductDataReturns(new CreateProductDataOutput
            {
                Error = error,
            });
        }

        private void CreateProductDataReturns(Product product)
        {
            CreateProductDataReturns(new CreateProductDataOutput
            {
                Product = product,
            });
        }
        
        private void CreateProductDataReturns(CreateProductDataOutput output)
        {
            DataService
                .Setup(x => x.CreateProductData(It.IsAny<CreateProductDataInput>()))
                .Returns(output);
        }
        

        private void Act(CreateProductInput input)
        {
            Output = Service.CreateProduct(new CreateProductInput
            {
                Data = input.Data
            });
        }
    }

    public class ProductServiceTests_GetProducts : ProductsServiceTests
    {
        [Test]
        public void It_Returns_InputValidationError_If_Name_Is_Empty_String()
        {
            var response = Service.GetProducts(new GetProductsInput
            {
                Name = "",
            });
            
            Assert.IsInstanceOf<InputValidationError>(response.Error);
            Assert.AreEqual("name must not be empty", response.Error.Message);
        }

        [Test]
        public void It_Returns_InputValidationError_If_Name_Longer_Than_30_Characters()
        {
            var tooLongString = "This string is 30 characters..";
            
            Assert.AreEqual(tooLongString.Length, 30);

            var response = Service.GetProducts(new GetProductsInput
            {
                Name = tooLongString,
            });
            
            Assert.IsInstanceOf<InputValidationError>(response.Error);
            Assert.AreEqual("Valid product names are shorter than 30 characters", response.Error.Message);
        }

        [Test]
        public void It_Returns_DataRetrievalError_If_DataService_Returns_An_Error()
        {
            var error = new Error("Something happened");
            GetProductsDataItemsReturns(error);
            
            var response = Service.GetProducts(new GetProductsInput
            {
                Name = "Valid Product Name",
            });
            
            Assert.IsInstanceOf<DataRetrievalError>(response.Error);
            Assert.AreEqual(error.Message, response.Error.Message);
        }

        [Test]
        public void It_Returns_Product_Data_From_Data_Service()
        {
            var productDataItem = new ProductDataItem()
            {
                Id = 99,
                Name = "My product",
                Description = "My Products description",
                Sku = "0123456789abcdef",
                AvailableOnline = false,
            };
            
            GetProductsDataItemsReturns(productDataItem);
            
            var response = Service.GetProducts(new GetProductsInput
            {
                Name = "Valid Product Name",
            });
            
            Assert.IsNull(response.Error);
         
            Assert.AreEqual(1, response.Products.Count);
            var actualProduct = response.Products.First();

            actualProduct.Should().BeEquivalentTo<Product>(new()
            {
                Id = productDataItem.Id,
                Name = productDataItem.Name,
                Description = productDataItem.Description,
                Sku = productDataItem.Sku,
                AvailableOnline = productDataItem.AvailableOnline,
            });
        }
    }

    public class ProductsServiceTests
    {
        protected ProductsService Service;
        protected Mock<IProductsDataService> DataService;
        private Mock<ILogger<ProductsService>> _loggerMock;
        protected ErrorableOutput<Error> Output;
        protected UpdateProductInput ValidUpdateProductInput;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<ProductsService>>();
            DataService = new Mock<IProductsDataService>();
            Service = new ProductsService(DataService.Object, _loggerMock.Object);

            ValidUpdateProductInput = GetBlankUpdateProductInput(x =>
            {
                x.Data.Id = 23;
                x.Data.Name = "A Valid Name";
            });
        }

        

        protected void AssertInputValidationMessage(string expectedMessage)
        {
            Assert.IsInstanceOf<InputValidationError>(Output.Error);
            Assert.AreEqual(expectedMessage, Output.Error.Message);
        }

        protected void AssertPassedInputValidation()
        {
            Assert.IsNotInstanceOf<InputValidationError>(Output.Error);
        }

        protected void GetProductsDataItemsReturns(ProductDataItem productDataItem)
        {
            GetProductDataItemsReturns(new GetProductDataOutput
            {
                Products = new List<ProductDataItem> { productDataItem },
            });
        }

        private void GetProductDataItemsReturns(GetProductDataOutput output)
        {
            DataService
                .Setup(x => x.GetProductData(It.IsAny<GetProductDataInput>()))
                .Returns(output);
        }

        protected void GetProductsDataItemsReturns(Error error)
        {
            GetProductDataItemsReturns(new GetProductDataOutput
            {
                Error = error,
            });
        }

        public class CreateProductInputValidationTestCase
        {
            public readonly string ExpectedMessage;
            public readonly CreateProductInput Input;

            public CreateProductInputValidationTestCase(string expectedMessage, Action<CreateProductInput> action)
            {
                ExpectedMessage = expectedMessage;
                Input = GetValidCreateProductInput(action);
            }
        }

        public class UpdateProductInputValidationTestCase
        {
            public readonly string ExpectedMessage;
            public readonly UpdateProductInput Input;
            
            public UpdateProductInputValidationTestCase(string expectedMessage, Action<UpdateProductInput> action)
            {
                ExpectedMessage = expectedMessage;
                Input = GetBlankUpdateProductInput(x =>
                {
                    x.Data.Id = 78;
                    action(x);
                });
            }
        }
        
        public class UpdateInputValidationPassThroughTestCase
        {
            public readonly string Reason;
            public readonly UpdateProductInput Input;
            
            public UpdateInputValidationPassThroughTestCase(string reason, Action<UpdateProductInput> action)
            {
                Reason = reason;
                Input = GetBlankUpdateProductInput(x =>
                {
                    x.Data.Id = 78;
                    action(x);
                });
            }
        }

        protected void AssertResponseHasError<TError>()
        {
            Assert.IsInstanceOf<TError>(Output.Error);
        }
        
        protected void AssertLogsContain(string message, LogLevel logLevel)
        {
            _loggerMock.Verify(
                x => x.Log(
                    logLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains(message)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(1));
        }
        
        protected void AssertResponseProductEquals(Product product)
        {
            Assert.IsNull(Output.Error);

            Output.Should()
                .BeEquivalentTo(new
                {
                    Product = product,
                });
        }

        protected static CreateProductInput GetValidCreateProductInput(Action<CreateProductInput> action = null)
        {
            var validRequest = new CreateProductInput()
            {
                Data = new PartialProduct
                {
                    Name = "Product 1",
                    Description = "My Good description",
                    Sku = GetValidTestSku(),
                    AvailableOnline = true,
                },
            };

            action?.Invoke(validRequest);

            return validRequest;
        }
        
        protected static UpdateProductInput GetBlankUpdateProductInput(Action<UpdateProductInput> action = null)
        {
            var validRequest = new UpdateProductInput()
            {
                Data = new ProductUpdateSubmission(),
            };

            action?.Invoke(validRequest);

            return validRequest;
        }

        protected static UpdateProductInput GetUpdateProductInputWithoutUpdates()
        {
            return GetBlankUpdateProductInput(x =>
            {
                x.Data.Id = 67;
            });
        }

        protected Product TestProduct(int id)
        {
            return new Product
            {
                Id = id,
            };
        }
    }
}