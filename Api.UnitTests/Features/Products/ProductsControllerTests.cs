using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Api.Features.Products;

namespace Api.UnitTests.Features.Products
{
    public class ProductsControllerDeleteTests : ProductsControllerTests
    {
        [Test]
        public void It_Returns_BadRequest_If_Service_Returns_InputValidationError()
        {
            var error = new InputValidationError("Must include the id of product to delete");
            DeleteProductReturns(error);
            
            Act<BadRequestObjectResult>(id: null);
            
            AssertResponse(new
            {
                StatusCode = 400,
                Value = $"Input validation error occurred - '{error.Message}'",
            });
        }

        [Test]
        public void It_Returns_500_If_Service_Returns_DataDeletionError()
        {
            DeleteProductReturns(new DataDeletionError());
            
            var response = Act<StatusCodeResult>(id: 1);
            
            Assert.AreEqual(500, response.StatusCode);
        }
        
        [Test]
        public void It_Returns_404_If_Service_Returns_DataNotFoundError()
        {
            DeleteProductReturns(new DataNotFoundError());

            Act<NotFoundObjectResult>(id: 1);

            AssertResponse(new
            {
                Value = "Product not found",
                StatusCode = 404,
            });
        }

        [Test]
        public void It_Returns_200_If_Product_Successfully_Deleted()
        {
            DeleteProductReturns(new DeleteProductOutput());

            var response = Act<OkResult>(id: 1);
            
            Assert.AreEqual(200, response.StatusCode);
        }

        private TResult Act<TResult>(int? id) where TResult : ActionResult
        {
            Response = (TResult)Controller.DeleteProduct(id: id);
            return (TResult)Response;
        }
    }

    public class ProductsControllerUpdateTests : ProductsControllerTests
    {
        [Test]
        public void It_Returns_BadRequest_If_Service_Returns_InputValidationError()
        {
            var error = new InputValidationError("Must include the id of product to update");
            UpdateProductReturns(error);

            var response = Act<BadRequestObjectResult>(null);

            response.Should().BeEquivalentTo(new
            {
                StatusCode = 400,
                Value = $"Input validation error occurred - '{error.Message}'",
            });
        }
        
        [Test]
        public void It_Returns_NotFound_If_Service_Returns_DataNotFoundError()
        {
            UpdateProductReturns(new DataNotFoundError());

            var response = Act<NotFoundObjectResult>(new());

            response.Should().BeEquivalentTo(new
            {
                StatusCode = 404,
                Value = "Unable to update non existent entity",
            });
        }
        
        [Test]
        public void It_Passes_Request_Model_To_Service()
        {
            var dto = new UpdateProductInputDto
            {
                Data = new ProductUpdateSubmission
                {
                    Id = 1,
                    Name = "Prod1",
                    Description = "Pretty good",
                    Sku = "My-Sku",
                    AvailableOnline = true,
                }
            };
            
            Act<OkObjectResult>(dto);

            VerifyUpdateProductCalledWith(dto);
        }

        [Test]
        public void It_Returns_The_Updated_Product()
        {
            var dto = new UpdateProductInputDto
            {
                Data = new ProductUpdateSubmission
                {
                    Id = 24,
                    Name = "Prod24-Updated",
                },
            };

            var updatedProduct = new Product
            {
                Id = 24,
                Name = dto.Data.Name,
            };
            
            UpdateProductReturns(updatedProduct);
            
            var response = Act<OkObjectResult>(dto);

            Assert.AreEqual(200, response.StatusCode);

            var result = (UpdateProductResponseDto)response.Value;

            result.Product.Should().BeEquivalentTo(updatedProduct);
        }

        private void VerifyUpdateProductCalledWith(UpdateProductInputDto dto)
        {
            Service
                .Verify(
                    x => x.UpdateProduct(It.Is<UpdateProductInput>(actual => AssertEqual(dto, actual))),
                    Times.Exactly(1));
        }
        
        private bool AssertEqual(UpdateProductInput expected, UpdateProductInput actual)
        {
            actual.Should().BeEquivalentTo(expected);
            
            return true;
        }

        private TResult Act<TResult>(UpdateProductInputDto dto) where TResult : ActionResult
        {
            Response = (TResult)Controller.UpdateProduct(dto);
            return (TResult)Response;
        }
    }

    public class ProductsControllerCreateTests : ProductsControllerTests
    {
        [Test]
        public void It_Returns_BadRequest_If_Service_Returns_InputValidationError()
        {
            var error = new InputValidationError("name must not be empty");
            CreateProductReturns(new() { Error = error });

            var response = (BadRequestObjectResult)Controller.CreateProduct(new());

            response.Should().BeEquivalentTo(new
            {
                StatusCode = 400,
                Value = $"Input validation error occurred - '{error.Message}'",
            });
        }
        
        [Test]
        public void It_Passes_RequestModel_To_Service()
        {
            var dto = new CreateProductInputDto
            {
                Data = new PartialProduct
                {
                    Name = "Prod1",
                    Description = "Pretty good",
                    Sku = "My-Sku",
                    AvailableOnline = true,
                }
            };

            Controller.CreateProduct(dto);

            VerifyCreateProductCalledWith(dto);
        }

        [Test]
        public void It_Returns_The_Newly_Created_Product()
        {
            var dto = new CreateProductInputDto
            {
                Data = new PartialProduct
                {
                    Name = "Prod23",
                }
            };

            var createdProduct = new Product
            {
                Id = 23,
                Name = dto.Data.Name,
            };
            
            CreateProductReturns(new CreateProductOutput
            {
                Product = createdProduct,
            });

            var response = (OkObjectResult)Controller.CreateProduct(dto);
            
            Assert.AreEqual(200, response.StatusCode);

            var result = (CreateProductResponseDto)response.Value;

            result.Product.Should().BeEquivalentTo(createdProduct);
        }

        private void VerifyCreateProductCalledWith(CreateProductInputDto dto)
        {
            Service
                .Verify(
                    x => x.CreateProduct(It.Is<CreateProductInput>(actual => AssertEqual(dto, actual))),
                    Times.Exactly(1));
        }
        
        private bool AssertEqual(CreateProductInput expected, CreateProductInput actual)
        {
            actual.Should().BeEquivalentTo(expected);
            
            return true;
        }
    }

    public class ProductsControllerSearchTests : ProductsControllerTests
    {
        [Test]
        public void It_Passes_RequestModel_To_Service()
        {
            var expectedRequest = new GetProductsInput
            {
                Name = "bob",
            };

            Controller.SearchProducts("bob");

            VerifyGetProductsCalledWith(expectedRequest);
        }

        private void VerifyGetProductsCalledWith(GetProductsInput expectedInput)
        {
            Service
                .Verify(
                    x => x.GetProducts(It.Is<GetProductsInput>(actual => actual.Name == expectedInput.Name)),
                    Times.Exactly(1));
        }

        [Test]
        public void It_Returns_BadRequest_If_InputValidationError_Occurred()
        {
            SetGetProductsResponse(new GetProductsOutput
            {
                Error = new InputValidationError("name must not be empty"),
            });

            var response = (BadRequestObjectResult)Controller.SearchProducts("");
            
            Assert.AreEqual(400, response.StatusCode);
            Assert.AreEqual("Input validation error occurred - 'name must not be empty'", response.Value);
        }
        
        [Test]
        public void It_Returns_500_If_DataRetrievalError_Occurred()
        {
            SetGetProductsResponse(new GetProductsOutput
            {
                Error = new DataRetrievalError("Something happened"),
            });

            var response = (StatusCodeResult)Controller.SearchProducts("");
            
            Assert.AreEqual(500, response.StatusCode);
        }

        [Test]
        public void It_Returns_NotFound_If_No_Products_Were_Found()
        {
            SetGetProductsResponse(new GetProductsOutput
            {
                Products = new List<Product>(),
            });

            var response = (NotFoundObjectResult)Controller.SearchProducts("Product1");
            
            Assert.AreEqual(404, response.StatusCode);
            Assert.AreEqual("No products were found", response.Value);
        }

        [Test]
        public void It_Returns_Data_If_Successful()
        {
            SetGetProductsResponse(new GetProductsOutput
            {
                Products = new List<Product>
                {
                    new()
                    {
                        Id = 34,
                        Name = "My Product-34",
                    },
                },
            });

            var response = (OkObjectResult)Controller.SearchProducts("Product1");
            
            Assert.AreEqual(200, response.StatusCode);

            var result = (GetProductsResponseDto)response.Value;

            Assert.AreEqual(1, result.Products.Count);

            result.Products.First().Should().BeEquivalentTo(new
            {
                Id = 34,
                Name = "My Product-34",
            });
        }
    }

    public class ProductsControllerTests
    {
        protected ProductsController Controller;
        protected Mock<IProductsService> Service;
        protected IActionResult Response;

        [SetUp]
        public void SetUp()
        {
            Service = new Mock<IProductsService>();
            SetGetProductsResponse(new GetProductsOutput()
            {
                Products = new List<Product>(),
            });
            CreateProductReturns(new CreateProductOutput
            {
                Product = new(),
            });

            UpdateProductReturns(new UpdateProductOutput()
            {
                Product = new(),
            });

            Controller = new ProductsController(Service.Object);
        }

        protected void SetGetProductsResponse(GetProductsOutput getProductsOutput)
        {
            Service
                .Setup(x => x.GetProducts(It.IsAny<GetProductsInput>()))
                .Returns(getProductsOutput);
        }
        
        protected void CreateProductReturns(CreateProductOutput output)
        {
            Service
                .Setup(x => x.CreateProduct(It.IsAny<CreateProductInput>()))
                .Returns(output);
        }
        
        protected void UpdateProductReturns(UpdateProductOutput output)
        {
            Service
                .Setup(x => x.UpdateProduct(It.IsAny<UpdateProductInput>()))
                .Returns(output);
        }
        
        protected void UpdateProductReturns(Product product)
        {
            UpdateProductReturns(new UpdateProductOutput
            {
                Product = product,
            });
        }
        
        protected void UpdateProductReturns(Error error)
        {
            UpdateProductReturns(new UpdateProductOutput
            {
                Error = error,
            });
        }

        protected void DeleteProductReturns(DeleteProductOutput output)
        {
            Service
                .Setup(x => x.DeleteProduct(It.IsAny<DeleteProductInput>()))
                .Returns(output);
        }

        protected void DeleteProductReturns(Error error)
        {
            DeleteProductReturns(new DeleteProductOutput
            {
                Error = error,
            });
        }

        protected void AssertResponse(object o)
        {
            Response.Should().BeEquivalentTo(o);
        }
    }
}