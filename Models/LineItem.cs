using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PRSWebApi.Models;

[Table("LineItem")]
[Index("RequestId", "ProductId", Name = "req_pdt", IsUnique = true)]
public partial class LineItem
{
    [Key]
    [Column("LineItemID")]
    public int LineItemId { get; set; }

    [Column("RequestID")]
    public int? RequestId { get; set; }

    [Column("ProductID")]
    public int? ProductId { get; set; }

    public int Quantity { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("LineItems")]
    public virtual Product? Product { get; set; }

    [ForeignKey("RequestId")]
    [InverseProperty("LineItems")]
    public virtual Request? Request { get; set; }

}
