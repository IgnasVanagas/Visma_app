using System.Collections.Generic;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Visma_app.Models;
using System.Linq;
using System.IO;

namespace Visma_app.Services
{
    public class ShortageService
    {
        private readonly string _filePath;
        private List<Shortage> _shortages;
        private readonly JsonSerializerOptions _jsonOptions;

        public ShortageService(string filePath)
        {
            _filePath = filePath;
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };
            Load();
        }
        /// <summary>
        /// Adds a new shortage to the system. If a shortage with the same title and room already exists, it checks for priority.
        /// </summary>

        public void AddShortage(Shortage shortage)
        {
            var existing = _shortages.FirstOrDefault(x =>
                x.Title.Equals(shortage.Title, StringComparison.OrdinalIgnoreCase)
                && x.Room == shortage.Room);

            if (existing != null)
            {
                if (shortage.Priority > existing.Priority)
                {
                    Console.WriteLine("Higher priority request found. Overriding...");
                    _shortages.Remove(existing);
                    _shortages.Add(shortage);
                    Save();
                }
                else
                {
                    Console.WriteLine("Shortage already exists with equal or higher priority.");
                }
                return;
            }

            _shortages.Add(shortage);
            Save();
            Console.WriteLine("Shortage added.");
        }
        /// <summary>
        /// Retrieves a list of shortages based on the provided filters. Admins can see all shortages, while regular users can only see their own.
        /// </summary>
        public List<Shortage> GetShortages(string user, bool isAdmin, string? title, string? category, string? room, DateTime? startDate, DateTime? endDate)
        {
            var query = _shortages.AsQueryable();

            if (!isAdmin)
                query = query.Where(x => x.Name.Equals(user, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(title))
                query = query.Where(x => x.Title.Contains(title, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(category) && Enum.TryParse<Category>(category, true, out var parsedCategory))
                query = query.Where(x => x.Category == parsedCategory);

            if (!string.IsNullOrEmpty(room) && Enum.TryParse<Room>(room, true, out var parsedRoom))
                query = query.Where(x => x.Room == parsedRoom);

            if (startDate.HasValue)
                query = query.Where(x => x.CreatedOn >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(x => x.CreatedOn <= endDate.Value);

            return query.OrderByDescending(x => x.Priority).ToList();
        }
        /// <summary>
        /// Deletes a shortage based on the title and room. Only the user who created it or an admin can delete it.
        /// </summary>
        public void DeleteShortage(string title, string room, string user, bool isAdmin)
        {
            if (!Enum.TryParse<Room>(room, true, out var parsedRoom))
            {
                Console.WriteLine("Invalid room specified.");
                return;
            }

            var shortage = _shortages.FirstOrDefault(x =>
                x.Title.Equals(title, StringComparison.OrdinalIgnoreCase)
                && x.Room == parsedRoom);

            if (shortage == null)
            {
                Console.WriteLine("Shortage not found.");
                return;
            }

            if (isAdmin || shortage.Name.Equals(user, StringComparison.OrdinalIgnoreCase))
            {
                _shortages.Remove(shortage);
                Save();
                Console.WriteLine("Shortage deleted.");
            }
            else
            {
                Console.WriteLine("You don't have permission to delete this shortage.");
            }
        }

        private void Load()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath);
                    _shortages = JsonSerializer.Deserialize<List<Shortage>>(json, _jsonOptions) ?? new List<Shortage>();
                }
                else
                {
                    _shortages = new List<Shortage>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load shortages: {ex.Message}");
                _shortages = new List<Shortage>();
            }
        }

        private void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(_shortages, _jsonOptions);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save shortages: {ex.Message}");
            }
        }

    }
}
