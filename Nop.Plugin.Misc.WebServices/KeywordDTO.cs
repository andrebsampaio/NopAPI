using Nop.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.WebServices
{
    public class KeywordDTO
    {
        public KeywordDTO() { }
        public KeywordDTO(SearchTermReportLine S)
        {
            this.Keyword = S.Keyword;
            this.Count = S.Count;
        }

        public string Keyword { get; set; }

        public int Count { get; set; }
    }
}
