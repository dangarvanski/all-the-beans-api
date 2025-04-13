using all_the_beans_application.Commands;
using all_the_breans_infrastructure.Interfaces;
using all_the_breans_sharedKernal.Entities;
using AutoFixture.NUnit3;
using Moq;

namespace all_the_beans_tests_unit.Commands;

public class DeleteRecordCommandTests
{
    [Test]
    [AutoData]
    public async Task DeleteRecord_WhenSuccessful_ReturnsTrue(DeleteRecordCommand deleteRecordCommand, BeanDbRecord newRecord)
    {
        // Arange
        var repository = new Mock<IBeansDbRepository>();
        repository.Setup(r => r.GetRecordByIndexAsync(deleteRecordCommand.index))
            .ReturnsAsync(newRecord);
        repository.Setup(r => r.DeleteRecordByIndexAsync(It.IsAny<BeanDbRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var query = new DeleteRecordCommand(deleteRecordCommand.index);
        var queryHandler = new DeleteRecordCommandHandler(repository.Object);

        // Act
        var result = await queryHandler.Handle(query, It.IsAny<CancellationToken>());

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task DeleteRecord_WhenUnsuccessful_ReturnsFalse()
    {
        // Arange
        var repository = new Mock<IBeansDbRepository>();
        repository.Setup(r => r.DeleteRecordByIndexAsync(It.IsAny<BeanDbRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var query = new DeleteRecordCommand(It.IsAny<int>());
        var queryHandler = new DeleteRecordCommandHandler(repository.Object);

        // Act
        var result = await queryHandler.Handle(query, It.IsAny<CancellationToken>());

        // Assert
        Assert.That(result, Is.False);
    }
}
