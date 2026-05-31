using api.Constants;
using api.Dtos.FinancialTransaction;
using api.Dtos.User;
using api.QueryObjects;
using api.Services.FinancialTransactions;
using api.Services.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    /// <summary>
    /// Provides API endpoints for managing financial transactions and generating financial transaction reports.
    /// </summary>
    /// <remarks>
    /// All endpoints require authentication and are accessible only to users
    /// who satisfy the <c>NotBanned</c> authorization policy.
    /// </remarks>
    [Authorize(Policy = Policies.NotBanned)]
    [ApiController]
    [Route("api/financial-transactions")]
    public class FinancialTransactionController : ControllerBase
    {
        private readonly IFinancialTransactionService _financialTransactionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionController"/> class.
        /// </summary>
        /// <param name="transactionService">
        /// The service responsible for financial transaction operations.
        /// </param>
        public FinancialTransactionController(IFinancialTransactionService transactionService)
        {
            _financialTransactionService = transactionService;
        }

        /// <summary>
        /// Retrieves a paginated list of financial transactions.
        /// </summary>
        /// <param name="query">
        /// The query parameters used for pagination, sorting, and filtering.
        /// </param>
        /// <returns>
        /// A paginated collection of financial transactions.
        /// </returns>
        /// <response code="200">
        /// Returns the paginated list of financial transactions.
        /// </response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<PagedItems<FinancialTransactionOutputDto>>> GetAll(
            [FromQuery] EntityQuery query)
        {
            var transactions = await _financialTransactionService.GetAllAsync(query);

            return transactions.ToActionResult(this);
        }

        /// <summary>
        /// Retrieves a financial transaction by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the financial transaction.</param>
        /// <returns>
        /// The requested financial transaction.
        /// </returns>
        /// <response code="200">
        /// Returns the requested financial transaction.
        /// </response>
        /// <response code="404">
        /// The financial transaction was not found.
        /// </response>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<FinancialTransactionOutputDto>> GetById([FromRoute] Guid id)
        {
            var result = await _financialTransactionService.GetByIdAsync(id);

            return result.ToActionResult(this);
        }

        /// <summary>
        /// Creates a new financial transaction.
        /// </summary>
        /// <param name="transactionDto">
        /// The data used to create the financial transaction.
        /// </param>
        /// <returns>
        /// The created financial transaction.
        /// </returns>
        /// <response code="201">
        /// The financial transaction was successfully created.
        /// </response>
        /// <response code="404">
        /// The specified category was not found.
        /// </response>
        [HttpPost]
        public async Task<ActionResult<FinancialTransactionOutputDto>> Create(
            [FromBody] FinancialTransactionCreateInputDto transactionDto)
        {
            var result = await _financialTransactionService.CreateAsync(transactionDto);

            if (result.IsError)
            {
                return result.ToActionResult(this);
            }

            return CreatedAtAction(
                actionName: nameof(GetById),
                routeValues: new { id = result.Value.Id },
                value: result.Value);
        }

        /// <summary>
        /// Updates an existing financial transaction.
        /// </summary>
        /// <param name="id">The identifier of the financial transaction to update.</param>
        /// <param name="dto">The updated financial transaction data.</param>
        /// <returns>
        /// The updated financial transaction.
        /// </returns>
        /// <response code="200">
        /// The financial transaction was successfully updated.
        /// </response>
        /// <response code="404">
        /// The financial transaction or category was not found.
        /// </response>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FinancialTransactionOutputDto>> Update(
            [FromRoute] Guid id, [FromBody] FinancialTransactionUpdateInputDto dto)
        {
            var result = await _financialTransactionService.UpdateAsync(id, dto);

            return result.ToActionResult(this);
        }

        /// <summary>
        /// Deletes a financial transaction.
        /// </summary>
        /// <param name="id">The identifier of the financial transaction to delete.</param>
        /// <returns>
        /// A response indicating whether the deletion was successful.
        /// </returns>
        /// <response code="204">
        /// The financial transaction was successfully deleted.
        /// </response>
        /// <response code="404">
        /// The financial transaction was not found.
        /// </response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var result = await _financialTransactionService.DeleteAsync(id);

            return result.ToNoContentResult(this);
        }

        /// <summary>
        /// Generates a grouped financial transaction report.
        /// </summary>
        /// <param name="query">
        /// The report query parameters including grouping strategy,
        /// pagination, and optional date filters.
        /// </param>
        /// <returns>
        /// A paginated grouped financial transaction report.
        /// </returns>
        /// <response code="200">
        /// Returns the generated financial transaction report.
        /// </response>
        [HttpGet("report")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<PagedItems<GroupedReportOutputDto>>> GetReport(
            [FromQuery] ReportQuery query)
        {
            var report = await _financialTransactionService.GetReportAsync(query);

            return report.ToActionResult(this);
        }
    }
}
