using System.Data;
using Core.Contracts.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Data.Repositories;

internal abstract class Repository<TRepository>(IServiceProvider serviceProvider) where TRepository : IRepository
{
    protected readonly IDbConnection connection = serviceProvider.GetRequiredService<IDbConnection>();
    protected readonly ILogger<TRepository> logger = serviceProvider.GetRequiredService<ILogger<TRepository>>();
    protected readonly INotificationEventFacade notificationEventFacade = serviceProvider.GetRequiredService<INotificationEventFacade>();
    protected IDbTransaction? transaction;

    protected IDbTransaction BeginTransaction()
    {
        if (this.transaction != null)
        {
            return this.transaction;
        }

        if (!this.connection.State.HasFlag(ConnectionState.Open))
        {
            this.connection.Open();
        }
        transaction = this.connection.BeginTransaction();
        return transaction;
    }

    protected bool SaveChanges()
    {
        this.transaction?.Commit();
        CloseConnection();

        return false;
    }

    protected bool RollbackChanges()
    {
        this.transaction?.Rollback();
        CloseConnection();

        return true;
    }

    protected async Task<bool> SaveChanges(IEvent @event)
    {
        SaveChanges();
        await notificationEventFacade.PublishAsync(@event, CancellationToken.None);
        return true;
    }

    protected void CloseConnection()
    {
        if (this.connection.State.HasFlag(ConnectionState.Open))
        {
            this.connection.Close();
        }
    }
}