document.addEventListener("DOMContentLoaded", () => {
    const container = document.getElementById("rooms-container");

    let selectedStartTime = null;
    let selectedEndTime = null;

    // Form for sending request
    const timeForm = document.createElement("div");
    timeForm.innerHTML = `
        <h2>Select time of reservation</h2>
        <label>Start:</label><br>
        <input type="datetime-local" id="start"><br><br>

        <label>End:</label><br>
        <input type="datetime-local" id="end"><br><br>

        <button id="confirmTime">Confirm time</button>
        <hr><br>
    `;
    container.appendChild(timeForm);

    document.getElementById("confirmTime").addEventListener("click", () => {
        const start = document.getElementById("start").value;
        const end = document.getElementById("end").value;

        if (!start || !end) {
            alert("You need to pick both times");
            return;
        }

        selectedStartTime = new Date(start).toISOString();
        selectedEndTime = new Date(end).toISOString();

        console.log("Selected start time:", selectedStartTime);
        console.log("Selected end time:", selectedEndTime);

        loadRooms();
    });

    // Loading rooms
    async function loadRooms() {
        try {
            // Build query parameters
            const params = new URLSearchParams();
            if (selectedStartTime) params.append('startTime', selectedStartTime);
            if (selectedEndTime) params.append('endTime', selectedEndTime);

            const url = `/api/rooms?${params.toString()}`;
            console.log("Fetching from URL:", url);

            const res = await fetch(url);
            if (!res.ok) throw new Error("Error loading rooms");
            const data = await res.json();

            console.log("Data received from API:", JSON.stringify(data, null, 2));

            // Check if seats have isReserved property
            if (data.length > 0 && data[0].seats.length > 0) {
                console.log("First seat object:", data[0].seats[0]);
                console.log("Has isReserved?", 'isReserved' in data[0].seats[0]);
            }

            container.querySelectorAll(".room").forEach(r => r.remove());

            data.forEach(room => {
                console.log(`🏠 Processing Room ${room.id} (${room.name})`);
                console.log(`   Seats in room:`, room.seats);

                const roomDiv = document.createElement("div");
                roomDiv.className = "room";

                // Header
                const header = document.createElement("div");
                header.className = "room-header";

                const title = document.createElement("h3");
                title.innerText = `${room.name} (Capacity: ${room.capacity})`;
                header.appendChild(title);

                if (room.floor) {
                    const floorInfo = document.createElement("span");
                    floorInfo.className = "floor-info";
                    floorInfo.innerText = `Floor ${room.floor}`;
                    header.appendChild(floorInfo);
                }

                roomDiv.appendChild(header);

                if (room.equipment.length > 0) {
                    const equipmentDiv = document.createElement("div");
                    equipmentDiv.className = "equipment-list";

                    const equipmentTitle = document.createElement("h4");
                    equipmentTitle.innerText = "Equipment:";
                    equipmentDiv.appendChild(equipmentTitle);

                    const ul = document.createElement("ul");
                    room.equipment.forEach(e => {
                        const li = document.createElement("li");
                        li.className = "equipment-item";
                        li.innerText = e.name;
                        ul.appendChild(li);
                    });

                    equipmentDiv.appendChild(ul);
                    roomDiv.appendChild(equipmentDiv);
                }

                const seatsDiv = document.createElement("div");
                seatsDiv.className = "seats";

                room.seats.forEach(seat => {
                    console.log(`   Seat ${seat.id}: isReserved=${seat.isReserved}`);

                    const btn = document.createElement("button");
                    btn.className = "seat";

                    if (seat.isReserved === true) {
                        console.log(`     → Adding RESERVED class`);
                        btn.classList.add("reserved");
                        btn.disabled = true;
                    } else {
                        console.log(`     → Adding AVAILABLE class`);
                        btn.classList.add("available");
                        btn.addEventListener("click", () =>
                            reserveSeat(room.id, seat.id)
                        );
                    }

                    btn.innerText = seat.id;
                    seatsDiv.appendChild(btn);
                });

                roomDiv.appendChild(seatsDiv);
                container.appendChild(roomDiv);
            });
        } catch (err) {
            console.error("Error fetching rooms:", err);
        }
    }

    // Seat reservation
    async function reserveSeat(roomId, seatId) {
        if (!selectedStartTime || !selectedEndTime) {
            alert("Pick reservation time");
            return;
        }

        const username = prompt("Enter username:");
        if (!username) return;

        const request = {
            RoomId: roomId,
            SeatId: seatId,
            Username: username,
            StartTime: selectedStartTime,
            EndTime: selectedEndTime
        };

        console.log("Sending reservation:", request);

        try {
            const res = await fetch("/api/reserve", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(request)
            });

            if (!res.ok) {
                const errData = await res.json();
                alert("Error creating reservation: " + (errData.error || "unknown error"));
            } else {
                alert("Reservation successful for: " + username);
                loadRooms(); // Reload rooms to update seat colors
            }
        } catch (err) {
            console.error("Error creating reservation:", err);
        }
    }
});