/*using DB.Data.Models;
using DB.Data.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DB;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
namespace AGRICORE_ABM_object_relational_mapping.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class FarmsDataToLPController : Controller
    {

        private AgricoreContext _context;
        private IMapper _mapper;


        public FarmsDataToLPController(AgricoreContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        [HttpGet("/get_all_farms")]
        public IEnumerable<Farm> GetAllFarms() => _context.Farms.ToList();


        [HttpGet("/get_farms_data_for_LP")]
        public IEnumerable<ClossingValFarmAssetToLPDTO> GetAllFarmsDataForLPByYear(int year)
        {
            var farmsWithFactors = _context.ClossingValFarmAssets
                .Include(C => C.Farm)
                 .Where(f => f.YearId == year)
                .ToList();
            var dtoList = _mapper.Map<List<ClossingValFarmAssetToLPDTO>>(farmsWithFactors);
            foreach (var dto in dtoList)
            {
                var farmAgricultureProductYearVariable = _context.FarmAgricultureProductYearVariables
                        .Where(v => v.FarmId == dto.FarmId && v.YearId == year)
                        .Select(v => v.UAA)
                        .FirstOrDefault();

                dto.A_0 = farmAgricultureProductYearVariable;

            }

            return dtoList;
        }

        [HttpGet("/get_farms_data_for_SP_CROPS")]
        public List<FarmYearProductionFactorDTO> GetFarmDataCrops(int year)
        {
            var farmsFactors = _context.FarmYearProductionFactors
                               .Include(C => C.AgriculturalProducts)
                               .Where(f => f.YearId == year)
                               .ToList();

            var dtoList = _mapper.Map<List<FarmYearProductionFactorDTO>>(farmsFactors);

            return dtoList;
        }

        [HttpGet("/get_farms_data_for_SP_SUBS")]
        public List<FarmYearProductionFactorDTO> GetFarmDataSubs(int year)
        {
            var farmsFactors = _context.FarmYearDecoupledSubsidies
                                  .Where(f => f.YearId == year)
                               .ToList();

            var dtoList = _mapper.Map<List<FarmYearProductionFactorDTO>>(farmsFactors);

            return dtoList;
        }



        [HttpPost("/add_farms")]
        public IEnumerable<Farm> AddFarms(List<Farm> farms)
        {
            foreach (var farm in farms)
            {
                _context.Farms.Add(farm);
            }
            _context.SaveChanges();
            return farms.ToList();
        }

        [HttpPost("/add_years")]
        public IEnumerable<Year> AddYears(List<Year> Years)
        {
            foreach (var year in Years)
            {
                _context.Years.Add(year);
            }
            _context.SaveChanges();
            return Years.ToList();
        }

        [HttpPost("/add_clossing_values_farm_year")]
        public ActionResult<List<ClossingValFarmValue>> AddClossingValuesFarmYear(List<ClossingValFarmAssetDto> dtoList)
        {
            try
            {
                foreach (var value in dtoList)
                {
                    var entity = _mapper.Map<ClossingValFarmValue>(value);

                    // Check if there is already data for the farm and year
                    var existingEntity = _context.ClossingValFarmAssets
                        .SingleOrDefault(c => c.FarmId == entity.FarmId && c.YearId == entity.YearId);

                    if (existingEntity != null)
                    {
                        // Return error message
                        return StatusCode(409, "There is already data for that farm and year. You can update but not post new data.");
                    }

                    _context.ClossingValFarmAssets.Add(entity);
                    _context.SaveChanges();
                }

                return Ok(dtoList);
            }
            catch (Exception ex)
            {
                // Handle the exception
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPost("/add_production_factors_per_year_and_farm")]
        public IActionResult AddFarmYearProductionFactor(FarmYearProductionFactorDTO farmYearProductionFactorDto)
        {
            // Map DTO to model
            var farmYearProductionFactor = _mapper.Map<FarmYearProductionFactor>(farmYearProductionFactorDto);

            // Check if record already exists
            var existingRecord = _context.FarmYearProductionFactors.FirstOrDefault(fypf => fypf.YearId == farmYearProductionFactor.YearId && fypf.FarmId == farmYearProductionFactor.FarmId);

            if (existingRecord != null)
            {
                // Record already exists, return a bad request response
                return BadRequest("FarmYearProductionFactor record with same YearId, FarmId already exists. Try with PUT");
            }
            // Check if Farm and Year exist
            var farm = _context.Farms.FirstOrDefault(f => f.Id == farmYearProductionFactor.FarmId);
            if (farm == null)
            {
                return BadRequest($"Farm with id {farmYearProductionFactor.FarmId} does not exist.");
            }

            var year = _context.Years.FirstOrDefault(y => y.Id == farmYearProductionFactor.YearId);
            if (year == null)
            {
                return BadRequest($"Year with id {farmYearProductionFactor.YearId} does not exist.");
            }

            // Add record to database
            _context.FarmYearProductionFactors.Add(farmYearProductionFactor);
            _context.SaveChanges();

            // Map model back to DTO and return
            return Ok(farmYearProductionFactorDto);
        }


        [HttpPost("/save_response_data_from_LP")]
        public IActionResult SaveResponseFromLP([FromBody] List<ValueFromLPDTO> result, [FromQuery] int year)
        {
            try
            {
                foreach (var dto in result)
                {
                    var entity = _mapper.Map<ValueFromLP>(dto);
                    _context.ValuesFromLP.Add(entity);
                }

                _context.SaveChanges();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(500, new { error = "Failed to save data from LP to database" });
            }
        }

        *//* This method is now broken as the entities properties are not properly added *//*
        [HttpPut("/update_clossing_values_farm_year")]
        public ActionResult<List<ClossingValFarmValue>> UpdateClossingValuesFarmYear(List<ClossingValFarmAssetDto> dtoList)
        {
            try
            {
                foreach (var value in dtoList)
                {
                    var entity = _context.ClossingValFarmAssets
                        .SingleOrDefault(c => c.FarmId == value.FarmId && c.YearId == value.YearId);

                    if (entity != null)
                    {
                        // Update the existing entity with the new values

                        
                        entity.AgriculturalLand = value.AgriculturalLand;
                        
                        entity.ForestLand = value.ForestLand;
                        entity.FarmBuildings = value.FarmBuildings;
                        entity.MachineryAndEquipment = value.MachineryAndEquipment;
                        entity.IntangibleAssetsTradable = value.IntangibleAssetsTradable;
                        entity.IntangibleAssetsNonTradable = value.IntangibleAssetsNonTradable;
                        entity.OtherNonCurrentAssets = value.OtherNonCurrentAssets;

                        _context.SaveChanges();
                    }
                    else
                    {
                        // Return error message if the entity is not found
                        return StatusCode(404, "No data found for the specified farm and year.");
                    }
                }

                return Ok(dtoList);
            }
            catch (Exception ex)
            {
                // Handle the exception
                return StatusCode(500, ex.Message);
            }
        }

     
           




        }
    }*/



