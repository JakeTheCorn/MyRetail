using NUnit.Framework;
using System.Linq;
using Api.Features.Products;


namespace Api.IntegrationTests
{
    public class SmokeTests : ApiTestCase
    {
        [Test]
        public void I_Can_Talk_To_An_Sql_Database()
        {
            var result = Db.Query<int>("select 1 + 1").Single();
            
            Assert.AreEqual(2, result);
        }

        [Test]
        public void Pinging_The_Ping_Endpoint_Returns_Pong()
        {
            var response = Client.Get("/ping");
        
            Expect(response)
                .ToBeOk()
                .And.ToHaveBody("pong");
        }

        [Test]
        public void Pinging_The_DbPing_Endpoint_Returns_If_It_Is_Possible_To_Connect()
        {
            var response = Client.Get("/db-ping");
        
            Expect(response)
                .ToBeOk()
                .And.ToHaveBody(new
                {
                    CanConnect = Db.Query<int>("select 1 + 2").Single() == 3,
                });
        }

        [Test]
        public void Products_Can_Be_Inserted()
        {
            Expect(Db).ToHaveEmptyTables("Products");
        
            var product = new Product
            {
                Name = "MyProduct1",
                Description = "Good Description",
                Sku = "0123456789abcdef",
                AvailableOnline = true,
            };

            Db.Insert(product);
            
            Assert.False(default == product.Id);
            Assert.IsNotEmpty(Db.GetAll<Product>());
        }

        [Test]
        public void Locations_Can_Be_Inserted()
        {
            Expect(Db).ToHaveEmptyTables("Locations");
        
            var location = new Location
            {
                Name = "MyProduct1",
                AddressStreetLine1 = "5110 Norwaldo Ave",
                AddressCity = "Indianapolis",
                AddressState = "IN",
                AddressZip = "46205"
            };

            Db.Insert(location);
            
            Assert.False(default == location.Id);
            Assert.IsNotEmpty(Db.GetAll<Location>());
        }

        [Test]
        public void Product_Location_Junctions_Can_Be_Inserted()
        {
            Expect(Db).ToHaveEmptyTables("Locations", "Products", "ProductLocationJunctions");
            
            var product = new Product
            {
                Name = "MyProduct1",
                Description = "Good Description",
                Sku = "0123456789abcdef",
                AvailableOnline = true,
            };

            var location = new Location
            {
                Name = "MyProduct1",
                AddressStreetLine1 = "5110 Norwaldo Ave",
                AddressCity = "Indianapolis",
                AddressState = "IN",
                AddressZip = "46205"
            };

            Db.Insert(product);
            Db.Insert(location);
            Assert.IsNotEmpty(Db.GetAll<Location>());
            Assert.IsNotEmpty(Db.GetAll<Product>());

            var productLocationJunction = new ProductLocationJunction
            {
                ProductId = product.Id,
                LocationId = location.Id,
            };

            Db.Insert(productLocationJunction);

            Assert.False(productLocationJunction.Id == default);
            Assert.IsNotEmpty(Db.GetAll<ProductLocationJunction>());
        }
    }
}