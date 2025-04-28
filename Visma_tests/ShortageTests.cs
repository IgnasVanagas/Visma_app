using Visma_app.Models;
using Visma_app.Services;
using Xunit;
using System;
using System.IO;
using System.Linq;

namespace VismaResourceManager.Tests
{
    public class ShortageServiceTests
    {
        private const string TestFilePath = "test_shortages.json";

        private ShortageService CreateService()
        {
            if (File.Exists(TestFilePath))
                File.Delete(TestFilePath);

            return new ShortageService(TestFilePath);
        }

        [Fact]
        public void CanAddAndRetrieveShortage()
        {
            var service = CreateService();
            service.AddShortage(new Shortage
            {
                Title = "Test Shortage",
                Name = "TestUser",
                Room = Room.Kitchen,
                Category = Category.Food,
                Priority = 5,
                CreatedOn = DateTime.Now
            });

            var shortages = service.GetShortages("TestUser", true, null, null, null, null, null);
            Assert.Contains(shortages, s => s.Title == "Test Shortage");
        }

        [Fact]
        public void CannotDuplicateShortageUnlessHigherPriority()
        {
            var service = CreateService();
            service.AddShortage(new Shortage
            {
                Title = "Speaker",
                Name = "User1",
                Room = Room.MeetingRoom,
                Category = Category.Electronics,
                Priority = 3,
                CreatedOn = DateTime.Now
            });

            service.AddShortage(new Shortage
            {
                Title = "Speaker",
                Name = "User1",
                Room = Room.MeetingRoom,
                Category = Category.Electronics,
                Priority = 2,
                CreatedOn = DateTime.Now
            });

            var shortages = service.GetShortages("User1", true, null, null, null, null, null);
            Assert.Single(shortages);
            Assert.Equal(3, shortages.First().Priority);

            service.AddShortage(new Shortage
            {
                Title = "Speaker",
                Name = "User1",
                Room = Room.MeetingRoom,
                Category = Category.Electronics,
                Priority = 5,
                CreatedOn = DateTime.Now
            });

            shortages = service.GetShortages("User1", true, null, null, null, null, null);
            Assert.Single(shortages);
            Assert.Equal(5, shortages.First().Priority);
        }

        [Fact]
        public void AdminCanSeeAllShortages()
        {
            var service = CreateService();
            service.AddShortage(new Shortage
            {
                Title = "Coffee",
                Name = "User1",
                Room = Room.Kitchen,
                Category = Category.Food,
                Priority = 4,
                CreatedOn = DateTime.Now
            });
            service.AddShortage(new Shortage
            {
                Title = "Monitor",
                Name = "User2",
                Room = Room.MeetingRoom,
                Category = Category.Electronics,
                Priority = 7,
                CreatedOn = DateTime.Now
            });

            var shortages = service.GetShortages("admin", true, null, null, null, null, null);
            Assert.Equal(2, shortages.Count);
        }

        [Fact]
        public void NonAdminCanSeeOnlyOwnShortages()
        {
            var service = CreateService();
            service.AddShortage(new Shortage
            {
                Title = "Coffee",
                Name = "User1",
                Room = Room.Kitchen,
                Category = Category.Food,
                Priority = 4,
                CreatedOn = DateTime.Now
            });
            service.AddShortage(new Shortage
            {
                Title = "Monitor",
                Name = "User2",
                Room = Room.MeetingRoom,
                Category = Category.Electronics,
                Priority = 7,
                CreatedOn = DateTime.Now
            });

            var shortages = service.GetShortages("User1", false, null, null, null, null, null);
            Assert.Single(shortages);
            Assert.Equal("User1", shortages.First().Name);
        }

        [Fact]
        public void CanDeleteOwnShortage()
        {
            var service = CreateService();
            service.AddShortage(new Shortage
            {
                Title = "Projector",
                Name = "User1",
                Room = Room.MeetingRoom,
                Category = Category.Electronics,
                Priority = 6,
                CreatedOn = DateTime.Now
            });

            service.DeleteShortage("Projector", "MeetingRoom", "User1", false);
            var shortages = service.GetShortages("User1", true, null, null, null, null, null);
            Assert.Empty(shortages);
        }

        [Fact]
        public void CannotDeleteOthersShortageAsNonAdmin()
        {
            var service = CreateService();
            service.AddShortage(new Shortage
            {
                Title = "Keyboard",
                Name = "User2",
                Room = Room.MeetingRoom,
                Category = Category.Electronics,
                Priority = 8,
                CreatedOn = DateTime.Now
            });

            service.DeleteShortage("Keyboard", "MeetingRoom", "User1", false);
            var shortages = service.GetShortages("User2", true, null, null, null, null, null);
            Assert.Single(shortages);
        }

        [Fact]
        public void AdminCanDeleteAnyShortage()
        {
            var service = CreateService();
            service.AddShortage(new Shortage
            {
                Title = "Keyboard",
                Name = "User2",
                Room = Room.MeetingRoom,
                Category = Category.Electronics,
                Priority = 8,
                CreatedOn = DateTime.Now
            });

            service.DeleteShortage("Keyboard", "MeetingRoom", "admin", true);
            var shortages = service.GetShortages("admin", true, null, null, null, null, null);
            Assert.Empty(shortages);
        }

        [Fact]
        public void FilteringByTitleWorks()
        {
            var service = CreateService();
            service.AddShortage(new Shortage
            {
                Title = "Wireless Speaker",
                Name = "User1",
                Room = Room.MeetingRoom,
                Category = Category.Electronics,
                Priority = 6,
                CreatedOn = DateTime.Now
            });

            var results = service.GetShortages("User1", false, "Speaker", null, null, null, null);
            Assert.Single(results);
        }

        [Fact]
        public void FilteringByDateRangeWorks()
        {
            var service = CreateService();
            var date1 = new DateTime(2023, 1, 15);
            var date2 = new DateTime(2023, 2, 15);
            var date3 = new DateTime(2023, 3, 15);

            service.AddShortage(new Shortage
            {
                Title = "Item1",
                Name = "User1",
                Room = Room.Kitchen,
                Category = Category.Food,
                Priority = 5,
                CreatedOn = date1
            });

            service.AddShortage(new Shortage
            {
                Title = "Item2",
                Name = "User1",
                Room = Room.Bathroom,
                Category = Category.Other,
                Priority = 5,
                CreatedOn = date3
            });

            var results = service.GetShortages("User1", true, null, null, null, new DateTime(2023, 1, 1), new DateTime(2023, 2, 28));
            Assert.Single(results);
            Assert.Equal("Item1", results.First().Title);
        }
    }
}