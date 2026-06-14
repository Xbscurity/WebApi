using api.Constants;
using api.Dtos.FinancialTransaction;
using api.Queries;
using api.Services.FinancialTransactions;
using api.Services.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    /// <summary>
    /// Provides administrative API endpoints for managing categories.
    /// </summary>
    /// <remarks>
    /// All endpoints require administrator role.
    /// </remarks>
    [Authorize(Roles = Roles.Admin)]
    [ApiController]
    [Route("api/admin/financial-transactions")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class AdminFinancialTransactionController : ControllerBase
    {
        private readonly IFinancialTransactionService _financialTransactionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminFinancialTransactionController"/> class.
        /// </summary>
        /// <param name="transactionService">
        /// The service responsible for financial transaction operations.
        /// </param>
        public AdminFinancialTransactionController(IFinancialTransactionService transactionService)
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
        public async Task<ActionResult<PagedItems<AdminFinancialTransactionOutputDto>>> GetAll(
            [FromQuery] AdminEntityQuery query)
        {
            var transactions = await _financialTransactionService.GetAllForAdminAsync(query);

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
        public async Task<ActionResult<AdminFinancialTransactionOutputDto>> GetById([FromRoute] Guid id)
        {
            var result = await _financialTransactionService.GetByIdForAdminAsync(id);

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
        public async Task<ActionResult<AdminFinancialTransactionOutputDto>> Create(
            [FromBody] AdminFinancialTransactionCreateInputDto transactionDto)
        {
            var result = await _financialTransactionService.CreateForAdminAsync(transactionDto);

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
        public async Task<ActionResult<AdminFinancialTransactionOutputDto>> Update(
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
    }
}
