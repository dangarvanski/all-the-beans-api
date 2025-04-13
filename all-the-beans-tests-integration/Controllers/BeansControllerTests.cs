using all_the_beans_api.Controllers;
using all_the_beans_application.Queries;
using all_the_breans_infrastructure.Interfaces;
using all_the_breans_infrastructure.Repositories;
using all_the_breans_sharedKernal.Entities;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;

namespace all_the_beans_tests_integration.Controllers;

[TestFixture]
public class BeansControllerTests
{
    private TestServer _server;
    private HttpClient _client;
    private BeansDbRepository _context;
    private SqliteConnection _connection;

    [SetUp]
    public async Task SetUp()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        // Initialize _context for test operations with the shared connection
        _context = new BeansDbRepository(new DbContextOptionsBuilder<BeansDbRepository>()
            .UseSqlite(_connection)
            .EnableSensitiveDataLogging()
            .Options);

        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                Console.WriteLine("Registering services...");
                services.AddDbContext<BeansDbRepository>(options =>
                    options.UseSqlite(_connection).EnableSensitiveDataLogging());
                services.AddScoped<IBeansDbRepository, BeansDbRepository>();
                services.AddControllers()
                    .AddApplicationPart(typeof(BeansController).Assembly)
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.PropertyNamingPolicy = null;
                    });
                services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
                    Assembly.GetExecutingAssembly(),
                    typeof(GetRecordByIndexQuery).Assembly));
                services.AddMemoryCache();
                services.Configure<IpRateLimitOptions>(options =>
                {
                    options.EnableEndpointRateLimiting = true;
                    options.StackBlockedRequests = false;
                    options.RealIpHeader = "X-Real-IP";
                    options.ClientIdHeader = "X-ClientId";
                    options.GeneralRules = new List<RateLimitRule>
                    {
                        new RateLimitRule
                        {
                            Endpoint = "*",
                            Period = "1m",
                            Limit = 100
                        }
                    };
                });
                services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
                services.AddInMemoryRateLimiting();
                Console.WriteLine("Services registered.");
            })
            .Configure(app =>
            {
                app.UseIpRateLimiting();
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            });

        _server = new TestServer(builder);
        _client = _server.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Real-IP", "192.168.1.1");

        // Initialize the database once using the shared connection
        _context.Database.EnsureCreated();
        await _context.UpdateCountAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        if (_context != null)
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
        _connection?.Close();
        _connection?.Dispose();
        _client?.Dispose();
        _server?.Dispose();
    }

    [Test]
    public async Task GetAllRecords_HappyPath_ReturnsRecords()
    {
        // Arrange
        var records = new[]
        {
            new BeanDbRecord
            {
                _id = Guid.NewGuid().ToString(),
                index = 15,
                Name = "NovaBrazilia",
                Description = "It's good.",
                Color = "Dark Roast",
                Country = "Brazil",
                Cost = "$30.99",
                Image = "https://example.com/image.jpg",
                IsBOTD = false
            },
            new BeanDbRecord
            {
                _id = Guid.NewGuid().ToString(),
                index = 16,
                Name = "EthiopiaYirg",
                Description = "Fruity.",
                Color = "Light Roast",
                Country = "Ethiopia",
                Cost = "$25.00",
                Image = "https://example.com/image2.jpg",
                IsBOTD = true
            }
        };
        await _context.InsertNewBeanRecordAsync(records[0], new CancellationToken());
        await _context.InsertNewBeanRecordAsync(records[1], new CancellationToken());
        await _context.SaveChangesAsync();
        Console.WriteLine("Records added: count=2");

        // Act
        var response = await _client.GetAsync("/beans/all-records?page=1&pageSize=10");
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"GET response: {response.StatusCode}, Content: '{responseContent}'");
        List<BeanDbRecord>? result = null;
        if (!string.IsNullOrEmpty(responseContent))
        {
            result = JsonSerializer.Deserialize<List<BeanDbRecord>>(responseContent);
        }

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True, $"Expected 200, got {response.StatusCode}");
        Assert.That(result, Is.Not.Null, "Expected non-null result");
        Assert.That(result.Count, Is.EqualTo(2), "Expected 2 records");
        Assert.That(result.Any(r => r.index == 15 && r.Name == "NovaBrazilia"), "Missing record index=15");
        Assert.That(result.Any(r => r.index == 16 && r.Name == "EthiopiaYirg"), "Missing record index=16");
    }

    [Test]
    public async Task GetAllRecords_UnhappyPath_NoRecords_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/beans/all-records?page=999&pageSize=10");
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"GET response: {response.StatusCode}, Content: '{responseContent}'");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound), $"Expected 404, got {response.StatusCode}");
    }

    [Test]
    public async Task GetRecordByIndex_HappyPath_ReturnsRecord()
    {
        // Arrange
        var record = new BeanDbRecord
        {
            _id = Guid.NewGuid().ToString(),
            index = 15,
            Name = "NovaBrazilia",
            Description = "It's good.",
            Color = "Dark Roast",
            Country = "Brazil",
            Cost = "$30.99",
            Image = "https://example.com/image.jpg",
            IsBOTD = false
        };
        await _context.InsertNewBeanRecordAsync(record, new CancellationToken());
        await _context.SaveChangesAsync();
        Console.WriteLine("Record added: index=15");

        // Act
        var response = await _client.GetAsync("/beans/record-by-index/15");
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"GET response: {response.StatusCode}, Content: '{responseContent}'");
        BeanDbRecord? result = null;
        if (!string.IsNullOrEmpty(responseContent))
        {
            result = JsonSerializer.Deserialize<BeanDbRecord>(responseContent);
        }

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True, $"Expected 200, got {response.StatusCode}");
        Assert.That(result, Is.Not.Null, "Expected non-null record");
        Assert.That(result.index, Is.EqualTo(15), "Index mismatch");
        Assert.That(result.Name, Is.EqualTo("NovaBrazilia"), "Name mismatch");
    }

    [Test]
    public async Task GetRecordByIndex_UnhappyPath_NotFound_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/beans/record-by-index/999");
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"GET response: {response.StatusCode}, Content: '{responseContent}'");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound), $"Expected 404, got {response.StatusCode}");
    }

    [Test]
    public async Task GetBeanOfTheDay_HappyPath_ReturnsRecord()
    {
        // Arrange
        var record = new BeanDbRecord
        {
            _id = Guid.NewGuid().ToString(),
            index = 15,
            Name = "NovaBrazilia",
            Description = "It's good.",
            Color = "Dark Roast",
            Country = "Brazil",
            Cost = "$30.99",
            Image = "https://example.com/image.jpg",
            IsBOTD = true
        };
        await _context.InsertNewBeanRecordAsync(record, new CancellationToken());
        await _context.SaveChangesAsync();
        Console.WriteLine("BOTD added: index=15");

        // Act
        var response = await _client.GetAsync("/beans/bean-of-the-day");
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"GET response: {response.StatusCode}, Content: '{responseContent}'");
        BeanDbRecord? result = null;
        if (!string.IsNullOrEmpty(responseContent))
        {
            result = JsonSerializer.Deserialize<BeanDbRecord>(responseContent);
        }

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True, $"Expected 200, got {response.StatusCode}");
        Assert.That(result, Is.Not.Null, "Expected non-null record");
        Assert.That(result.IsBOTD, Is.True, "Expected IsBOTD=true");
        Assert.That(result.Name, Is.EqualTo("NovaBrazilia"), "Name mismatch");
    }

    [Test]
    public async Task GetBeanOfTheDay_UnhappyPath_NoBOTD_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/beans/bean-of-the-day");
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"GET response: {response.StatusCode}, Content: '{responseContent}'");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound), $"Expected 404, got {response.StatusCode}");
    }

    [Test]
    public async Task CreateRecord_HappyPath_CreatesRecord()
    {
        // Arrange
        var newRecord = new CreateRecordRequest
        {
            Name = "NewBean",
            Description = "Fresh.",
            Color = "Medium Roast",
            Country = "Colombia",
            Cost = "$20.00",
            Image = "https://example.com/new.jpg",
        };

        // Act
        var response = await _client.PostAsJsonAsync("/beans/create-record", newRecord);
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"POST response: {response.StatusCode}, Content: '{responseContent}'");

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True, $"Expected 200, got {response.StatusCode}");
        Assert.That(responseContent, Does.Contain("A new record with index:"), "Expected index confirmation");
        var dbAllRecords = await _context.GetAllRecordsAsync(1, 99);
        var dbRecord = dbAllRecords.FirstOrDefault(b => b.Name == "NewBean");
        Assert.That(dbRecord, Is.Not.Null, "Record not in database");
        Assert.That(dbRecord.Description, Is.EqualTo("Fresh."), "Description mismatch");
    }

    [Test]
    public async Task CreateRecord_UnhappyPath_InvalidData_ReturnsBadRequest()
    {
        // Arrange
        var invalidRecord = new CreateRecordRequest
        {
            Name = string.Empty, // Invalid: empty name
            Description = "Fresh.",
            Color = "Medium Roast",
            Country = string.Empty,
            Cost = string.Empty,
            Image = "https://example.com/new.jpg"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/beans/create-record", invalidRecord);
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"POST response: {response.StatusCode}, Content: '{responseContent}'");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest), $"Expected 400, got {response.StatusCode}");
    }

    [Test]
    public async Task UpdateRecord_HappyPath_ReturnsUpdatedRecord()
    {
        // Arrange
        var record = new BeanDbRecord
        {
            _id = Guid.NewGuid().ToString(),
            index = 15,
            Name = "NovaBrazilia",
            Description = "It's good.",
            Color = "Dark Roast",
            Country = "Brazil",
            Cost = "$30.99",
            Image = "https://example.com/image.jpg",
            IsBOTD = false
        };
        await _context.InsertNewBeanRecordAsync(record, new CancellationToken());
        await _context.SaveChangesAsync();
        Console.WriteLine("Record added: index=15");
        var dbRecordBefore = await _context.GetRecordByIndexAsync(15);
        Console.WriteLine($"Database record before: {dbRecordBefore != null}");

        var updateRequest = new UpdateRecordRequest { Description = "Really good!" };

        // Act
        var response = await _client.PatchAsJsonAsync("/beans/update-record/15", updateRequest);
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"PATCH response: {response.StatusCode}, Content: '{responseContent}'");
        BeanDbRecord? updatedRecord = null;
        if (!string.IsNullOrEmpty(responseContent))
        {
            updatedRecord = JsonSerializer.Deserialize<BeanDbRecord>(responseContent);
        }

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True, $"Expected 200, got {response.StatusCode}");
        Assert.That(updatedRecord, Is.Not.Null, "Expected non-null record after update. If null, UpdateBeanRecordAsync may have failed.");
        Assert.That(updatedRecord.Description, Is.EqualTo("Really good!"), "Description mismatch in response");

        // Verify database
        var dbRecordResponse = await _client.GetAsync("/beans/record-by-index/15");
        var dbRecordResponseContent = await response.Content.ReadAsStringAsync();
        var dbRecord = JsonSerializer.Deserialize<BeanDbRecord>(responseContent);
        Assert.That(dbRecord, Is.Not.Null, "Database record not found");
        Assert.That(dbRecord.Description, Is.EqualTo("Really good!"), "Database description mismatch");
    }

    [Test]
    public async Task UpdateRecord_UnhappyPath_NotFound_ReturnsBadRequest()
    {
        // Arrange
        var updateRequest = new UpdateRecordRequest { Description = "Really good!" };

        // Act
        var response = await _client.PatchAsJsonAsync("/beans/update-record/999", updateRequest);
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"PATCH response: {response.StatusCode}, Content: '{responseContent}'");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest), $"Expected 400, got {response.StatusCode}");
    }

    [Test]
    public async Task DeleteRecord_HappyPath_DeletesRecord()
    {
        // Arrange
        var record = new BeanDbRecord
        {
            _id = Guid.NewGuid().ToString(),
            index = 15,
            Name = "NovaBrazilia",
            Description = "It's good.",
            Color = "Dark Roast",
            Country = "Brazil",
            Cost = "$30.99",
            Image = "https://example.com/image.jpg",
            IsBOTD = false
        };
        await _context.InsertNewBeanRecordAsync(record, new CancellationToken());
        await _context.SaveChangesAsync();
        Console.WriteLine("Record added: index=15");

        // Act
        var response = await _client.DeleteAsync("/beans/delete-record/15");
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"DELETE response: {response.StatusCode}, Content: '{responseContent}'");

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True, $"Expected 200, got {response.StatusCode}");
        Assert.That(responseContent, Does.Contain("Record with index: 15 has been deleted."), "Expected delete confirmation");
        var dbRecord = await _context.GetRecordByIndexAsync(15);
        Assert.That(dbRecord, Is.Null, "Record should be deleted");
    }

    [Test]
    public async Task DeleteRecord_UnhappyPath_NotFound_ReturnsBadRequest()
    {
        // Act
        var response = await _client.DeleteAsync("/beans/delete-record/999");
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"DELETE response: {response.StatusCode}, Content: '{responseContent}'");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest), $"Expected 400, got {response.StatusCode}");
    }

    [Test]
    public async Task GetRecordByIndex_RateLimit_Exceeds100Requests_Fails()
    {
        // Arrange
        var record = new BeanDbRecord
        {
            _id = Guid.NewGuid().ToString(),
            index = 15,
            Name = "NovaBrazilia",
            Description = "It's good.",
            Color = "Dark Roast",
            Country = "Brazil",
            Cost = "$30.99",
            Image = "https://example.com/image.jpg",
            IsBOTD = false
        };
        await _context.InsertNewBeanRecordAsync(record, new CancellationToken());
        await _context.SaveChangesAsync();
        Console.WriteLine("Record added: index=15");

        // Act: Send 100 requests sequentially over 50 seconds, then 1 more
        var responses = new List<HttpResponseMessage>();
        int requestsPerSecond = 2; // 100 requests over 50 seconds
        int totalRequests = 100;
        for (int i = 0; i < totalRequests; i++)
        {
            var response = await _client.GetAsync("/beans/record-by-index/15");
            // Log rate-limiting headers
            Console.WriteLine($"Request {i + 1} - Status: {response.StatusCode}");
            if (response.Headers.Contains("X-Rate-Limit-Limit"))
            {
                Console.WriteLine($"  X-Rate-Limit-Limit: {response.Headers.GetValues("X-Rate-Limit-Limit").FirstOrDefault()}");
            }
            if (response.Headers.Contains("X-Rate-Limit-Remaining"))
            {
                Console.WriteLine($"  X-Rate-Limit-Remaining: {response.Headers.GetValues("X-Rate-Limit-Remaining").FirstOrDefault()}");
            }
            responses.Add(response);
            await Task.Delay(1000 / requestsPerSecond); // Pace requests
        }
        var successCount = responses.Count(r => r.IsSuccessStatusCode);
        Console.WriteLine($"First 100 requests - Successful: {successCount}");

        // Send 101st request
        var lastResponse = await _client.GetAsync("/beans/record-by-index/15");
        var lastContent = await lastResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"101st GET response: {lastResponse.StatusCode}, Content: '{lastContent}'");
        if (lastResponse.Headers.Contains("X-Rate-Limit-Limit"))
        {
            Console.WriteLine($"  X-Rate-Limit-Limit: {lastResponse.Headers.GetValues("X-Rate-Limit-Limit").FirstOrDefault()}");
        }
        if (lastResponse.Headers.Contains("X-Rate-Limit-Remaining"))
        {
            Console.WriteLine($"  X-Rate-Limit-Remaining: {lastResponse.Headers.GetValues("X-Rate-Limit-Remaining").FirstOrDefault()}");
        }

        // Assert
        Assert.That(successCount, Is.EqualTo(100), "Expected all 100 requests to succeed");
        Assert.That(lastResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.TooManyRequests), $"Expected 429 for 101st request, got {lastResponse.StatusCode}");
    }
}
