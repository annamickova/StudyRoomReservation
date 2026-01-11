let allReservations = [];
let currentEditId = null;

// load reservations on page load
document.addEventListener("DOMContentLoaded", () => {
    loadReservations();
    loadStatistics();

    // filter listener
    document.getElementById("filterUser").addEventListener("input", (e) => {
        filterReservations(e.target.value);
    });
});

// load all reservations
async function loadReservations() {
    try {
        const res = await fetch("/api/reservations");
        if (!res.ok) throw new Error("Failed to load reservations");

        allReservations = await res.json();
        console.log("Loaded reservations:", allReservations);

        displayReservations(allReservations);
    } catch (err) {
        console.error("Error loading reservations:", err);
        alert("Failed to load reservations");
    }
}

// display reservations in table
function displayReservations(reservations) {
    const tbody = document.getElementById("reservationsBody");
    const noReservations = document.getElementById("noReservations");
    const table = document.getElementById("reservationsTable");

    tbody.innerHTML = "";

    if (reservations.length === 0) {
        table.style.display = "none";
        noReservations.style.display = "block";
        return;
    }

    table.style.display = "table";
    noReservations.style.display = "none";

    reservations.forEach(res => {
        const row = document.createElement("tr");

        const startTime = new Date(res.startTime).toLocaleString();
        const endTime = new Date(res.endTime).toLocaleString();

        const statusBadge = res.isConfirmed
            ? '<span class="status-badge status-confirmed">Confirmed</span>'
            : '<span class="status-badge status-pending">Pending</span>';

        const roleBadge = `<span class="role-badge role-${res.userRole.toLowerCase()}">${res.userRole}</span>`;

        row.innerHTML = `
            <td>${res.id}</td>
            <td>${res.username}</td>
            <td>${roleBadge}</td>
            <td>${res.roomName}</td>
            <td>${res.seatId}</td>
            <td>${startTime}</td>
            <td>${endTime}</td>
            <td>${statusBadge}</td>
            <td>
                <div class="action-buttons">
                    ${!res.isConfirmed ? `<button class="btn btn-confirm" onclick="confirmReservation(${res.id})">Confirm</button>` : ''}
                    <button class="btn btn-edit" onclick="editReservation(${res.id})">Edit</button>
                    <button class="btn btn-delete" onclick="deleteReservation(${res.id})">Delete</button>
                </div>
            </td>
        `;

        tbody.appendChild(row);
    });
}

async function loadStatistics() {
    try {
        const res = await fetch("/api/report");
        if (!res.ok) throw new Error("Failed to load statistics");

        const report = await res.json();

        document.getElementById("totalRes").textContent = report.totalReservations;
        document.getElementById("confirmedRes").textContent = report.confirmedReservations;
        document.getElementById("pendingRes").textContent = report.pendingReservations;
        document.getElementById("totalUsers").textContent = report.totalUsers;
        document.getElementById("totalRooms").textContent = report.totalRooms;
        document.getElementById("totalSeats").textContent = report.totalSeats;
    } catch (err) {
        console.error("Error loading statistics:", err);
    }
}

// filter reservations
function filterReservations(username) {
    if (!username.trim()) {
        displayReservations(allReservations);
        return;
    }
    const filtered = allReservations.filter(res =>
        res.username.toLowerCase().includes(username.toLowerCase())
    );

    displayReservations(filtered);
}

// confirm reservation
async function confirmReservation(id) {
    if (!confirm("Confirm this reservation?")) return;

    try {
        const res = await fetch(`/api/reservations/${id}/confirm`, {
            method: "POST"
        });

        if (!res.ok) throw new Error("Failed to confirm");

        alert("Reservation confirmed");
        loadReservations();
    } catch (err) {
        console.error("Error confirming reservation:", err);
        alert("Failed to confirm reservation");
    }
}

// edit reservation
function editReservation(id) {
    const reservation = allReservations.find(r => r.id === id);
    if (!reservation) return;

    currentEditId = id;

    // convert to datetime-local format
    const startLocal = new Date(reservation.startTime).toISOString().slice(0, 16);
    const endLocal = new Date(reservation.endTime).toISOString().slice(0, 16);

    document.getElementById("editStartTime").value = startLocal;
    document.getElementById("editEndTime").value = endLocal;

    document.getElementById("editModal").style.display = "block";
}

// save edit
async function saveEdit() {
    const startTime = new Date(document.getElementById("editStartTime").value).toISOString();
    const endTime = new Date(document.getElementById("editEndTime").value).toISOString();

    if (!startTime || !endTime) {
        alert("Please fill in both times");
        return;
    }

    if (new Date(startTime) >= new Date(endTime)) {
        alert("Start time must be before end time");
        return;
    }

    try {
        const res = await fetch(`/api/reservations/${currentEditId}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                startTime: startTime,
                endTime: endTime
            })
        });

        if (!res.ok) {
            const data = await res.json();
            throw new Error(data.error || "Failed to update");
        }

        alert("Reservation updated!");
        closeEditModal();
        loadReservations();
    } catch (err) {
        console.error("Error updating reservation:", err);
        alert(err.message);
    }
}

// close edit modal
function closeEditModal() {
    document.getElementById("editModal").style.display = "none";
    currentEditId = null;
}

// delete reservation
async function deleteReservation(id) {
    if (!confirm("Are you sure you want to delete this reservation?")) return;

    try {
        const res = await fetch(`/api/reservations/${id}`, {
            method: "DELETE"
        });

        if (!res.ok) throw new Error("Failed to delete");

        alert("Reservation deleted");
        loadReservations();
    } catch (err) {
        console.error("Error deleting reservation:", err);
        alert("Failed to delete reservation");
    }
}

// close modal when clicking outside
window.onclick = function(event) {
    const modal = document.getElementById("editModal");
    if (event.target === modal) {
        closeEditModal();
    }
}