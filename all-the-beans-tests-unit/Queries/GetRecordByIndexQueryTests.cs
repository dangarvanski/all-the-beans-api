using all_the_beans_application.Queries;
using all_the_breans_infrastructure.Interfaces;
using all_the_breans_sharedKernal.Entities;
using AutoFixture.NUnit3;
using Moq;

namespace all_the_beans_tests_unit.Queries;

public class GetRecordByIndexQueryTests
{
    [Test]
    [AutoData]
    public async Task WhenAValidIndexIsPassed_ReturnsTheCorrectBean(BeanDbRecord testRecord)
    {
        // Arange
        var recordIndex = testRecord.index;
        var repository = new Mock<IBeansDbRepository>();

        repository.Setup(r => r.GetRecordByIndexAsync(recordIndex))
            .ReturnsAsync(testRecord);

        var query = new GetRecordByIndexQuery(recordIndex);
        var queryHandler = new GetRecordByIndexQueryHandler(repository.Object);

        // Act
        var result = await queryHandler.Handle(query, It.IsAny<CancellationToken>());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!._id, Is.EqualTo(testRecord._id));
            Assert.That(result.Name, Is.EqualTo(testRecord.Name));
            Assert.That(result.Color, Is.EqualTo(testRecord.Color));
            Assert.That(result.Country, Is.EqualTo(testRecord.Country));
            Assert.That(result.Cost, Is.EqualTo(testRecord.Cost));
            Assert.That(result.Image, Is.EqualTo(testRecord.Image));
            Assert.That(result.IsBOTD, Is.EqualTo(testRecord.IsBOTD));
        });
    }

    [Test]
    [AutoData]
    public async Task WhenAnInvalidIndexIsPassed_ReturnsNullResponse()
    {
        // Arange
        var repository = new Mock<IBeansDbRepository>();

        var query = new GetRecordByIndexQuery(1);
        var queryHandler = new GetRecordByIndexQueryHandler(repository.Object);

        // Act
        var result = await queryHandler.Handle(query, It.IsAny<CancellationToken>());

        // Assert
        Assert.That(result, Is.Null);
    }
}
