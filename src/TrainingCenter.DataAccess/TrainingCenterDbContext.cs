namespace TrainingCenter.DataAccess;

using Microsoft.EntityFrameworkCore;
using TrainingCenter.Core.Models;

/// <summary>
/// Контекст EF Core для хранения заявок учебного центра в памяти.
/// </summary>
public class TrainingCenterDbContext : DbContext
{
    /// <summary>
    /// ctor.
    /// </summary>
    /// <param name="options">Настройки провайдера и контекста EF Core.</param>
    public TrainingCenterDbContext(DbContextOptions<TrainingCenterDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Набор сохранённых заявок и состояний доставки.
    /// </summary>
    public DbSet<Lead> Leads => this.Set<Lead>();

    /// <summary>
    /// Настраивает ключ и ограничения свойств сущности заявки.
    /// </summary>
    /// <param name="modelBuilder">Конструктор модели EF Core.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Lead>().HasKey(lead => lead.Id);
        modelBuilder.Entity<Lead>().Property(lead => lead.Name).HasMaxLength(100).IsRequired();
        modelBuilder.Entity<Lead>().Property(lead => lead.Phone).HasMaxLength(32).IsRequired();
        modelBuilder.Entity<Lead>().Property(lead => lead.Email).HasMaxLength(254).IsRequired();
    }
}