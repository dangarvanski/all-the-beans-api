using all_the_beans_application.Commands;
using all_the_breans_infrastructure.Interfaces;
using all_the_breans_sharedKernal.Entities;
using AutoFixture.NUnit3;
using Moq;

namespace all_the_beans_tests_unit.Commands;

public class UpdateRecordCommandTests
{
    [Test]
    [AutoData]
    public async Task UpdateRecord_WhenSuccessful_ReturnsUpdatedRecord(UpdateRecordRequest updateRecordRequest, BeanDbRecord newRecord)
    {
        // Arange
        var repository = new Mock<IBeansDbRepository>();
        repository.Setup(r => r.GetRecordByIndexAsync(newRecord.index))
            .ReturnsAsync(newRecord);
        repository.Setup(r => r.UpdateBeanRecordAsync(It.IsAny<BeanDbRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var query = new UpdateRecordCommand(newRecord.index, updateRecordRequest);
        var queryHandler = new UpdateRecordCommandHandler(repository.Object);

        // Act
        var result = await queryHandler.Handle(query, It.IsAny<CancellationToken>());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!._id, Is.EqualTo(newRecord._id));
            Assert.That(result.Name, Is.EqualTo(newRecord.Name));
            Assert.That(result.Color, Is.EqualTo(newRecord.Color));
            Assert.That(result.Country, Is.EqualTo(newRecord.Country));
            Assert.That(result.Cost, Is.EqualTo(newRecord.Cost));
            Assert.That(result.Image, Is.EqualTo(newRecord.Image));
            Assert.That(result.IsBOTD, Is.EqualTo(newRecord.IsBOTD));
        });
    }

    [Test]
    public async Task UpdateRecord_WhenUnsuccessful_ReturnsNull()
    {
        // Arange
        var repository = new Mock<IBeansDbRepository>();
        repository.Setup(r => r.UpdateBeanRecordAsync(It.IsAny<BeanDbRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var query = new UpdateRecordCommand(It.IsAny<int>(), It.IsAny<UpdateRecordRequest>());
        var queryHandler = new UpdateRecordCommandHandler(repository.Object);

        // Act
        var result = await queryHandler.Handle(query, It.IsAny<CancellationToken>());

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task UpdateRecord_WhenNoValidFields_ThrowsArgumentException()
    {
        // Arrange
        var repository = new Mock<IBeansDbRepository>();

        var record = new BeanDbRecord { _id = Guid.NewGuid().ToString(), index = 15 };
        repository.Setup(r => r.GetRecordByIndexAsync(15))
            .ReturnsAsync(record);

        var request = new UpdateRecordRequest { Description = "" };
        var command = new UpdateRecordCommand(15, request);
        var queryHandler = new UpdateRecordCommandHandler(repository.Object);


        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await queryHandler.Handle(command, CancellationToken.None));
    }
}
