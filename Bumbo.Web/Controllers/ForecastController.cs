﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Bumbo.Data;
using Bumbo.Data.Models;
using Bumbo.Data.Models.Enums;
using Bumbo.Logic.Forecast;
using Bumbo.Web.Models.Forecast;
using Microsoft.EntityFrameworkCore;

namespace Bumbo.Web.Controllers
{
    [Route("branches/{branchId}/{controller}")]
    public class ForecastController : Controller
    {
        private readonly RepositoryWrapper _wrapper;
        private readonly ForecastViewModel _viewModel;

        public ForecastController(RepositoryWrapper wrapper)
        {
            _wrapper = wrapper;
            _viewModel = new ForecastViewModel();
        }

        /// <summary>
        /// Creates the default view for viewing forecasts for 1 week
        /// </summary>
        /// <param name="branchId">Id of the branch</param>
        /// <param name="department">Filter on department</param>
        /// <param name="year">The year of the forecast</param>
        /// <param name="weekNr">The year's week for the forecast</param>
        /// <returns>View with <see cref="ForecastViewModel"/> as parameter</returns>
        [Route("{year}/{weekNr}/{department}")]
        [Route("{year}/{weekNr}")]
        [Route("")]
        public async Task<IActionResult> Index(int branchId, Department? department, int year = -1, int weekNr = -1)
        {
            // Check for default values
            var redirect = false;
            if (year == -1)
            {
                redirect = true;
                year = DateTime.Now.Year;
            }

            if (weekNr == -1)
            {
                redirect = true;
                weekNr = DateLogic.GetWeekNumber(DateTime.Now);
            } 
            // Check if week is not in current year
            else if (weekNr <= 0)
            {
                redirect = true;
                weekNr = 52;
                year -= 1;
            } else if (weekNr > 52)
            {
                redirect = true;
                weekNr = 1;
                year += 1;
            }

            if (redirect) return RedirectToAction("Index", "Forecast", new { branchId, year, weekNr });

            // Define viewmodel variables
            _viewModel.Branch = await _wrapper.Branch.Get(b => b.Id == branchId);
            _viewModel.Department = department;
            
            var firstDayOfWeek = ISOWeek.ToDateTime(year, weekNr, DayOfWeek.Monday);
            
            _viewModel.Forecasts = await _wrapper.Forecast.GetAll(
                f => f.BranchId == branchId,
                f => f.Department == department || department == null,
                // Week filter
                f => f.Date >= firstDayOfWeek,
                f => f.Date < firstDayOfWeek.AddDays(7)
            );

            _viewModel.WeekNr = weekNr;
            _viewModel.Year = year;

            return View(_viewModel);
        }

        //// GET: Forecast/Details/5
        //[Route("{year}/{weekNr}/{department}")]
        //public async Task<IActionResult> Details(int branchId, Department department, int weekNr)
        //{
        //    var forecast = await _wrapper.Forecast.Get(m => m.BranchId == branchId && m.Department == department);
        //    if (forecast == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(forecast);
        //}

        // GET: Forecast/Create
        [Route("create")]
        public async Task<IActionResult> Create(int branchId)
        {
            ViewData["BranchId"] = branchId;
            // ViewData["BranchId"] = new SelectList(await _wrapper.Branch.GetAll(), "Id", "Name");
            return View();
        }

        // POST: Forecast/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("create")]
        public async Task<IActionResult> Create(int branchId, [Bind("Date,Department,WorkingHours")] Forecast forecast)
        {
            if (!ModelState.IsValid) return RedirectToAction();

            forecast.BranchId = branchId;
            // De return value van forecast is nodig voor de Redirect to action methode.
            /*var forecast =*/ await _wrapper.Forecast.Add(forecast);
            
            return RedirectToAction("Index",
            new {
                branchId = forecast.BranchId,
                year = forecast.Date.Year,
                weekNr = DateLogic.GetWeekNumber(forecast.Date)
            });
        }
        
        // public virtual IActionResult Edit(int id)
        // {
        //     var model = _repository.Get(id);
        //     if (model == null) return NotFound();
        //
        //     return View(model);
        // }
        //
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public virtual IActionResult Edit(int id, [FromForm] Forecast model)
        // {
        //     if (id != model.Id) return NotFound();
        //     if (!ModelState.IsValid) return View(model);
        //
        //     try
        //     {
        //         _repository.Update(model);
        //         _repository.Save();
        //     }
        //     catch (DbUpdateConcurrencyException)
        //     {
        //         if (!ModelExists(model.Id)) return NotFound();
        //
        //         throw;
        //     }
        //     return RedirectToAction(nameof(Index));
        // }
        //
        // public virtual IActionResult Delete(int id)
        // {
        //     var model = _repository.Get(id);
        //     if (model == null) return NotFound();
        //
        //     return View(model);
        // }
        //
        // [HttpPost, ActionName("Delete")]
        // [ValidateAntiForgeryToken]
        // public virtual IActionResult DeleteConfirmed(int id)
        // {
        //     var model = _repository.Get(id);
        //     _repository.Delete(model);
        //     _repository.Save();
        //     return RedirectToAction(nameof(Index));
        // }
    }
}
