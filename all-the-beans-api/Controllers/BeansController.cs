using all_the_beans_application.Commands;
using all_the_beans_application.Queries;
using all_the_breans_sharedKernal.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace all_the_beans_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BeansController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BeansController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("get-all-records")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<List<BeanDbRecord>>> GetRecordById()
        {
            var records = await _mediator.Send(new GetAllRecordsQuery());

            if (records.Count == 0)
            {
                return NotFound("No records have been found!");
            }

            return Ok(records);
        }

        [HttpGet("get-record-by-index/{index}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<BeanDbRecord>> GetRecordById(int index)
        {
            var post = await _mediator.Send(new GetRecordByIndexQuery(index));

            if (post == null)
            {
                return NotFound("Post not found!");
            }

            return Ok(post);
        }

        [HttpPost("create-record")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<Dictionary<bool, int>>> CreateRecord(CreateRecordRequest newRecord)
        {
            var result = await _mediator.Send(new CreateRecordCommand(newRecord));

            if (result.ContainsKey(false))
            {
                return BadRequest("Failed to add new record. Contact support for more information.");
            }

            return Ok($"A new record with index: {result.First().Value} has been added.");
        }

        [HttpPatch("update-record/{index}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<BeanDbRecord>> UpdateRecord(int index, [FromBody] UpdateRecordRequest recordUpdate)
        {
            var result = await _mediator.Send(new UpdateRecordCommand(index, recordUpdate));

            if (result == null)
            {
                return BadRequest($"Failed to update record with index: {index}. If this error persists, contact support for more information.");
            }

            return Ok(result);
        }

        [HttpDelete("delete-record/{index}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<bool>> DeleteRecord(int index)
        {
            var result = await _mediator.Send(new DeleteRecordCommand(index));

            if (result == false)
            {
                return BadRequest($"A record with index: {index} was not found.");
            }

            return Ok($"Record with index: {index} has been deleted.");
        }
    }
}
