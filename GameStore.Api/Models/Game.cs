namespace GameStore.Api.Models;

public class Game
{
    public int Id { get; set; }

    // public string? Name { get; set; } // ? makes the Name nullable;
    public required String Name { get; set; } // any property must be either required or nullable or there must be a default value

    public Genre? Genre { get; set; } // composite property

    public int GenreId { get; set; } // foreign key property // makes the association between Game and Genre required;

    public decimal Price { get; set; }

    public DateOnly ReleaseDate { get; set; } 
}