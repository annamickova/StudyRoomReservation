namespace StudyRoomReservation;

/// <summary>
/// Model class representing equipment for room.
/// </summary>
public class Equipment
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    public Equipment(){}

    public Equipment(string name)
    {
        Name = name;
    }
}

public class RoomEquipment
{
    public int RoomId { get; set; }
    public int EquipmentId { get; set; }
}