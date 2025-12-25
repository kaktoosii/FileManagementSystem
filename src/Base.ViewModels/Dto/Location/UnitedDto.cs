namespace ViewModels;

public class UnitedDto
{
    public int Id { get; set; }
    [Display(Name="استان")]
    public string Title { get; set; }

    public UnitedDto(int id, string title)
    {
        Id = id;
        Title = title;
    }

}
