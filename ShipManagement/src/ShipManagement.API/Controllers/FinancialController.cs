using Microsoft.AspNetCore.Mvc;
using ShipManagement.API.Models;
using ShipManagement.API.Services;

namespace ShipManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinancialController : ControllerBase
    {
        private readonly IFinancialService _financialService;
        private readonly ILogger<FinancialController> _logger;

        public FinancialController(IFinancialService financialService, ILogger<FinancialController> logger)
        {
            _financialService = financialService;
            _logger = logger;
        }

        [HttpGet("detail")]
        public async Task<ActionResult<IEnumerable<FinancialReportItem>>> GetFinancialReportDetail([FromQuery] FinancialReportRequest request)
        {
            try
            {
                if (request.ShipId <= 0 || request.Year <= 0 || request.Month <= 0 || request.Month > 12)
                    return BadRequest("Valid ShipId, Year, and Month (1-12) are required");

                var result = await _financialService.GetFinancialReportDetailAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving financial report detail for ship {ShipId}, period {Year}-{Month}", 
                    request.ShipId, request.Year, request.Month);
                return StatusCode(500, "An error occurred while retrieving the financial report");
            }
        }

        [HttpGet("summary")]
        public async Task<ActionResult<IEnumerable<FinancialReportItem>>> GetFinancialReportSummary([FromQuery] FinancialReportRequest request)
        {
            try
            {
                if (request.ShipId <= 0 || request.Year <= 0 || request.Month <= 0 || request.Month > 12)
                    return BadRequest("Valid ShipId, Year, and Month (1-12) are required");

                var result = await _financialService.GetFinancialReportSummaryAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving financial report summary for ship {ShipId}, period {Year}-{Month}", 
                    request.ShipId, request.Year, request.Month);
                return StatusCode(500, "An error occurred while retrieving the financial report summary");
            }
        }
    }
}