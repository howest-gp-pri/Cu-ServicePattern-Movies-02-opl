using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cu_ServicePattern_Movies_01.Core.Data;
using Cu_ServicePattern_Movies_01.ViewModels;
using Cu_ServicePattern_Movies_01.Services.Interfaces;
using Cu_ServicePattern_Movies_01.Core;
using Cu_ServicePattern_Movies_01.Core.Interfaces;
using Cu_ServicePattern_Movies_01.Core.Services.Interfaces;
using Cu_ServicePattern_Movies_01.Core.Services.Models.RequestModels;

namespace Cu_ServicePattern_Movies_01.Controllers
{
    public class MoviesController : Controller
    {
        private readonly MovieDbContext _movieDbContext;
        private readonly IFormBuilderService _formBuilderService;
        private readonly IFileService _fileService;
        private readonly IMovieService _movieService;

        public MoviesController(MovieDbContext movieDbContext, IFormBuilderService formBuilderService, IFileService fileService, IMovieService movieService)
        {
            _movieDbContext = movieDbContext;
            _formBuilderService = formBuilderService;
            _fileService = fileService;
            _movieService = movieService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _movieService.GetAllAsync();
            var moviesIndexViewModel = new MoviesIndexViewModel
            {
                Movies = result.Items.Select(m =>
                new MoviesInfoViewModel
                {
                    Id = m.Id,
                    Name = m.Title,
                    Price = m.Price,
                })
            };
            moviesIndexViewModel.PageTitle = "Our movies";
            return View(moviesIndexViewModel);
        }
        [HttpGet]
        public async Task<IActionResult> Info(int id)
        {
            var result = await _movieService.GetbyIdAsync(id);
            if(!result.IsSuccess)
            {
                return NotFound(result.Errors);
            }
            //get the movie from the result.Items list
            var movie = result.Items.First();
            var moviesInfoViewModel = new MoviesInfoViewModel
            {
                Id = movie.Id,
                Name = movie.Title,
                ReleaseDate = movie.ReleaseDate,
                Company = new BaseViewModel 
                {
                    Id  = movie.Company.Id,
                    Name = movie.Company.Name,
                },
                Image = movie.Image,
            };
            moviesInfoViewModel.PageTitle = "Info";
            return View(moviesInfoViewModel);
        }
        //create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var moviesCreateViewModel = new MoviesCreateViewModel
            {
                ReleaseDate = DateTime.Now,
                Companies = await _formBuilderService.GetCompaniesDropDown(),
                Actors = await _formBuilderService.GetActorsDropDown(),
                Directors = await _formBuilderService.GetDirectorsCheckboxes(),
            };
            return View(moviesCreateViewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MoviesCreateViewModel moviesCreateViewModel)
        {
            if(!ModelState.IsValid)
            {
                moviesCreateViewModel.Companies = await _formBuilderService.GetCompaniesDropDown();
                moviesCreateViewModel.Actors = await _formBuilderService.GetActorsDropDown();
               return View(moviesCreateViewModel);
            }
            //call the MovieService
            var directorIds = moviesCreateViewModel.Directors.Where(d => d.IsSelected == true)
                .Select(d => d.Value);
            var movieCreateRequestModel = new MovieCreateRequestModel
            {
                Title = moviesCreateViewModel.Title,
                CompanyId = moviesCreateViewModel.CompanyId,
                Price = moviesCreateViewModel.Price,
                ReleaseDate = moviesCreateViewModel.ReleaseDate,
                ActorIds = moviesCreateViewModel.ActorIds,
                DirectorIds = directorIds,
                Image = moviesCreateViewModel.Image,
            };
            var result = await _movieService.CreateAsync(movieCreateRequestModel);
            if(result.IsSuccess)
            {
                return RedirectToAction("Index");
            }
            return RedirectToAction("Error","Home");
        }
        //update
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var result = await _movieService.GetbyIdAsync(id);
            if (!result.IsSuccess)
            {
                return NotFound();
            }
            var movie = result.Items.First();
            var moviesUpdateViewModel = new MoviesUpdateViewModel
            {
                Id = movie.Id,
                Title = movie.Title,
                Price = movie.Price,
                ReleaseDate = movie.ReleaseDate,
                CompanyId = (int)movie.CompanyId,
                Companies = await _formBuilderService.GetCompaniesDropDown(),
                Actors = await _formBuilderService.GetActorsDropDown(),
                ActorIds = movie.Actors.Select(a => a.Id),
                Directors = await _formBuilderService.GetDirectorsCheckboxes(),
            };
            //check the directors
            var directorIds = movie.Directors.Select(d => d.Id);
            
            foreach(var checkbox in moviesUpdateViewModel.Directors)
            {
                if(directorIds.Contains(checkbox.Value))
                {
                    checkbox.IsSelected = true;
                }
            }
            return View(moviesUpdateViewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(MoviesUpdateViewModel moviesUpdateViewModel)
        {
            if (!ModelState.IsValid)
            {
                moviesUpdateViewModel.Companies = await _formBuilderService.GetCompaniesDropDown();
                moviesUpdateViewModel.Actors = await _formBuilderService.GetActorsDropDown();
                return View(moviesUpdateViewModel);
            }
            var directorIds = moviesUpdateViewModel.Directors
                .Where(d => d.IsSelected == true).Select(d => d.Value);
            var movieUpdateRequestModel = new MovieUpdateRequestModel 
            {
                Id = moviesUpdateViewModel.Id,
                Title = moviesUpdateViewModel.Title,
                CompanyId = moviesUpdateViewModel.CompanyId,
                ActorIds = moviesUpdateViewModel.ActorIds,
                DirectorIds = directorIds,
                Price = moviesUpdateViewModel.Price,
                Image = moviesUpdateViewModel.Image,
                ReleaseDate = moviesUpdateViewModel.ReleaseDate
            };
            var result = await _movieService.UpdateAsync(movieUpdateRequestModel);
            if(result.IsSuccess)
            {
                return RedirectToAction("Index");
            }
            return RedirectToAction("Error","Home");

        }

        [HttpGet]
        public async Task<IActionResult> ConfirmDelete(int id)
        {
            var movie = await _movieDbContext.Movies
                .FirstOrDefaultAsync(m =>  m.Id == id);
            if(movie == null)
            {
                return NotFound();
            }
            var moviesDeleteViewModel = new MoviesDeleteViewModel
            {
                Id = movie.Id,
                Name = movie.Title
            };
            return View(moviesDeleteViewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(MoviesDeleteViewModel moviesDeleteViewModel)
        {
            //delete the movie
            var result = await _movieService.DeleteAsync(moviesDeleteViewModel.Id);
            //save the changes to the database
            //savechanges
            if (result.IsSuccess)
            {
                return RedirectToAction("Index");
            }
            return RedirectToAction("Error","Home");
        }
    }
}
