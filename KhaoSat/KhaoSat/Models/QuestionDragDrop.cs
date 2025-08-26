using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KhaoSat.Models;

[Table("QuestionDragDrop")]
public partial class QuestionDragDrop
{
    [Key]
    public int ItemId { get; set; }

    public int QuestionId { get; set; }

    public string? DraggableText { get; set; }

    public string? DropTarget { get; set; }

    [ForeignKey("QuestionId")]
    [InverseProperty("QuestionDragDrops")]
    public virtual Question Question { get; set; } = null!;
}
