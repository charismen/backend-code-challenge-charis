using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShipManagement.API.Exceptions;
using ShipManagement.API.Models;
using ShipManagement.API.Services;

namespace ShipManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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
                if (string.IsNullOrWhiteSpace(request.ShipCode) || request.Year <= 0 || request.Month <= 0 || request.Month > 12)
                    return BadRequest("Valid ShipCode, Year, and Month (1-12) are required");

                var result = await _financialService.GetFinancialReportDetailAsync(request);
                return Ok(result);
            }
            catch (NotFoundException nf)
            {
                _logger.LogWarning(nf, "Ship with code {ShipCode} not found for financial detail request", request.ShipCode);
                return NotFound(nf.Message);
            }
            catch (UnauthorizedAccessException ua)
            {
                _logger.LogWarning(ua, "Unauthorized access");
                return Unauthorized("Unauthorized access");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving financial report detail for ship {ShipCode}, period {Year}-{Month}", 
                    request.ShipCode, request.Year, request.Month);
                return StatusCode(500, "An error occurred while retrieving the financial report");
            }
        }

        [HttpGet("summary")]
        public async Task<ActionResult<IEnumerable<FinancialReportItem>>> GetFinancialReportSummary([FromQuery] FinancialReportRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ShipCode) || request.Year <= 0 || request.Month <= 0 || request.Month > 12)
                    return BadRequest("Valid ShipCode, Year, and Month (1-12) are required");

                var result = await _financialService.GetFinancialReportSummaryAsync(request);
                return Ok(result);
            }
            catch (NotFoundException nf)
            {
                _logger.LogWarning(nf, "Ship with code {ShipCode} not found for financial summary request", request.ShipCode);
                return NotFound(nf.Message);
            }
            catch (UnauthorizedAccessException ua)
            {
                _logger.LogWarning(ua, "Unauthorized access");
                return Unauthorized("Unauthorized access");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving financial report summary for ship {ShipCode}, period {Year}-{Month}", 
                    request.ShipCode, request.Year, request.Month);
                return StatusCode(500, "An error occurred while retrieving the financial report summary");
            }
        }
    }
}
