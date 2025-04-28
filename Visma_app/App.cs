using System;
using Visma_app.Models;
using Visma_app.Services;

namespace Visma_app
{
    public class App
    {
        private readonly ShortageService _service;
        private string _currentUser = "";
        private bool _isAdmin = false;

        public App()
        {
            _service = new ShortageService("shortages.json");
        }

        public void Run()
        {
            Console.WriteLine("Welcome to Visma Resource Manager!");
            Console.Write("Enter your name: ");
            _currentUser = Console.ReadLine() ?? "";

            _isAdmin = _currentUser.Equals("admin", StringComparison.OrdinalIgnoreCase);

            while (true)
            {
                Console.WriteLine("\nCommands: add | list | delete | exit");
                Console.Write("Enter command: ");
                var command = Console.ReadLine()?.ToLower();

                switch (command)
                {
                    case "add":
                        AddShortage();
                        break;
                    case "list":
                        ListShortages();
                        break;
                    case "delete":
                        DeleteShortage();
                        break;
                    case "exit":
                        return;
                    default:
                        Console.WriteLine("Unknown command.");
                        break;
                }
            }
        }

        private void AddShortage()
        {
            Console.Write("Title: ");
            var title = Console.ReadLine() ?? "";

            Console.WriteLine("Room (MeetingRoom / Kitchen / Bathroom): ");
            var roomInput = Console.ReadLine() ?? "";
            if (!Enum.TryParse<Room>(roomInput, true, out var room))
            {
                Console.WriteLine("Invalid room.");
                return;
            }

            Console.WriteLine("Category (Electronics / Food / Other): ");
            var categoryInput = Console.ReadLine() ?? "";
            if (!Enum.TryParse<Category>(categoryInput, true, out var category))
            {
                Console.WriteLine("Invalid category.");
                return;
            }

            Console.Write("Priority (1-10): ");
            if (!int.TryParse(Console.ReadLine(), out int priority) || priority < 1 || priority > 10)
            {
                Console.WriteLine("Invalid priority.");
                return;
            }

            var shortage = new Shortage
            {
                Title = title,
                Name = _currentUser,
                Room = room,
                Category = category,
                Priority = priority,
                CreatedOn = DateTime.Now
            };

            _service.AddShortage(shortage);
        }

        private void ListShortages()
        {
            Console.WriteLine("\n--- Filters ---");
            Console.Write("Title contains (optional): ");
            var titleFilter = Console.ReadLine();

            Console.Write("Category (optional - Electronics / Food / Other): ");
            var categoryFilter = Console.ReadLine();

            Console.Write("Room (optional - MeetingRoom / Kitchen / Bathroom): ");
            var roomFilter = Console.ReadLine();

            Console.Write("Created On - Start date (yyyy-MM-dd) (optional): ");
            var startDateStr = Console.ReadLine();
            DateTime? startDate = DateTime.TryParse(startDateStr, out var start) ? start : null;

            Console.Write("Created On - End date (yyyy-MM-dd) (optional): ");
            var endDateStr = Console.ReadLine();
            DateTime? endDate = DateTime.TryParse(endDateStr, out var end) ? end : null;

            var results = _service.GetShortages(_currentUser, _isAdmin, titleFilter, categoryFilter, roomFilter, startDate, endDate);

            foreach (var s in results)
            {
                Console.WriteLine($"{s.Title} - {s.Room} - {s.Category} - Priority: {s.Priority} - Created by: {s.Name} on {s.CreatedOn}");
            }
        }

        private void DeleteShortage()
        {
            Console.Write("Enter title of the shortage to delete: ");
            var title = Console.ReadLine() ?? "";

            Console.Write("Enter room of the shortage to delete (MeetingRoom / Kitchen / Bathroom): ");
            var room = Console.ReadLine() ?? "";

            _service.DeleteShortage(title, room, _currentUser, _isAdmin);
        }
    }
}
