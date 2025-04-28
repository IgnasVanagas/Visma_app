using System;

namespace Visma_app.Models
{
    public class Shortage
    {
        public string Title { get; set; }
        public string Name { get; set; }
        public Room Room { get; set; }
        public Category Category { get; set; }
        public int Priority { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
