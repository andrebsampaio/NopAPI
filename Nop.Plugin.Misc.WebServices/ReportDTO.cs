using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.WebServices
{
    public class ReportDTO
    {
        public ReportDTO() { }
        public ReportDTO(decimal AllTimeSale)
        {
            this.AllTimeSale = AllTimeSale;
        }

        public decimal AllTimeSale {get;set;}
    }
}
