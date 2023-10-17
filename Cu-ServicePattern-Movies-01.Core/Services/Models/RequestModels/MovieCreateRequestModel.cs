using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cu_ServicePattern_Movies_01.Core.Services.Models.RequestModels
{
    public class MovieCreateRequestModel
    {
        public string Title { get; set; }
        public DateTime ReleaseDate { get; set; }
        public decimal Price { get; set; }
        public int CompanyId { get; set; }
        public IFormFile Image { get; set; }
        public IEnumerable<int> ActorIds { get; set; }
        public IEnumerable<int> DirectorIds { get; set; }
    }
}
