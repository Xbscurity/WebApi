using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Helpers
{
    public class ReportKey
    {
    public string Category { get; set; } = "No category";
    public int? Month { get; set; }
    public int? Year { get; set; }
    }
}