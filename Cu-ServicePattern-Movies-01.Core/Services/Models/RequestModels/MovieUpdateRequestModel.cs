using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cu_ServicePattern_Movies_01.Core.Services.Models.RequestModels
{
    public class MovieUpdateRequestModel : MovieCreateRequestModel
    {
        public int Id { get; set; }
    }
}
