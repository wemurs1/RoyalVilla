using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RoyalVilla.DTO;
using RoyalVillaWeb.Models;
using RoyalVillaWeb.Services;
using RoyalVillaWeb.Services.IServices;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RoyalVillaWeb.Controllers
{
    public class VillaController : Controller
    {
        private readonly IVillaService _villaService;
        private readonly IMapper _mapper;

        public VillaController(IVillaService villaService, IMapper mapper )
        {
            _mapper = mapper;
            _villaService = villaService;
        }

        public async Task<IActionResult> Index()
        {
            List<VillaDTO> villaList = new();
            try
            {
                var response = await _villaService.GetAllAsync<ApiResponse<List<VillaDTO>>>();
                if(response!=null && response.Success && response.Data != null)
                {
                    villaList= response.Data;
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
            }

            return View(villaList);
        }

        [Authorize(Roles ="Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VillaCreateDTO createDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(createDTO);
            }

            try
            {
                var response = await _villaService.CreateAsync<ApiResponse<VillaDTO>>(createDTO);
                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = "Villa created successfully";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
            }

            return View(createDTO);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                TempData["error"] = "Invalid villa ID";
                return RedirectToAction(nameof(Index));
            }
            try
            {
                var response = await _villaService.GetAsync<ApiResponse<VillaDTO>>(id);
                if (response != null && response.Success && response.Data != null)
                {
                    return View(_mapper.Map<VillaUpdateDTO>(response.Data));
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
            }


            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VillaUpdateDTO updateDTO)
        {

            try
            {
                var response = await _villaService.UpdateAsync<ApiResponse<object>>(updateDTO);
                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = "Villa updated successfully";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                TempData["error"] = "Invalid villa ID";
                return RedirectToAction(nameof(Index));
            }
            try
            {
                var response = await _villaService.GetAsync<ApiResponse<VillaDTO>>(id);
                if (response != null && response.Success && response.Data != null)
                {
                    return View(response.Data);
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
            }


            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(VillaDTO villaDTO)
        {
            
            try
            {
                var response = await _villaService.DeleteAsync<ApiResponse<object>>(villaDTO.Id);
                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = "Villa deleted successfully";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
