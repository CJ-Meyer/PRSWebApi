using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PRSWebApi.Models;

public partial class PRSDbContext : DbContext
{

    public PRSDbContext(DbContextOptions<PRSDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<LineItem> LineItems { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Request> Requests { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vendor> Vendors { get; set; }
}
