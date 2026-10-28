namespace Shop.Domain;

/// <summary>استان.</summary>
public class State
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public virtual List<City> Cities { get; set; } = [];
}

/// <summary>شهر.</summary>
public class City
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;

    public int StateId { get; set; }
    public virtual State State { get; set; } = default!;
}
