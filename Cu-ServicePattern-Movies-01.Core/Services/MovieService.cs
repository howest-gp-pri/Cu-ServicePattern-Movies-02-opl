using Cu_ServicePattern_Movies_01.Core.Data;
using Cu_ServicePattern_Movies_01.Core.Interfaces;
using Cu_ServicePattern_Movies_01.Core.Services.Interfaces;
using Cu_ServicePattern_Movies_01.Core.Services.Models;
using Cu_ServicePattern_Movies_01.Core.Services.Models.RequestModels;
using Cu_ServicePattern_Movies_01.Core.Services.Models.ResultModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cu_ServicePattern_Movies_01.Core.Services
{
    public class MovieService : IMovieService
    {
        private readonly MovieDbContext _movieDbContext;
        private readonly IFileService _fileService;

        public MovieService(MovieDbContext movieDbContext, IFileService fileService)
        {
            _movieDbContext = movieDbContext;
            _fileService = fileService;
        }

        public async Task<BaseResultModel> CreateAsync(MovieCreateRequestModel movieCreateRequestModel)
        {
            //perform check
            //check if company exists
            if(!await _movieDbContext.Companies.AnyAsync(c => c.Id == movieCreateRequestModel.CompanyId))
            {
                return new ResultModel<Movie>
                {
                    IsSuccess = false,
                    Errors = new List<string> { "Company not found!" }
                };
            }
            //check if title exists
            //use IQueryable
            if(await GetAll().AnyAsync(m => m.Title.ToUpper() == movieCreateRequestModel.Title.ToUpper()))
            {
                return new ResultModel<Movie>
                {
                    IsSuccess = false,
                    Errors = new List<string> { "Movie exists!" }
                };
            }
            //create the movie
            var movie = new Movie();
            movie.Title = movieCreateRequestModel.Title;
            movie.Price = movieCreateRequestModel.Price;
            movie.ReleaseDate = movieCreateRequestModel.ReleaseDate;
            movie.CompanyId = movieCreateRequestModel.CompanyId;
            //actors
            movie.Actors = await _movieDbContext
                .Actors
                .Where(m => movieCreateRequestModel.ActorIds.Contains(m.Id)).ToListAsync();
            //Directors
            movie.Directors = await _movieDbContext
                .Directors
                .Where(d => movieCreateRequestModel.DirectorIds.Contains(d.Id)).ToListAsync();
            //image
            if (movieCreateRequestModel.Image != null)
            {
                movie.Image = await _fileService.Store(movieCreateRequestModel.Image);
            }
            //add to context
            _movieDbContext.Movies.Add(movie);
            //savechanges
            return await SaveChangesAsync();
        }

        public async Task<BaseResultModel> DeleteAsync(int id)
        {
            var movie = await _movieDbContext.Movies
               .Include(m => m.Company)
               .Include(m => m.Actors)
               .Include(m => m.Directors)
               .FirstOrDefaultAsync(m => m.Id == id);
            if (movie != null)
            {
                //delete the movie image
                if (!String.IsNullOrEmpty(movie.Image))
                {
                    _fileService.Delete(movie.Image);
                }
                _movieDbContext.Movies.Remove(movie);
                //savechanges
                return await SaveChangesAsync();
            }
            return new BaseResultModel 
            { 
                IsSuccess = false,
                Errors = new List<string> { "Movie not found!" }
            };
        }

        public IQueryable<Movie> GetAll()
        {
            return _movieDbContext.Movies.AsQueryable();
        }

        public async Task<ResultModel<Movie>> GetAllAsync()
        {
            return new ResultModel<Movie>
            {
                IsSuccess = true,
                Items = await _movieDbContext.Movies
                .Include(m => m.Company)
                .Include(m => m.Actors)
                .Include(m => m.Directors)
                .ToListAsync(),
            };
        }

        public async Task<ResultModel<Movie>> GetbyIdAsync(int id)
        {
            //check if exists
            var movie = await _movieDbContext.Movies
               .Include(m => m.Company)
               .Include(m => m.Actors)
               .Include(m => m.Directors)
               .FirstOrDefaultAsync(m => m.Id == id);
            if(movie == null)
            {
                return new ResultModel<Movie>
                {
                    IsSuccess = false,
                    Errors = new List<string> { "Movie not found" }
                };
            }
            return new ResultModel<Movie>
            {
                IsSuccess = true,
                Items = new List<Movie> { movie }
            };
        }

        public async Task<BaseResultModel> SaveChangesAsync()
        {
            try
            {
                await _movieDbContext.SaveChangesAsync();
                return new BaseResultModel { IsSuccess = true };
            }
            catch (DbUpdateException dbUpdateException)
            {
                Console.WriteLine(dbUpdateException.Message);
                return new BaseResultModel 
                {
                    IsSuccess = false,
                    Errors = new List<string> { "A unknown error occurred. Please try again later." }
                };
            }
        }

        public async Task<BaseResultModel> UpdateAsync(MovieUpdateRequestModel movieUpdateRequestModel)
        {
            //update
            var movie = await _movieDbContext.Movies
               .Include(m => m.Company)
               .Include(m => m.Actors)
               .Include(m => m.Directors)
               .FirstOrDefaultAsync(m => m.Id == movieUpdateRequestModel.Id);
            if (movie == null)
            {
                return new BaseResultModel 
                {
                    IsSuccess = false,
                    Errors = new List<string>{ "Movie not found" }
                };
            }
            //check if title exists
            if(movieUpdateRequestModel.Title != movie.Title)
            {
                //check if new title exists
                if(await GetAll().AnyAsync(m => m.Title.ToUpper() == movieUpdateRequestModel.Title.ToUpper()))
                {
                    //new title already exists
                    new BaseResultModel
                    {
                        IsSuccess = false,
                        Errors = new List<string> { "Title exists!" }
                    };
                }
            }
            //edit the properties
            movie.Title = movieUpdateRequestModel.Title;
            movie.ReleaseDate = movieUpdateRequestModel.ReleaseDate;
            movie.CompanyId = movieUpdateRequestModel.CompanyId;
            movie.Price = movieUpdateRequestModel.Price;
            //actors
            movie.Actors.Clear();
            movie.Actors = await _movieDbContext
                .Actors
                .Where(m => movieUpdateRequestModel.ActorIds.Contains(m.Id)).ToListAsync();
            //Directors
            movie.Directors.Clear();
            //get the list of the selected directors
            movie.Directors = await _movieDbContext
                .Directors
                .Where(d => movieUpdateRequestModel.DirectorIds.Contains(d.Id)).ToListAsync();
            //image
            if (movieUpdateRequestModel.Image != null)
            {
                if (movie.Image != null)
                {
                    movie.Image = await _fileService.Update(movieUpdateRequestModel.Image, movie.Image);
                }
                else
                {
                    movie.Image = await _fileService.Store(movieUpdateRequestModel.Image);
                }

            }
            //savechanges
            return await SaveChangesAsync();
        }
    }
}