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
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<Dictionary<bool, int>>> CreateRecord(CreateRecordRequest newRecord)
        {
            var result = await _mediator.Send(new CreateRecordCommand(newRecord));

            if (result.ContainsKey(false))
            {
                return NotFound("This shit works bro!");
            }

            return Ok($"A new record with index: {result.First().Value} has been added.");
        }
    }
}
