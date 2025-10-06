using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla_API.Models.DTO;
using System.Collections;

namespace RoyalVilla_API.Controllers
{
    [Route("api/villa-amenities")]
    [ApiController]
    //[Authorize(Roles = "Customer,Admin")]
    public class VillaAmentiesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public VillaAmentiesController(ApplicationDbContext db, IMapper mapper)
        {
            _db= db;
            _mapper= mapper;
        }


        [HttpGet]
        //[Authorize(Roles ="Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaAmentiesDTO>>),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<VillaAmentiesDTO>>>> GetVillaAmenities()
        {
            var villas = await _db.VillaAmenities.ToListAsync();
            var dtoResponseVillaAmenities = _mapper.Map<List<VillaAmentiesDTO>>(villas);
            var response = ApiResponse<IEnumerable<VillaAmentiesDTO>>.Ok(dtoResponseVillaAmenities, "Villa Amenities retrieved successfully");
            return Ok(response);
        }

        [HttpGet("{id:int}")]
        //[AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<VillaAmentiesDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<VillaAmentiesDTO>>> GetVillaAmenitiesById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return NotFound(ApiResponse<object>.NotFound("VillaAmenities ID must be greater than 0"));
                }

                var villaAmenities = await _db.VillaAmenities.FirstOrDefaultAsync(u => u.Id == id);
                if (villaAmenities == null) 
                {
                    return NotFound(ApiResponse<object>.NotFound($"VillaAmenities with ID {id} was not found"));
                }
                return Ok(ApiResponse<VillaAmentiesDTO>.Ok(_mapper.Map<VillaAmentiesDTO>(villaAmenities), "Records retrieved successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occurred while creating the villa:", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<VillaAmentiesDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<VillaAmentiesDTO>>> CreateVillaAmenities(VillaAmentiesCreateDTO villaAmentiesDTO)
        {
            try
            {
                if (villaAmentiesDTO == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Villa Amenities data is required"));
                }


                var villaExists = await _db.Villa.FirstOrDefaultAsync(u => u.Id== villaAmentiesDTO.VillaId);

                if (villaExists != null)
                {
                    return Conflict(ApiResponse<object>.Conflict($"Villa with the ID '{villaAmentiesDTO.VillaId}'does not exist."));
                }

                VillaAmenities villaAmenities = _mapper.Map<VillaAmenities>(villaAmentiesDTO);
                villaAmenities.CreatedDate = DateTime.Now;
                await _db.VillaAmenities.AddAsync(villaAmenities);
                await _db.SaveChangesAsync();

                var response = ApiResponse<VillaAmentiesDTO>.CreatedAt(_mapper.Map<VillaAmentiesDTO>(villaAmenities), "Villa Amenities created successfully");
                return CreatedAtAction(nameof(CreateVillaAmenities), new {id=villaAmenities.Id},response);

            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occurred while creating the villa amenities:", ex.Message);
                return StatusCode(500,errorResponse);
            }
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<VillaAmentiesDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<VillaAmentiesDTO>>> UpdateVilla(int id, VillaAmentiesUpdateDTO villaAmentiesDTO)
        {
            try
            {
                if (villaAmentiesDTO == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("VillaAmenities data is required"));
                }

                if (id != villaAmentiesDTO.Id)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("VillaAmenities ID in URL does not match VillaAmenities ID in request body"));
                }
                

                var existingVilla = await _db.VillaAmenities.FirstOrDefaultAsync(u => u.Id == id);

                if (existingVilla ==null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"VillaAmenities with ID {id} was not found"));
                }

                var duplicateVilla = await _db.VillaAmenities.FirstOrDefaultAsync(u => u.Name.ToLower() == villaAmentiesDTO.Name.ToLower()
                && u.Id != id);

                if (duplicateVilla != null)
                {
                    return Conflict(ApiResponse<object>.Conflict($"A villa with the name '{villaAmentiesDTO.Name}' already exists"));
                }

                _mapper.Map(villaAmentiesDTO,existingVilla);
                existingVilla.UpdatedDate = DateTime.Now;
                
                await _db.SaveChangesAsync();
                var response = ApiResponse<VillaAmentiesDTO>.Ok(_mapper.Map<VillaAmentiesDTO>(villaAmentiesDTO), "VillaAmenities updated successfully");
                return Ok(villaAmentiesDTO);

            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occurred while creating the villa:", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }


        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteVilla(int id)
        {
            try
            {
                var existingVilla = await _db.VillaAmenities.FirstOrDefaultAsync(u => u.Id == id);

                if (existingVilla == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"VillaAmenities with ID {id} was not found"));
                }

                _db.VillaAmenities.Remove(existingVilla);
                await _db.SaveChangesAsync();

                var response = ApiResponse<object>.NoContent("VillaAmenities deleted successfully");
                return Ok(response);

            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occurred while creating the villa:", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

    }
}
