using all_the_beans_application.Services;
using all_the_breans_infrastructure.Interfaces;
using all_the_breans_sharedKernal.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;

namespace AllTheBeans.Tests.Services
{
    [TestFixture]
    public class BeansServiceTests
    {
        private Mock<IBeansDbRepository> _mockDbRepo;
        private Mock<ILogger<BeansService>> _mockLogger;
        private BeansService _beansService;

        [SetUp]
        public void SetUp()
        {
            _mockDbRepo = new Mock<IBeansDbRepository>();
            _mockLogger = new Mock<ILogger<BeansService>>();
            _beansService = new BeansService(_mockDbRepo.Object, _mockLogger.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _beansService.Dispose();
        }

        [Test]
        public void CalculateNextRunDelay_BeforeMidnight_SchedulesForNextMidnight()
        {
            // Arrange
            var currentTime = new DateTime(2025, 4, 13, 14, 30, 0); // April 13, 2025, 2:30 PM
            var expectedNextRun = new DateTime(2025, 4, 14, 0, 0, 0); // April 14, 2025, midnight
            var expectedDelay = expectedNextRun - currentTime;

            // Act
            var actualDelay = _beansService.CalculateNextRunDelay(currentTime);

            // Assert
            Assert.That(actualDelay, Is.EqualTo(expectedDelay).Within(TimeSpan.FromSeconds(1)),
                $"Expected delay to be {expectedDelay.TotalHours:F2} hours, but was {actualDelay.TotalHours:F2} hours.");
        }

        [Test]
        public void CalculateNextRunDelay_AfterMidnight_SchedulesForNextDayMidnight()
        {
            // Arrange
            var currentTime = new DateTime(2025, 4, 13, 0, 30, 0); // April 13, 2025, 12:30 AM
            var expectedNextRun = new DateTime(2025, 4, 14, 0, 0, 0); // April 14, 2025, midnight
            var expectedDelay = expectedNextRun - currentTime;

            // Act
            var actualDelay = _beansService.CalculateNextRunDelay(currentTime);

            // Assert
            Assert.That(actualDelay, Is.EqualTo(expectedDelay).Within(TimeSpan.FromSeconds(1)),
                $"Expected delay to be {expectedDelay.TotalHours:F2} hours, but was {actualDelay.TotalHours:F2} hours.");
        }

        [Test]
        public void CalculateNextRunDelay_AtMidnight_SchedulesForNextDayMidnight()
        {
            // Arrange
            var currentTime = new DateTime(2025, 4, 13, 0, 0, 0); // April 13, 2025, midnight
            var expectedNextRun = new DateTime(2025, 4, 14, 0, 0, 0); // April 14, 2025, midnight
            var expectedDelay = expectedNextRun - currentTime;

            // Act
            var actualDelay = _beansService.CalculateNextRunDelay(currentTime);

            // Assert
            Assert.That(actualDelay, Is.EqualTo(expectedDelay).Within(TimeSpan.FromSeconds(1)),
                $"Expected delay to be {expectedDelay.TotalHours:F2} hours, but was {actualDelay.TotalHours:F2} hours.");
        }

        [Test]
        public async Task SetBeanOfTheDay_NoCurrentBOTD_SelectsNewBOTD()
        {
            // Arrange
            var indexes = new List<int> { 1, 2, 3 };
            _mockDbRepo.Setup(repo => repo.GetAllIndexesForRecordsAsync())
                .ReturnsAsync(indexes);
            _mockDbRepo.Setup(repo => repo.GetBeanOfTheDayRecordAsync())
                .ReturnsAsync((BeanDbRecord)null); // No current BOTD

            // Act
            await _beansService.SetBeanOfTheDay();

            // Assert
            _mockDbRepo.Verify(repo => repo.SetBeanOfTheDayAsync(-1, It.Is<int>(index => indexes.Contains(index))), Times.Once());
        }

        [Test]
        public async Task SetBeanOfTheDay_WithCurrentBOTD_SelectsDifferentBOTD()
        {
            // Arrange
            var indexes = new List<int> { 1, 2, 3 };
            var currentBOTD = new BeanDbRecord { _id = Guid.NewGuid().ToString(), index = 1 };
            _mockDbRepo.Setup(repo => repo.GetAllIndexesForRecordsAsync())
                .ReturnsAsync(indexes);
            _mockDbRepo.Setup(repo => repo.GetBeanOfTheDayRecordAsync())
                .ReturnsAsync(currentBOTD);

            // Act
            await _beansService.SetBeanOfTheDay();

            // Assert
            _mockDbRepo.Verify(repo => repo.SetBeanOfTheDayAsync(1, It.Is<int>(index => index != 1 && indexes.Contains(index))), Times.Once());
        }

        [Test]
        public async Task SetBeanOfTheDay_NoBeans_DoesNotCallSetBeanOfTheDayAsync()
        {
            // Arrange
            _mockDbRepo.Setup(repo => repo.GetAllIndexesForRecordsAsync())
                .ReturnsAsync(new List<int>());

            // Act
            await _beansService.SetBeanOfTheDay();

            // Assert
            _mockDbRepo.Verify(repo => repo.SetBeanOfTheDayAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
        }

        [Test]
        public async Task StartAsync_SchedulesUpdateAtMidnightAndRepeatsEvery24Hours()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var indexes = new List<int> { 1, 2, 3 };
            _mockDbRepo.Setup(repo => repo.GetAllIndexesForRecordsAsync())
                .ReturnsAsync(indexes);
            _mockDbRepo.Setup(repo => repo.GetBeanOfTheDayRecordAsync())
                .ReturnsAsync((BeanDbRecord)null);

            // Override CalculateNextRunDelay to return a short delay for testing
            var mockService = new Mock<BeansService>(_mockDbRepo.Object, _mockLogger.Object) { CallBase = true };
            mockService.Setup(s => s.CalculateNextRunDelay(It.IsAny<DateTime>()))
                .Returns(TimeSpan.FromMilliseconds(100)); // Short delay for testing

            // Treat the service as IHostedService to start it
            var hostedService = mockService.Object as IHostedService;

            // Act
            await hostedService.StartAsync(cts.Token);
            await Task.Delay(500, cts.Token); // Allow a few cycles (100ms each)
            await hostedService.StopAsync(cts.Token);

            // Assert
            _mockDbRepo.Verify(repo => repo.SetBeanOfTheDayAsync(It.IsAny<int>(), It.IsAny<int>()), Times.AtLeast(2),
                "Expected SetBeanOfTheDay to be called multiple times within 500ms with 100ms delays.");
        }
    }
}