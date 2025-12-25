using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.ViewModels;
public class MessageViewModel
{
    public int Id { get; set; }
    public string? Subject { get; set; }
    public string? Description { get; set; }
    public string? PictureId { get; set; }
    public string? UserName { get; set; }
    public int SenderUserId { get; set; }
    public string? CreateDate { get; set; }
    public bool IsSeen { get; set; }
}
