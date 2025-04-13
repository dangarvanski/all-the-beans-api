using all_the_beans_application.Commands;
using all_the_breans_infrastructure.Interfaces;
using all_the_breans_sharedKernal.Entities;
using AutoFixture.NUnit3;
using Moq;

namespace all_the_beans_tests_unit.Commands;

public class CreateRecordCommandTests
{
    [Test]
    [AutoData]
    public async Task CreateRecord_WhenSuccessful_AValidIndexIsReturned(CreateRecordRequest createRecordRequest, BeanDbRecord newRecord)
    {
        // Arange
        var repository = new Mock<IBeansDbRepository>();
        repository.Setup(r => r.InsertNewBeanRecordAsync(It.IsAny<BeanDbRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newRecord);

        var query = new CreateRecordCommand(createRecordRequest);
        var queryHandler = new CreateRecordCommandHandler(repository.Object);

        // Act
        var result = await queryHandler.Handle(query, It.IsAny<CancellationToken>());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Keys.First(), Is.True);
        Assert.That(result.Values.First, Is.EqualTo(newRecord.index));
    }

    [Test]
    [AutoData]
    public async Task CreateRecord_WhenUnsuccessful_AZeroIndexIsReturned(CreateRecordRequest createRecordRequest, BeanDbRecord newRecord)
    {
        // Arange
        var repository = new Mock<IBeansDbRepository>();
        repository.Setup(r => r.InsertNewBeanRecordAsync(It.IsAny<BeanDbRecord>(), It.IsAny<CancellationToken>()));

        var query = new CreateRecordCommand(createRecordRequest);
        var queryHandler = new CreateRecordCommandHandler(repository.Object);

        // Act
        var result = await queryHandler.Handle(query, It.IsAny<CancellationToken>());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Keys.First(), Is.False);
        Assert.That(result.Values.First, Is.EqualTo(0));
    }
}
