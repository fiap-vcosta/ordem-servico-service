using Application.Time;

namespace UnitTests.Application.Time;

public class AprovacaoTimestampTests
{
    [Fact]
    public void EqualsPersisted_WhenOnlySubMicrosecondDiffers_ReturnsTrue()
    {
        // Arrange
        var persistida = new DateTime(2026, 11, 11, 1, 33, 49, 245, DateTimeKind.Utc).AddTicks(7980);
        var evento = new DateTimeOffset(persistida.AddTicks(7), TimeSpan.Zero);

        // Act
        var match = AprovacaoTimestamp.EqualsPersisted(persistida, evento);

        // Assert
        Assert.True(match);
    }

    [Fact]
    public void EqualsPersisted_WhenMicrosecondDiffers_ReturnsFalse()
    {
        // Arrange
        var persistida = new DateTime(2026, 11, 11, 1, 33, 49, 245, DateTimeKind.Utc).AddTicks(7980);
        var evento = new DateTimeOffset(persistida.AddTicks(10), TimeSpan.Zero);

        // Act
        var match = AprovacaoTimestamp.EqualsPersisted(persistida, evento);

        // Assert
        Assert.False(match);
    }

    [Fact]
    public void ToEventTimestamp_TruncatesToMicroseconds()
    {
        // Arrange
        var value = new DateTime(2026, 11, 11, 1, 33, 49, 245, DateTimeKind.Utc).AddTicks(7987);

        // Act
        var truncated = AprovacaoTimestamp.ToEventTimestamp(value);

        // Assert
        Assert.Equal(0, truncated.Ticks % TimeSpan.TicksPerMicrosecond);
        Assert.Equal(value.Ticks - 7, truncated.UtcTicks);
    }
}
