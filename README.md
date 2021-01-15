# Quartz.NET service for MassTransit

It is a small service `Quartz.NET` integrated with `MassTransit`.  MT uses `Quartz` for e.g. `JobConsumers`, schedule / reccuring messages.

MassTransit scheduler-based, using either `Quartz.NET` or `Hangfire`, where the scheduler runs in a service and schedules messages using a queue.

The service is set up for connection to `MSSQL Server`, but [can be easily used with another database](https://www.quartz-scheduler.net/documentation/quartz-3.x/tutorial/job-stores.html#ado-net-job-store-adojobstore). 

## Quartz management UI
The app also offers integrated ui managements (which can be disabled in `Startup.cs`)
- Quartzmin (avaible under: https://localhost:56601/)
- CrystalQuartz  (avaible under: https://localhost:56601/quartz)

Both solutions are imperfect and contain bugs.  If anyone knows better interfaces, please comment.

# Installation

- Use you're connection strings and credencials for RabbitMQ and Db in `appsettings.json`
- [You can use another Store Provider for Quartz](https://www.quartz-scheduler.net/documentation/quartz-3.x/tutorial/job-stores.html#ado-net-job-store-adojobstore)
- Run the script `create-quartz-tables-db.sql` on the database to create the quatrz tables.
- You can now start the service with e.g. command `dotnet run`