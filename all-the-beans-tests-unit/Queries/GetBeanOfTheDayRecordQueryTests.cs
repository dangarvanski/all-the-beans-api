using all_the_beans_application.Queries;
using all_the_breans_infrastructure.Interfaces;
using all_the_breans_sharedKernal.Entities;
using AutoFixture.NUnit3;
using Moq;

namespace all_the_beans_tests_unit.Queries;

public class GetBeanOfTheDayRecordQueryTests
{
    [Test]
    [AutoData]
    public async Task WhenGetBOTDRequested_ReturnsCorrectBean(BeanDbRecord response)
    {
        // Arange
        response.IsBOTD = true;

        var repository = new Mock<IBeansDbRepository>();

        repository.Setup(r => r.GetBeanOfTheDayRecordAsync())
            .ReturnsAsync(response);

        var query = new GetBeanOfTheDayRecordQuery();
        var queryHandler = new GetBeanOfTheDayRecordQueryHandler(repository.Object);

        // Act
        var botdResult = await queryHandler.Handle(query, It.IsAny<CancellationToken>());

        // Assert
        Assert.That(botdResult, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(botdResult!.IsBOTD, Is.True);
            Assert.That(botdResult._id, Is.EqualTo(response._id));
            Assert.That(botdResult.Name, Is.EqualTo(response.Name));
            Assert.That(botdResult.Color, Is.EqualTo(response.Color));
            Assert.That(botdResult.Country, Is.EqualTo(response.Country));
            Assert.That(botdResult.Cost, Is.EqualTo(response.Cost));
            Assert.That(botdResult.Image, Is.EqualTo(response.Image));
        });
    }
}
