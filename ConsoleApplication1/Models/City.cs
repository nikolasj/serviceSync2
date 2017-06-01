using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1.Models
{
    class City
    {
        public Int32 CityId { get; set; }
        public String Slug { get; set; }
        public String Name { get; set; }
        public String Timezone { get; set; }
        public Double Coords { get; set; }
        public String Language { get; set; }
    }
}
