using dotnetapp.Controllers;
using dotnetapp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace dotnetapp.Tests.Controllers
{
    [TestFixture]
    public class MenuControllerTests
    {
        private DbContextOptions<ApplicationDbContext> _dbContextOptions;

        [SetUp]
        public void Setup()
        {
            // Configure an in-memory database for testing
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            // Seed the database with sample data
            using (var context = new ApplicationDbContext(_dbContextOptions))
            {
                context.Dishes.AddRange(new List<Dish>
                {
                    new Dish { DishID = 1, Name = "Dish 1", Description="Demo1", Price = 10, AvailableQuantity= 20},
                    new Dish { DishID = 2, Name = "Dish 2", Description="Demo1", Price = 10, AvailableQuantity= 30 },
                    new Dish { DishID = 3, Name = "Dish 3", Description="Demo1", Price = 10, AvailableQuantity= 40 },
                    new Dish { DishID = 4, Name = "Dish 3", Description="Demo1", Price = 10, AvailableQuantity= 10 }
                });
                context.SaveChanges();
            }
        }

        [TearDown]
        public void Cleanup()
        {
            // Clean up the in-memory database after each test
            using (var context = new ApplicationDbContext(_dbContextOptions))
            {
                context.Database.EnsureDeleted();
            }
        }

        [Test]
        public void IndexAction_ReturnsViewResultWithMenu()
        {
            // Arrange
            using (var context = new ApplicationDbContext(_dbContextOptions))
            {
                var menuController = new MenuController(context);

                // Act
                var result = menuController.Index() as ViewResult;

                // Assert
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Model);
                Assert.IsInstanceOf<List<Dish>>(result.Model);
                
            }
        }

        [Test]
        public void menuWith_4()
        {
            // Arrange
            using (var context = new ApplicationDbContext(_dbContextOptions))
            {
                var menuController = new MenuController(context);

                // Act
                var result = menuController.Index() as ViewResult;
                Assert.AreEqual(4, (result.Model as List<Dish>).Count);
            }
        }

        [Test]
        public void CreateGetAction_ReturnsViewResultWithDishes()
        {
            // Arrange
            using (var context = new ApplicationDbContext(_dbContextOptions))
            {
                var bookingController = new BookingController(context);

                // Act
                var result = bookingController.Create() as ViewResult;

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(4, (result.Model as List<Dish>).Count);
            }
        }

         [Test]
        public void CreatePostAction_ReturnsViewResultWithConfirmation_WhenValidBookingData()
        {
            // Arrange
            using (var context = new ApplicationDbContext(_dbContextOptions))
            {
                var bookingController = new BookingController(context);

                // Act
                var result = bookingController.Create(1, 12) as ViewResult;

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual("Confirmation", result.ViewName);
                Assert.AreEqual("Booking successful!", result.ViewData["Message"]);

                // Verify the changes in the database
                var updatedDish = context.Dishes.Find(1);
                Assert.AreEqual(8, updatedDish.AvailableQuantity);

                var booking = context.Bookings.SingleOrDefault();
                Assert.IsNotNull(booking);
                Assert.AreEqual(1, booking.DishID);
                Assert.AreEqual(12, booking.BookedQuantity);
            }
        }
        

        [Test]
        public void CreatePostAction_ReturnsViewResultWithError_WhenDishNotFound()
        {
            // Arrange
            using (var context = new ApplicationDbContext(_dbContextOptions))
            {
                var bookingController = new BookingController(context);

                // Act
                var result = bookingController.Create(5, 2) as ViewResult;

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual("Error", result.ViewName);
                Assert.AreEqual("Dish not found.", result.ViewData["ErrorMessage"]);
            }
        }

        [Test]
        public void CreatePostAction_ReturnsViewResultWithError_WhenInvalidQuantity()
        {
            // Arrange
            using (var context = new ApplicationDbContext(_dbContextOptions))
            {
                var bookingController = new BookingController(context);

                // Act
                var result = bookingController.Create(1, 22) as ViewResult;

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual("Error", result.ViewName);
                Assert.AreEqual("Invalid quantity or dish not available.", result.ViewData["ErrorMessage"]);
            }
        }
        [Test]
        public void CancelPostAction_ReturnsViewResultWithConfirmation_WhenValidBookingId()
        {
            // Arrange
            using (var context = new ApplicationDbContext(_dbContextOptions))
            {
                // Create a booking to cancel
                var booking = new Booking { BookingID = 1, DishID = 2, BookedQuantity = 3 };
                context.Bookings.Add(booking);
                context.SaveChanges();

                var bookingController = new BookingController(context);

                // Act
                var result = bookingController.Cancel(1) as ViewResult;

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual("Confirmation", result.ViewName);
                Assert.AreEqual("Booking cancellation successful!", result.ViewData["Message"]);

                // Verify the changes in the database
                var updatedDish = context.Dishes.Find(2);
                Assert.AreEqual(33, updatedDish.AvailableQuantity);

                var deletedBooking = context.Bookings.Find(1);
                Assert.IsNull(deletedBooking);
            }
        }

        
        [Test]
        public void DishClassExists()
        {
            var dish = new Dish();
        
            Assert.IsNotNull(dish);
        }
        
        [Test]
        public void BookingClassExists()
        {
            var booking = new Booking();
        
            Assert.IsNotNull(booking);
        }
        
        [Test]
        public void ApplicationDbContextContainsDbSetDishProperty()
        {
            using (var context = new ApplicationDbContext(_dbContextOptions))
                    {
            // var context = new ApplicationDbContext();
        
            var propertyInfo = context.GetType().GetProperty("Dishes");
        
            Assert.IsNotNull(propertyInfo);
            Assert.AreEqual(typeof(DbSet<Dish>), propertyInfo.PropertyType);
                    }
        }
        
        [Test]
        public void ApplicationDbContextContainsDbSetBookingProperty()
        {
            using (var context = new ApplicationDbContext(_dbContextOptions))
                    {
            // var context = new ApplicationDbContext();
        
            var propertyInfo = context.GetType().GetProperty("Bookings");
        
            Assert.IsNotNull(propertyInfo);
            Assert.AreEqual(typeof(DbSet<Booking>), propertyInfo.PropertyType);
        }
        }

        [Test]
        public void Booking_Properties_BookingID_ReturnExpectedDataTypes()
        {
            // Arrange
            Booking booking = new Booking();
            Assert.That(booking.BookingID, Is.TypeOf<int>());
        }

        [Test]
        public void Booking_Properties_DishID_ReturnExpectedDataTypes()
        {
            // Arrange
            Booking booking = new Booking();
            Assert.That(booking.DishID, Is.TypeOf<int>());
        }
        [Test]
        public void Booking_Properties_BookedQuantity_ReturnExpectedDataTypes()
        {
            // Arrange
            Booking booking = new Booking();

            Assert.That(booking.BookedQuantity, Is.TypeOf<int>());
        }

        [Test]
        public void Dish_Properties_DishID_ReturnExpectedDataTypes()
        {
            // Arrange
            Dish dish = new Dish();

            Assert.That(dish.DishID, Is.TypeOf<int>());
        }

        [Test]
        public void Dish_Properties_Name_ReturnExpectedDataTypes()
        {
            // Arrange
            Dish dish = new Dish();

            dish.Name="";

            Assert.That(dish.Name, Is.TypeOf<string>());
        }

        [Test]
        public void Dish_Properties_Description_ReturnExpectedDataTypes()
        {
            // Arrange
            Dish dish = new Dish();
            dish.Description="";
            Assert.That(dish.Description, Is.TypeOf<string>());
        }

        [Test]
        public void Dish_Properties_Price_ReturnExpectedDataTypes()
        {
            // Arrange
            Dish dish = new Dish();

            Assert.That(dish.Price, Is.TypeOf<decimal>());
        }

        [Test]
        public void Dish_Properties_AvailableQuantity_ReturnExpectedDataTypes()
        {
            // Arrange
            Dish dish = new Dish();

            Assert.That(dish.AvailableQuantity, Is.TypeOf<int>());
        }

        [Test]
        public void Dish_Properties_DishID_ReturnExpectedValues()
        {
            // Arrange
            int expectedDishID = 1;

            Dish dish = new Dish
            {
                DishID = expectedDishID,
            };

            // Act

            // Assert
            Assert.AreEqual(expectedDishID, dish.DishID);
        }
        [Test]
        public void Dish_Properties_Name_ReturnExpectedValues()
        {
            string expectedName = "Chicken Curry";

            Dish dish = new Dish
            {
                Name = expectedName
            };

            Assert.AreEqual(expectedName, dish.Name);
        }

        [Test]
        public void Dish_Properties_Description_ReturnExpectedValues()
        {
            string expectedDescription = "Delicious chicken curry dish";

            Dish dish = new Dish
            {
                Description = expectedDescription
            };
            Assert.AreEqual(expectedDescription, dish.Description);
        }

        [Test]
        public void Dish_Properties_Price_ReturnExpectedValues()
        {
            decimal expectedPrice = 9.99m;

            Dish dish = new Dish
            {
                Price = expectedPrice
            };
            Assert.AreEqual(expectedPrice, dish.Price);
        }
        [Test]
        public void Dish_Properties_AvailableQuantity_ReturnExpectedValues()
        {
            int expectedAvailableQuantity = 10;

            Dish dish = new Dish
            {
                AvailableQuantity = expectedAvailableQuantity
            };
            Assert.AreEqual(expectedAvailableQuantity, dish.AvailableQuantity);
        }
    }
}