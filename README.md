# Study Room Reservation System

Simple web app for booking study room seats with time slots.

## Features

- Book seats in different study rooms
- Pick start and end time for your reservation
- Prevents double bookings automatically
- Uses multiple threads to handle requests faster

## Setup

### Database

Run this in MySQL:

```sql
CREATE DATABASE study_rooms;
USE study_rooms;

DROP TABLE IF EXISTS reservation;
DROP TABLE IF EXISTS seat;
DROP TABLE IF EXISTS room;

CREATE TABLE room (
    id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL,
    capacity INT NOT NULL
);

CREATE TABLE seat (
    id INT PRIMARY KEY AUTO_INCREMENT,
    room_id INT,
    FOREIGN KEY (room_id) REFERENCES room(id)
);

CREATE TABLE reservation (
    id INT PRIMARY KEY AUTO_INCREMENT,
    room_id INT NOT NULL,
    seat_id INT NOT NULL,
    username VARCHAR(100) NOT NULL,
    start_time DATETIME NOT NULL,
    end_time DATETIME NOT NULL,
    FOREIGN KEY (room_id) REFERENCES room(id)
);

CREATE INDEX idx_reservation_seat ON reservation(seat_id);

-- Add test data
DELETE FROM reservation;
DELETE FROM seat;
DELETE FROM room;

INSERT INTO room (name, capacity) VALUES ('Room A', 5);
INSERT INTO room (name, capacity) VALUES ('Room B', 8);
INSERT INTO room (name, capacity) VALUES ('Room C', 10);

-- Room A seats
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room A';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room A';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room A';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room A';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room A';

-- Room B seats
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room B';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room B';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room B';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room B';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room B';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room B';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room B';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room B';

-- Room C seats
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room C';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room C';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room C';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room C';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room C';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room C';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room C';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room C';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room C';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room C';
```

### Config file

Make `appsettings.json` with your MySQL credentials:

```json
{
  "Database": {
    "Server": "localhost",
    "Port": "3306",
    "Database": "study_rooms",
    "User": "root",
    "Password": "your_password"
  }
}
```

### Run it

```bash
dotnet run
```

Go to `http://localhost:8080`

## How to use

### Main page
- Pick a time range first
- Then click on any seat to reserve it
- Enter your name

## API

**Get rooms:**
```
GET /api/rooms
```

**Make reservation:**
```
POST /api/reserve

{
  "RoomId": 1,
  "SeatId": 1,
  "Username": "jan",
  "StartTime": "2025-11-28T10:00:00",
  "EndTime": "2025-11-28T12:00:00"
}
```

## Technical

- ASP.NET Core with C#
- MySQL database
- 4 worker threads handle reservations in parallel
- Uses ConcurrentQueue for thread safety