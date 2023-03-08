using Microsoft.EntityFrameworkCore;
using TodoMinimalApi.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoDatabase"));
//builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

// declare the common route to reuse it
var todoItems = app.MapGroup("/todoitems");

todoItems.MapGet("/", async (TodoDb db) => await db.Todos.ToListAsync())
    .WithOpenApi();

todoItems.MapGet("/complete", async (TodoDb db) => await db.Todos.Where(x => x.IsComplete).ToListAsync());

todoItems.MapGet("/{id}", async (int id, TodoDb db) =>
    await db.Todos.FindAsync(id)
        is { } todo
        ? Results.Ok(todo)
        : Results.NotFound());

todoItems.MapPost("/", async (TodoItemDTO todoItemDTO, TodoDb db) =>
{
    var todoItem = new Todo
    {
        IsComplete = todoItemDTO.IsComplete,
        Name = todoItemDTO.Name
    };
    db.Todos.Add(todoItem);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/{todoItem.Id}", todoItem);
});

todoItems.MapPut("/{id}", async (int id, TodoItemDTO inputTodo, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.Name = inputTodo.Name;
    todo.IsComplete = inputTodo.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

todoItems.MapDelete("/{id}", DeleteTodoItem);


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.Run();

static async Task<IResult> DeleteTodoItem(int id, TodoDb db)
{
    if (await db.Todos.FindAsync(id) is { } todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return TypedResults.NotFound();
}