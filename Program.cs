using ebooks_dotnet7_api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DataContext>(opt => opt.UseInMemoryDatabase("ebooks"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

var ebooks = app.MapGroup("api/ebook");

// TODO: Add more routes
ebooks.MapPost("/", CreateEBookAsync);
ebooks.MapGet("/", GetAllEBookAsync);
ebooks.MapPut("/{id}", UpdateEBookAsync);
ebooks.MapPut("/{id}/change-availability", UpdateEBookAvailabilityAsync);
ebooks.MapPut("/{id}/increment-stock", IncrementEBookStockAsync);
ebooks.MapPost("/purchase", PurchaseEBookAsync);
ebooks.MapDelete("/{id}", DeleteEBookAsync);


app.Run();

// TODO: Add more methods
async Task<IResult> CreateEBookAsync([FromBody] CreateEBookDto createEBookDto, DataContext context)
{
    var eBook = new EBook
    {
        Title = createEBookDto.Title,
        Author = createEBookDto.Author,
        Genre = createEBookDto.Genre,
        Format = createEBookDto.Format,
        Price = createEBookDto.Price,
        IsAvailable = true,
        Stock = 0

    };
    var existBook = await context.EBooks.Where(b => b.Title == eBook.Title || b.Author == eBook.Author).FirstOrDefaultAsync();
    if(existBook is not null){
        return Results.Conflict("Ya existe un libro electronico con esos datos");
    }

    context.EBooks.Add(eBook);
    await context.SaveChangesAsync();

    return Results.Ok(eBook);
}


async Task<IResult> GetAllEBookAsync( DataContext context)
{

    return TypedResults.Ok(await context.EBooks.ToArrayAsync());
}

async Task<IResult> UpdateEBookAsync(int id,[FromBody] UpdateEBookDto updateEBookDto, DataContext context)
{
    var eBook = await context.EBooks.FindAsync(id);

    if(eBook is null){
        return TypedResults.NotFound("No existe un libro electionico con esa id");
    }
    if(updateEBookDto.Title is not null){
        eBook.Title = updateEBookDto.Title;
    }
    if(updateEBookDto.Author is not null){
        eBook.Author = updateEBookDto.Author;
    }
    if(updateEBookDto.Genre is not null){
        eBook.Genre = updateEBookDto.Genre;
    }
    if(updateEBookDto.Format is not null){
        eBook.Format = updateEBookDto.Format;
    }

    eBook.Price = updateEBookDto.Price;
    await context.SaveChangesAsync();
    return TypedResults.Ok("Se actualizo el libro electionico correctamente");
}

async Task<IResult> UpdateEBookAvailabilityAsync(int id, DataContext context)
{
    var eBook = await context.EBooks.FindAsync(id);

    if(eBook is null){
        return TypedResults.NotFound("No existe un libro electionico con esa id");
    }

    if(eBook.IsAvailable == true){
        eBook.IsAvailable = false;
        await context.SaveChangesAsync();
        return TypedResults.Ok("El libro electronico ahora no se encuentra disponible");
    } else {
        eBook.IsAvailable = true;
        await context.SaveChangesAsync();
        return TypedResults.Ok("El libro electronico ahora se encuentra disponible");
        
    }
}

async Task<IResult> IncrementEBookStockAsync(int id,[FromBody] IncrementEBookStockDto incrementEBookStockDto,DataContext context)
{

    var eBook = await context.EBooks.FindAsync(id);

    if(eBook is null){
        return TypedResults.NotFound("No existe un libro electionico con esa id");
    }


    var total = eBook.Stock + incrementEBookStockDto.Stock;
    eBook.Stock = total;

    await context.SaveChangesAsync();
    return TypedResults.Ok("Se incremento el stock del libro electionico correctamente");
}

async Task<IResult> PurchaseEBookAsync([FromBody] PurchaseEBookDto purchaseEBookDto, DataContext context)
{
    var eBook = await context.EBooks.FindAsync(purchaseEBookDto.Id);

    if(eBook is null){
        return TypedResults.NotFound("No existe un libro electionico con esa id");
    }

    if( eBook.Stock < purchaseEBookDto.Quantity){
        return TypedResults.BadRequest("La cantidad se pasa del Stock del libro");
    }
    var total = eBook.Price * purchaseEBookDto.Quantity;
    if( total != purchaseEBookDto.Pay ){
        return TypedResults.BadRequest("El monto difiere del precio total");
    }

    var newStock = eBook.Stock - purchaseEBookDto.Quantity;
    eBook.Stock = newStock;

    await context.SaveChangesAsync();
    return TypedResults.Ok("Se realizo la compra con exito");
}

async Task<IResult> DeleteEBookAsync(int id,DataContext context)
{
    var eBook = await context.EBooks.FindAsync(id);

    if(eBook is null){
        return TypedResults.NotFound("No existe un libro electionico con esa id");
    }

    context.EBooks.Remove(eBook);
    await context.SaveChangesAsync();
    return TypedResults.NoContent();
}
