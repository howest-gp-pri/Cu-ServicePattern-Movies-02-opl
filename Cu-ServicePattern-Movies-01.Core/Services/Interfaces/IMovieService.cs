using Cu_ServicePattern_Movies_01.Core.Services.Models;
using Cu_ServicePattern_Movies_01.Core.Services.Models.RequestModels;
using Cu_ServicePattern_Movies_01.Core.Services.Models.ResultModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cu_ServicePattern_Movies_01.Core.Services.Interfaces
{
    public interface IMovieService
    {
        Task<ResultModel<Movie>> GetbyIdAsync(int id);
        Task<ResultModel<Movie>> GetAllAsync();
        Task<BaseResultModel> CreateAsync(MovieCreateRequestModel movieCreateRequestModel);
        Task<BaseResultModel> UpdateAsync(MovieUpdateRequestModel movieUpdateRequestModel);
        Task<BaseResultModel> DeleteAsync(int id);
        IQueryable<Movie> GetAll();
        Task<BaseResultModel> SaveChangesAsync();
    }
}
