using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Base.DomainClasses;

public class BaseModel
{
    protected BaseModel()
    {
        this.CreateDate = DateTime.Now;
    }
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public DateTime CreateDate { get; set; }
    public bool IsDeleted { get; set; }

}

