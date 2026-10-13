namespace TrainingCenter.Tests.StorageTest;

using Microsoft.EntityFrameworkCore;
using TrainingCenter.Core.Enums;
using TrainingCenter.Core.Models;
using TrainingCenter.DataAccess;
using Xunit;

/// <summary>
/// Тест единицы работы <see cref="UnitOfWork"/> и generic-репозитория.
/// </summary>
public class UnitOfWorkTest
{
    /// <summary>
    /// Тест метода <see cref="UnitOfWork.SaveChangesAsync"/>.
    /// Проверяет общий контекст двух типов репозиториев и отсутствие записи до сохранения UoW.
    /// </summary>
    /// <returns>Задача выполнения проверки хранилища.</returns>
    [Fact]
    public async Task SaveChangesWithMultipleRepositoriesTest()
    {
        var options = CreateOptions();
        using var context = new TestDbContext(options);
        using var observer = new TestDbContext(options);
        var unitOfWork = new UnitOfWork(context);
        var leads = unitOfWork.GetRepository<Lead>();
        var notes = unitOfWork.GetRepository<StorageNote>();
        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            Name = "first",
            ClientType = ClientType.Individual,
        };
        var note = new StorageNote
        {
            Id = Guid.NewGuid(),
        };

        Assert.Same(leads, unitOfWork.GetRepository<Lead>());
        await leads.AddAsync(lead, CancellationToken.None);
        await notes.AddAsync(note, CancellationToken.None);
        Assert.Empty(await observer.Leads.AsNoTracking().ToListAsync());
        Assert.Empty(await observer.Set<StorageNote>().AsNoTracking().ToListAsync());

        Assert.Equal(2, await unitOfWork.SaveChangesAsync(CancellationToken.None));
        Assert.Equal(lead.Id, Assert.Single(await observer.Leads.AsNoTracking().ToListAsync()).Id);
        Assert.Equal(note.Id, Assert.Single(await observer.Set<StorageNote>().AsNoTracking().ToListAsync()).Id);
    }

    /// <summary>
    /// Тест метода <see cref="UnitOfWork.SaveChangesAsync"/>.
    /// Проверяет, что обновление попадает в хранилище только после сохранения UoW.
    /// </summary>
    /// <returns>Задача выполнения проверки обновления.</returns>
    [Fact]
    public async Task UpdateBeforeSaveTest()
    {
        var options = CreateOptions();
        using var context = new TestDbContext(options);
        using var observer = new TestDbContext(options);
        var unitOfWork = new UnitOfWork(context);
        var repository = unitOfWork.GetRepository<Lead>();
        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            Name = "first",
        };
        await repository.AddAsync(lead, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        lead.Name = "updated";
        repository.Update(lead);
        Assert.Equal("first", (await observer.Leads.AsNoTracking().SingleAsync()).Name);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);
        Assert.Equal("updated", (await observer.Leads.AsNoTracking().SingleAsync()).Name);
    }

    /// <summary>
    /// Тест метода <see cref="UnitOfWork.SaveChangesAsync"/>.
    /// Проверяет, что отменённое сохранение не записывает подготовленную заявку в хранилище.
    /// </summary>
    /// <returns>Задача выполнения проверки отмены.</returns>
    [Fact]
    public async Task SaveChangesWithCancellationTest()
    {
        var options = CreateOptions();
        using var context = new TestDbContext(options);
        using var observer = new TestDbContext(options);
        var unitOfWork = new UnitOfWork(context);
        await unitOfWork.GetRepository<Lead>().AddAsync(new Lead
        {
            Id = Guid.NewGuid(),
        }, CancellationToken.None);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => unitOfWork.SaveChangesAsync(cancellation.Token));
        Assert.Empty(await observer.Leads.AsNoTracking().ToListAsync());
    }

    private static DbContextOptions<TrainingCenterDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<TrainingCenterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    private class TestDbContext : TrainingCenterDbContext
    {
        /// <summary>
        /// ctor.
        /// </summary>
        /// <param name="options">Настройки отдельного тестового хранилища.</param>
        public TestDbContext(DbContextOptions<TrainingCenterDbContext> options)
            : base(options)
        {
        }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<StorageNote>().HasKey(note => note.Id);
        }
    }

    private class StorageNote
    {
        /// <summary>
        /// Идентификатор тестовой сущности для проверки второго generic-репозитория.
        /// </summary>
        public Guid Id { get; set; }
    }
}