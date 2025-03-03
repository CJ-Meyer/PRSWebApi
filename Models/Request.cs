using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace PRSWebApi.Models;

[Table("Request")]
public class Request
{
    [Key]
    [Column("RequestID")]
    public int RequestId { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? RequestNumber { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string Description { get; set; } = null!;

    [StringLength(255)]
    [Unicode(false)]
    public string Justification { get; set; } = null!;

    public DateOnly DateNeeded { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string DeliveryMode { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string? Status { get; set; } = "New";

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? Total { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? SubmittedDate { get; set; } = DateTime.UtcNow;
    [StringLength(100)]
    [Unicode(false)]
    public string? ReasonForRejection { get; set; }

    [InverseProperty("Request")]
    public virtual ICollection<LineItem> LineItems { get; set; } = new List<LineItem>();
    [ForeignKey("UserId")]
    [InverseProperty("Requests")]
    
    public User? User { get; set; }
}
