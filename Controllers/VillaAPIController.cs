using System;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using MagicVilla_VillaAPI.Data;

using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_VillaAPI.Controllers
{
	[Route("api/VillaAPI")]
	[ApiController]
	public class VillaAPIController : ControllerBase
	{

		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;

		public VillaAPIController(ApplicationDbContext db, IMapper mapper)
		{
			_db = db;
            _mapper = mapper;
		}

		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK)]

		public async Task<ActionResult<IEnumerable<VillaDTO>>> GetVillas()
		{
			IEnumerable<Villa> villaList = await _db.Villas.ToListAsync();
			return Ok(_mapper.Map<List<VillaDTO>>(villaList));
		}

		[HttpGet("{id:int}", Name ="GetVilla")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]

		public async Task<ActionResult<VillaDTO>> GetVilla(int id)
		{
			if (id == 0)
			{
				return BadRequest();
			}
			var villa =await _db.Villas.FirstOrDefaultAsync(u => u.Id == id);
			if (villa == null)
			{                            
				return NotFound();
			}

            return Ok(_mapper.Map<VillaDTO>(villa));
		}

		[HttpPost]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]

		public async Task<ActionResult<VillaDTO>> CreateVilla([FromBody] VillaCreateDTO createDTO)
		{
			if (await _db.Villas.FirstOrDefaultAsync(u => u.Name.ToLower() == createDTO.Name.ToLower()) !=null)
			{
				ModelState.AddModelError("CustomError", "Villa already exists!");
				return BadRequest(ModelState);
			}

			if (createDTO == null)
			{
				return BadRequest(createDTO);
			}

			Villa model = _mapper.Map<Villa>(createDTO);

 
			await _db.Villas.AddAsync(model);
			await _db.SaveChangesAsync();

			return CreatedAtRoute("GetVilla", new { id = model.Id} , model);
		}

		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[HttpDelete("{id:int}", Name = "DeleteVilla")]

		public async Task<IActionResult> DeleteVilla(int id)
		{
			if (id == 0)
			{
				return BadRequest();
			}
			var villa =await _db.Villas.FirstOrDefaultAsync(u => u.Id == id);
			if (villa == null)
			{
				return NotFound();
			}
			_db.Villas.Remove(villa);
			await _db.SaveChangesAsync();
			return NoContent();
		}

		[HttpPut("{id:int}", Name = "UpdateVilla")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]		
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<IActionResult> UpdateVilla(int id, [FromBody] VillaUpdateDTO updateDTO)
		{
			if (updateDTO == null || id != updateDTO.Id)
			{
				return BadRequest();
			}

			//var villa = _db.Villas.FirstOrDefault(u => u.Id == id);
			//villa.Name = villaDTO.Name;
			//villa.Sqft = villaDTO.Sqft;
			//villa.Occupancy = villaDTO.Occupancy;

			Villa model = _mapper.Map<Villa>(updateDTO);

			//Villa model = new()
			//{
			//	Id = updateDTO.Id,
			//	Name = updateDTO.Name,
			//	Details = updateDTO.Details,
			//	Rate = updateDTO.Rate,
			//	Occupancy = updateDTO.Occupancy,
			//	Sqft = updateDTO.Sqft,
			//	ImageUrl = updateDTO.ImageUrl,
			//	Amenity = updateDTO.Amenity
			//};

			_db.Villas.Update(model);
			await _db.SaveChangesAsync();

			return NoContent();
		}

		[HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]

		public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDTO> patchDTO)
		{
			if (patchDTO == null || id == 0)
			{
				return BadRequest();
			}
			var villa =await _db.Villas.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

			VillaUpdateDTO villaDTO = _mapper.Map<VillaUpdateDTO>(villa);

			
            if (villa == null)
			{
				return BadRequest();
			}
			patchDTO.ApplyTo(villaDTO, ModelState);

			Villa model = _mapper.Map<Villa>(villaDTO);

			
			_db.Villas.Update(model);
			await _db.SaveChangesAsync();

            if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			return NoContent();
		}

	}
}

