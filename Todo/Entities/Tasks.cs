using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Todo.Entities
{
    public class Tasks
    {
        public int id { get; set; }
        public string description { get; set; }
        public bool done { get; set; }
        public int idCategory { get; set; }
    }
}
