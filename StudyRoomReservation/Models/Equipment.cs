namespace StudyRoomReservation;

public class Equipment
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class RoomEquipment
{
    public int RoomId { get; set; }
    public int EquipmentId { get; set; }
}