using Xunit;

namespace StudyRoomReservation.Tests;

public class UnitTests
{
    [Fact]
    public void Room_Constructor_CreatesCorrectNumberOfSeats()
    {
        var room = new Room("Test Room", 5);
        Assert.Equal(5, room.Seats.Count);
        Assert.Equal("Test Room", room.Name);
    }

    [Fact]
    public void Room_Constructor_ThrowsException_WhenNameIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new Room("", 5));
    }

    [Fact]
    public void Room_Constructor_ThrowsException_WhenCapacityIsZero()
    {
        Assert.Throws<ArgumentException>(() => new Room("Test Room", 0));
    }

    [Fact]
    public void Reservation_Constructor_SetsPropertiesCorrectly()
    {
        var startTime = DateTime.Now;
        var endTime = startTime.AddHours(2);

        var reservation = new Reservation(1, 2, "jan", startTime, endTime);

        Assert.Equal(1, reservation.RoomId);
        Assert.Equal(2, reservation.SeatId);
        Assert.Equal("jan", reservation.Username);
    }

    [Fact]
    public void ValidateTimeRange_Valid_WhenStartBeforeEnd()
    {
        var startTime = DateTime.Now;
        var endTime = startTime.AddHours(2);
        var isValid = startTime < endTime;

        Assert.True(isValid);
    }

    [Theory]
    [InlineData("anna")]
    [InlineData("john_doe")]
    public void Username_IsValid_WhenNotEmpty(string username)
    {
        var isValid = !string.IsNullOrWhiteSpace(username);

        Assert.True(isValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Username_IsInvalid_WhenEmpty(string username)
    {
        var isValid = !string.IsNullOrWhiteSpace(username);

        Assert.False(isValid);
    }
}