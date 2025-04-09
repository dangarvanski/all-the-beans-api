using all_the_beans_application.Interfaces;
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
        private IBeansService _beanService;
        private readonly IMediator _mediator;

        public BeansController(IBeansService beansService, IMediator mediator)
        {
            _beanService = beansService;
            _mediator = mediator;
        }

        [HttpGet("get-record-by-id/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<BeanDbRecord>> GetRecordById(int id)
        {
            var post = await _mediator.Send(new GetRecordByIdQuery(id));

            if (post == null)
            {
                return NotFound("Post not found!");
            }

            return Ok(post);
        }
    }
}
