using System.Collections.Generic;
using System.Linq;
using Api.Features.Products;
using FluentAssertions;
using NUnit.Framework;

namespace Api.IntegrationTests.Features
{
    public class ProductSearchTests : ProductApiTestCase
    {
        [Test]
        public void It_Returns_Products_By_Name()
        {
            var gel = Product("gel");
            var chains = Product("chains");

            Db.Insert(gel);
            Db.Insert(chains);

            var response = Client.Get("/products?name=gel");

            var body = response.Parse<GetProductsResponseDto>();

            Assert.AreEqual(1, body.Products.Count);

            body.Products[0].Should().BeEquivalentTo(gel);
        }
    }
    
    public class ProductCreateTests : ProductApiTestCase
    {
        [Test]
        public void It_Creates_A_Product()
        {
            Expect(Db).ToHaveEmptyTables("Products");

            var partialProduct = new PartialProduct
            {
                Name = "My Created Product",
                Description = "A very good product",
                AvailableOnline = false,
                Sku = "0123456789abcdef",
            };

            var requestBody = new CreateProductInputDto
            {
                Data = partialProduct,
            };

            var response = Client.Post("/products", requestBody);

            var body = response.Parse<CreateProductResponseDto>();

            Assert.AreNotEqual(default(int), body.Product.Id);
            body.Product.Should().BeEquivalentTo(partialProduct);

            var savedProduct = Db.GetAll<Product>().Single();

            body.Product.Should().BeEquivalentTo(savedProduct);
        }
    }
    
    public class ProductDeleteTests : ProductApiTestCase
    {
        [Test]
        public void It_Deletes_A_Product()
        {
            Expect(Db).ToHaveEmptyTables("Products");

            var socks = Product("socks");

            Db.Insert(socks);
            
            Assert.IsNotEmpty(Db.GetAll<Product>());

            var response = Client.Delete($"/products?id={socks.Id}");

            Expect(response).ToBeOk();
            
            Assert.IsEmpty(Db.GetAll<Product>());
        }
    }
    
    public class ProductUpdateTests : ProductApiTestCase
    {
        [Test]
        public void It_Updates_A_Product_Name()
        {
            Expect(Db).ToHaveEmptyTables("Products");

            var socks = Product("socks");
            Db.Insert(socks);

            Assert.IsNotEmpty(Db.GetAll<Product>());

            var response = Client.Put("/products", new UpdateProductInputDto
            {
                Data = new ProductUpdateSubmission
                {
                    Id = socks.Id,
                    Name = "Used-Socks"
                }
            });

            Expect(response).ToBeOk();

            var product = Db.GetAll<Product>().Single();
            
            Assert.AreEqual("Used-Socks", product.Name);
            Assert.AreEqual(socks.Sku, product.Sku);
            Assert.AreEqual(socks.Description, product.Description);
            Assert.AreEqual(socks.AvailableOnline, product.AvailableOnline);
        }
        
        [Test]
        public void It_Updates_A_Product_Sku()
        {
            Expect(Db).ToHaveEmptyTables("Products");

            var socks = Product("socks");
            Db.Insert(socks);

            Assert.IsNotEmpty(Db.GetAll<Product>());

            var response = Client.Put("/products", new UpdateProductInputDto
            {
                Data = new ProductUpdateSubmission
                {
                    Id = socks.Id,
                    Sku = "0123456789abcdee"
                }
            });

            Expect(response).ToBeOk();

            var product = Db.GetAll<Product>().Single();
            
            Assert.AreNotEqual(socks.Sku, product.Sku);
            Assert.AreEqual(socks.Name, product.Name);
            Assert.AreEqual(socks.Description, product.Description);
            Assert.AreEqual(socks.AvailableOnline, product.AvailableOnline);
        }
    }

    public class ProductApiTestCase : ApiTestCase
    {
        protected Product Product(string name)
        {
            return new Product
            {
                Name = name,
                Description = $"{name} - Description",
                AvailableOnline = true,
                Sku = "0123456789abcdef", // should likely be unique on sku
            };
        }
    }
}