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
    }
}
