using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;

namespace Countries
{
    [Table ("Countries")]
    class CountriesDB
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public string Alpha2Code { get; set; }
        public int Capital { get; set; }
        public double Area { get; set; }
        public int Population { get; set; }
        public int Region { get; set; }
    }
    [Table ("Regions")]
    class Regions
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
    }
    [Table ("Cities")]
    class Cities
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
    }
}
