namespace TodoApp.Server.Features.Todos.Models;

public class TodoViewModel
{
    public int Id { get; set; }
    
    public string Text { get; set; }

    public bool IsFinished { get; set; }
}