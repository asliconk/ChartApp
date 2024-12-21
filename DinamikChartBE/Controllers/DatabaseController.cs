using DinamikChartBE.Interfaces;
using DinamikChartBE.Models;
using Microsoft.AspNetCore.Mvc;

namespace DinamikChartBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseController : ControllerBase
    {
        private readonly IDatabaseService _connectionService;

        public DatabaseController(IDatabaseService connectionService)
        {
            _connectionService = connectionService;
        }

        [HttpPost("validateConnection")]
        public IActionResult ValidateConnection([FromBody] ConnectionDetails connectionDetails)
        {
            if (_connectionService.TestAndSaveConnection(connectionDetails, HttpContext.Session))
            {
                return Ok(new { status = true });
            }

            return BadRequest("Unable to connect to the database.");
        }

        [HttpGet("fetchStoredProcedures")]
        public IActionResult FetchStoredProcedures()
        {
            var proceduresList = _connectionService.RetrieveStoredProcedures(HttpContext.Session);
            if (proceduresList == null || proceduresList.Count == 0)
            {
                return StatusCode(500, "Error retrieving stored procedures.");
            }

            return Ok(proceduresList);
        }

        [HttpPost("runStoredProcedure")]
        public IActionResult RunStoredProcedure([FromBody] string procedureIdentifier)
        {
            var executionResult = _connectionService.ExecuteStoredProcedure(procedureIdentifier, HttpContext.Session);
            if (executionResult == null)
            {
                return StatusCode(500, "Error executing stored procedure.");
            }

            return Ok(executionResult);
        }

        [HttpGet("fetchDatabaseViews")]
        public IActionResult FetchDatabaseViews()
        {
            var viewsList = _connectionService.RetrieveViews(HttpContext.Session);
            if (viewsList == null || viewsList.Count == 0)
            {
                return StatusCode(500, "Error retrieving views.");
            }

            return Ok(viewsList);
        }

        [HttpPost("runDatabaseView")]
        public IActionResult RunDatabaseView([FromBody] string viewIdentifier)
        {
            var queryResult = _connectionService.ExecuteViewQuery(viewIdentifier, HttpContext.Session);
            if (queryResult == null)
            {
                return StatusCode(500, "Error executing view query.");
            }

            return Ok(queryResult);
        }
    }
}
