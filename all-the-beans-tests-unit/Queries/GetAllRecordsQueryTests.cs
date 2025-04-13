using all_the_beans_application.Queries;
using all_the_breans_infrastructure.Interfaces;
using all_the_breans_sharedKernal.Entities;
using AutoFixture.NUnit3;
using Moq;

namespace all_the_beans_tests_unit.Queries;

[TestFixture]
[Parallelizable]
public class GetAllRecordsQueryTests
{
    [Test]
    [AutoData]
    public async Task WhenDataFound_ReturnsCorrectData(List<BeanDbRecord> response)
    {
        // Arange
        var pageNumber = 1;
        var pageSize = 10;
        var repository = new Mock<IBeansDbRepository>();

        repository.Setup(r => r.GetAllRecordsAsync(pageNumber, pageSize))
            .ReturnsAsync(response);

        var query = new GetAllRecordsQuery(pageNumber, pageSize);
        var queryHandler = new GetAllRecordsQueryHandler(repository.Object);

        // Act
        var listWithItemsResult = await queryHandler.Handle(query, It.IsAny<CancellationToken>());

        // Assert
        Assert.That(listWithItemsResult, Is.Not.Empty);
        Assert.That(listWithItemsResult.Count, Is.AtMost(10));
    }

    [Test]
    public async Task WhenNoDataIsFound_ReturnsAnEmptyList()
    {
        // Arange
        List<BeanDbRecord> response = new List<BeanDbRecord>();
        var pageNumber = 1;
        var pageSize = 10;
        var repository = new Mock<IBeansDbRepository>();

        repository.Setup(r => r.GetAllRecordsAsync(pageNumber, pageSize))
            .ReturnsAsync(response);

        var query = new GetAllRecordsQuery(pageNumber, pageSize);
        var queryHandler = new GetAllRecordsQueryHandler(repository.Object);

        // Act
        var listWithItemsResult = await queryHandler.Handle(query, It.IsAny<CancellationToken>());


        // Assert
        Assert.That(listWithItemsResult, Is.Empty);
        Assert.That(listWithItemsResult.Count, Is.EqualTo(0));
    }
}
