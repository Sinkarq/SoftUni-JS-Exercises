using AspNetCore.Hashids.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Server.Features.Todos.Models;

namespace TodoApp.Server.Features.Todos;

public class TodosController : ApiController
{
    private readonly ITodosService todosService;
    private readonly IMapper mapper;
    
    public TodosController(ITodosService todosService, IMapper mapper)
    {
        this.todosService = todosService;
        this.mapper = mapper;
    }

    [HttpGet]
    [Route("/Todos")]
    public async Task<IActionResult> GetAllTodos() => this.Ok(await this.todosService.GetAllAsync<TodoViewModel>());

    [HttpGet]
    [Route("/Todos/{id:hashids}")]
    public async Task<IActionResult> GetTodoById(
        [FromRoute]
        [ModelBinder(typeof(HashidsModelBinder))]
        int id) =>
        this.Ok(await this.todosService.GetByIdAsync<TodoViewModel>(id));

    [HttpDelete]
    [Route("/Todos/{id:hashids}")]
    public async Task<IActionResult> DeleteTodo(
        [FromRoute]
        [ModelBinder(typeof(HashidsModelBinder))] 
        int id)
    {
        var model = await this.todosService.GetByIdModelAsync(id);

        if (model == null)
        {
            return this.NotFound();
        }
        
        await this.todosService.DeleteAsync(model);

        return this.Ok();
    }

    [HttpPost]
    [Route("/Todos")]
    public async Task<IActionResult> CreateTodo(CreateTodoInputModel inputModel)
    {
        var model = this.mapper.Map<Todo>(inputModel);
        var result = await this.todosService.CreateAsync(model);
        var returnValue = this.mapper.Map<TodoViewModel>(result);

        return new CreatedResult($"/Todos/{returnValue.Id}", returnValue);
    }

    [HttpPut]
    [Route("/Todos/ChangeState/{id:hashids}")]
    public async Task<IActionResult> ChangeTodoState(
        [FromRoute]
        [ModelBinder(typeof(HashidsModelBinder))]
        int id)
    {
        var model = await this.todosService.GetByIdModelAsync(id);

        if (model == null)
        {
            return this.NotFound();
        }

        model.IsFinished = !model.IsFinished;
        await this.todosService.UpdateAsync(model);
        return this.Ok();
    }
}