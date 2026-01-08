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
    capacity INT NOT NULL,
    floor INT
);

 CREATE TABLE seat (
    id INT PRIMARY KEY AUTO_INCREMENT,
    room_id INT,
    FOREIGN KEY (room_id) REFERENCES room(id)
);

CREATE TABLE my_user (
    id INT PRIMARY KEY AUTO_INCREMENT,
    username VARCHAR(100),
    role ENUM('STUDENT','TEACHER','ADMIN')
);

CREATE TABLE reservation (
    id INT PRIMARY KEY AUTO_INCREMENT,
    seat_id INT NOT NULL,
    user_id INT NOT NULL,
    start_time DATETIME NOT NULL,
    end_time DATETIME NOT NULL,
    is_confirmed BOOLEAN,
    FOREIGN KEY (seat_id) REFERENCES seat(id),
    FOREIGN KEY (user_id) REFERENCES my_user(id)
);

CREATE TABLE equipment (
    id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100)
);

CREATE TABLE room_equipment (
    room_id INT,
    equipment_id INT,
    PRIMARY KEY (room_id, equipment_id),
    FOREIGN KEY (room_id) REFERENCES room(id),
    FOREIGN KEY (equipment_id) REFERENCES equipment(id)
);


CREATE INDEX idx_reservation_seat ON reservation(seat_id);

CREATE VIEW reservations_view AS
SELECT
    r.id,
    u.username,
    rm.name AS room,
    s.id AS seat_id,
    r.start_time,
    r.end_time,
    r.is_confirmed
FROM reservation r
JOIN my_user u ON r.user_id = u.id
JOIN seat s ON r.seat_id = s.id
JOIN room rm ON s.room_id = rm.id;

CREATE VIEW room_usage_view AS
SELECT
    rm.name,
    COUNT(r.id) AS reservation_count
FROM room rm
LEFT JOIN seat s ON rm.id = s.room_id
LEFT JOIN reservation r ON s.id = r.seat_id
GROUP BY rm.id;

-- this view not created yet
CREATE VIEW room_equimpment_view AS
SELECT 
    r.name AS room_name, 
    e.name AS equipment_name
FROM room r
LEFT JOIN room_equipment re ON r.id = re.room_id
LEFT JOIN equipment e ON re.equipment_id = e.id;


-- Clear existing data
DELETE FROM room_equipment;
DELETE FROM equipment;
DELETE FROM reservation;
DELETE FROM my_user;
DELETE FROM seat;
DELETE FROM room;

-- Add rooms with floor information
INSERT INTO room (name, capacity, floor) VALUES ('Room A', 5, 1);
INSERT INTO room (name, capacity, floor) VALUES ('Room B', 8, 1);
INSERT INTO room (name, capacity, floor) VALUES ('Room C', 10, 2);

-- Add seats for Room A (5 seats)
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room A';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room A';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room A';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room A';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room A';

-- Add seats for Room B (8 seats)
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room B';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room B';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room B';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room B';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room B';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room B';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room B';
INSERT INTO seat (room_id) SELECT id FROM room WHERE name='Room B';

-- Add seats for Room C (10 seats)
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

-- Add sample users
INSERT INTO my_user (username, role) VALUES ('anna', 'STUDENT');
INSERT INTO my_user (username, role) VALUES ('john', 'STUDENT');
INSERT INTO my_user (username, role) VALUES ('professor_smith', 'TEACHER');
INSERT INTO my_user (username, role) VALUES ('admin', 'ADMIN');

-- Add equipment
INSERT INTO equipment (name) VALUES ('Projector');
INSERT INTO equipment (name) VALUES ('Whiteboard');
INSERT INTO equipment (name) VALUES ('Computer');
INSERT INTO equipment (name) VALUES ('Smart TV');

-- Link equipment to rooms
-- Room A has Projector and Whiteboard
INSERT INTO room_equipment (room_id, equipment_id) 
SELECT r.id, e.id FROM room r, equipment e 
WHERE r.name = 'Room A' AND e.name = 'Projector';

INSERT INTO room_equipment (room_id, equipment_id) 
SELECT r.id, e.id FROM room r, equipment e 
WHERE r.name = 'Room A' AND e.name = 'Whiteboard';

-- Room B has Computer and Smart TV
INSERT INTO room_equipment (room_id, equipment_id) 
SELECT r.id, e.id FROM room r, equipment e 
WHERE r.name = 'Room B' AND e.name = 'Computer';

INSERT INTO room_equipment (room_id, equipment_id) 
SELECT r.id, e.id FROM room r, equipment e 
WHERE r.name = 'Room B' AND e.name = 'Smart TV';

-- Room C has all equipment
INSERT INTO room_equipment (room_id, equipment_id) 
SELECT r.id, e.id FROM room r, equipment e 
WHERE r.name = 'Room C' AND e.name = 'Projector';

INSERT INTO room_equipment (room_id, equipment_id) 
SELECT r.id, e.id FROM room r, equipment e 
WHERE r.name = 'Room C' AND e.name = 'Whiteboard';

INSERT INTO room_equipment (room_id, equipment_id) 
SELECT r.id, e.id FROM room r, equipment e 
WHERE r.name = 'Room C' AND e.name = 'Computer';

INSERT INTO room_equipment (room_id, equipment_id) 
SELECT r.id, e.id FROM room r, equipment e 
WHERE r.name = 'Room C' AND e.name = 'Smart TV';

-- Add sample reservation (user 'anna' reserves seat 1, tomorrow 10am-12pm)
INSERT INTO reservation (seat_id, user_id, start_time, end_time, is_confirmed)
VALUES (
    1,
    (SELECT id FROM my_user WHERE username = 'anna'),
    DATE_ADD(NOW(), INTERVAL 1 DAY) + INTERVAL 10 HOUR,
    DATE_ADD(NOW(), INTERVAL 1 DAY) + INTERVAL 12 HOUR,
    TRUE
);

-- Verify data
SELECT * FROM room;
SELECT * FROM seat;
SELECT * FROM my_user;
SELECT * FROM equipment;
SELECT * FROM room_equipment;
SELECT * FROM reservation;
SELECT * FROM reservations_view;
SELECT * FROM reservation_log;

CREATE TABLE IF NOT EXISTS reservation_log (
    id INT PRIMARY KEY AUTO_INCREMENT,
    reservation_id INT NOT NULL,
    action VARCHAR(50) NOT NULL,
    created_at DATETIME NOT NULL,
    FOREIGN KEY (reservation_id) REFERENCES reservation(id) ON DELETE CASCADE
);

CREATE INDEX idx_log_reservation ON reservation_log(reservation_id);
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
