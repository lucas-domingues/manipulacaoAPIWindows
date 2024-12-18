# Step-by-Step Guide: Creating a .NET API with RabbitMQ Producer and a Console App Consumer with SQL Server using Docker

## **Introduction**
In this guide, we will build a simple .NET application consisting of:
1. An API that publishes messages to a RabbitMQ queue.
2. A Console App that consumes these messages and writes them to a SQL Server database.
3. Docker containers to streamline the setup for RabbitMQ and SQL Server.

By the end of this tutorial, you will have a functioning system that demonstrates message-based communication between services.

---

## **Prerequisites**
- **.NET SDK** (6.0 or later) installed.
- **Docker** installed and running.
- Basic knowledge of C#, RabbitMQ, and Docker.

---

## **Step 1: Setting Up RabbitMQ and SQL Server in Docker**

### **1. Create a `docker-compose.yml` File**
In your project directory, create a `docker-compose.yml` file to define RabbitMQ and SQL Server services:

```yaml
default: &default
  restart: always
  logging:
    driver: "json-file"
  environment:
    TZ: "Europe/London"

services:
  rabbitmq:
    container_name: demo-rabbit
    <<: *default
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"

  sqlserver:
    container_name: demo-sqlserver
    <<: *default
    image: mcr.microsoft.com/mssql/server:2019-latest
    environment:
      SA_PASSWORD: "YourStrong!Passw0rd"
      ACCEPT_EULA: "Y"
    ports:
      - "1433:1433"
```

### **2. Start the Containers**
Run the following command to start the RabbitMQ and SQL Server containers:

```bash
docker-compose up -d
```

- RabbitMQ Management UI will be available at `http://localhost:15672` (default credentials: guest/guest).
- SQL Server will be accessible at `localhost,1433`.

---

## **Step 2: Create the .NET API**

### **1. Initialize the API Project**
Run the following commands to create a new .NET API project:

```bash
dotnet new webapi -n RabbitMQProducerAPI
cd RabbitMQProducerAPI
```

### **2. Install Required NuGet Packages**
Add RabbitMQ client library to your project:

```bash
dotnet add package RabbitMQ.Client
```

### **3. Implement the RabbitMQ Producer**
Edit the `Controllers/WeatherForecastController.cs` file or create a new controller for message publishing:

```csharp
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;

[ApiController]
[Route("[controller]")]
public class MessagesController : ControllerBase
{
    [HttpPost]
    public IActionResult SendMessage([FromBody] string message)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: "demo-queue",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "",
                                 routingKey: "demo-queue",
                                 basicProperties: null,
                                 body: body);
        }

        return Ok("Message sent to RabbitMQ");
    }
}
```

### **4. Run the API**
Run the following command to start your API:

```bash
dotnet run
```

Your API will be accessible at `http://localhost:5000`.

---

## **Step 3: Create the Console App Consumer**

### **1. Initialize the Console App**
Create a new console application project:

```bash
dotnet new console -n RabbitMQConsumerApp
cd RabbitMQConsumerApp
```

### **2. Install Required NuGet Packages**
Add RabbitMQ client and Entity Framework Core SQL Server packages:

```bash
dotnet add package RabbitMQ.Client

dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

### **3. Implement the Consumer and Database Logic**
Edit the `Program.cs` file to implement the consumer logic:

```csharp
using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.EntityFrameworkCore;

// Database Context
public class AppDbContext : DbContext
{
    public DbSet<MessageLog> MessageLogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=localhost,1433;Database=RabbitMQDemo;User Id=sa;Password=YourStrong!Passw0rd;");
    }
}

// Entity
public class MessageLog
{
    public int Id { get; set; }
    public string Message { get; set; }
    public DateTime ReceivedAt { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        // Ensure Database is Created
        using (var db = new AppDbContext())
        {
            db.Database.Migrate();
        }

        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: "demo-queue",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Received: {message}");

                // Save to Database
                using (var db = new AppDbContext())
                {
                    db.MessageLogs.Add(new MessageLog { Message = message, ReceivedAt = DateTime.Now });
                    db.SaveChanges();
                }
            };

            channel.BasicConsume(queue: "demo-queue",
                                 autoAck: true,
                                 consumer: consumer);

            Console.WriteLine("Consumer started. Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
```

### **4. Run the Console App**
Start the console app with:

```bash
dotnet run
```

---

## **Step 4: Test the System**
1. Use a tool like Postman or cURL to send a POST request to the API:

```http
POST http://localhost:5000/messages
Content-Type: application/json

"Hello, RabbitMQ!"
```

2. Observe the console app output as it processes the message.
3. Verify the message is stored in the SQL Server database by querying the `MessageLogs` table.

---

## **Conclusion**
In this tutorial, we created a complete message-processing system using RabbitMQ, .NET, and SQL Server. Docker streamlined the setup of RabbitMQ and SQL Server, while .NET provided the tools to build a producer API and a consumer console app. This setup can serve as a foundation for more complex, distributed systems.
