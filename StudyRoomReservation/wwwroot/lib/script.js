document.addEventListener("DOMContentLoaded", () => {
    const container = document.getElementById("rooms-container");

    let selectedStartTime = null;
    let selectedEndTime = null;

    // Form for sending request
    const timeForm = document.createElement("div");
    timeForm.innerHTML = `
        <h2>Vyberte čas rezervace</h2>
        <label>Začátek:</label><br>
        <input type="datetime-local" id="start"><br><br>

        <label>Konec:</label><br>
        <input type="datetime-local" id="end"><br><br>

        <button id="confirmTime">Potvrdit čas</button>
        <hr><br>
    `;
    container.appendChild(timeForm);

    document.getElementById("confirmTime").addEventListener("click", () => {
        const start = document.getElementById("start").value;
        const end = document.getElementById("end").value;

        if (!start || !end) {
            alert("Musíte vybrat oba časy!");
            return;
        }

        selectedStartTime = new Date(start).toISOString();
        selectedEndTime = new Date(end).toISOString();

        console.log("Vybraný start:", selectedStartTime);
        console.log("Vybraný end:", selectedEndTime);

        loadRooms();
    });
    
    
    // Loading rooms
    async function loadRooms() {
        try {
            const res = await fetch("/api/rooms");
            if (!res.ok) throw new Error("Chyba při načítání místností");
            const data = await res.json();

            console.log("Data received from API:", data);

            container.innerHTML = ""; // vyčistí formulář + staré místnosti

            data.forEach(room => {
                console.log("Rendering room:", room);

                const roomDiv = document.createElement("div");
                roomDiv.className = "room";

                const title = document.createElement("h3");
                title.innerText = `${room.name} (Capacity: ${room.capacity})`;
                roomDiv.appendChild(title);

                const seatsDiv = document.createElement("div");
                seatsDiv.className = "seats";

                (room.seats ?? []).forEach(seat => {
                    const seatBtn = document.createElement("button");
                    seatBtn.className = "seat";
                    seatBtn.innerText = seat.id;

                    seatBtn.addEventListener("click", () =>
                        reserveSeat(room.id, seat.id)
                    );

                    seatsDiv.appendChild(seatBtn);
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
            alert("Nejdříve vyberte čas rezervace!");
            return;
        }

        const username = prompt("Zadejte vaše jméno:");
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
                alert("Chyba při rezervaci: " + (errData.error || "neznámá chyba"));
            } else {
                alert("Rezervace úspěšná!");
            }
        } catch (err) {
            console.error("Chyba při rezervaci:", err);
        }
    }
});
