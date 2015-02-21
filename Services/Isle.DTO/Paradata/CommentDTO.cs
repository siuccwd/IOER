using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Isle.DTO
{
  public class CommentDTO
  {
    public int Id { get; set; }
    public int ParentId { get; set; }
    public string Name { get; set; }
    public string Date { get; set; }
    public string Text { get; set; }
    public string AvatarURL { get; set; }
  }
}
