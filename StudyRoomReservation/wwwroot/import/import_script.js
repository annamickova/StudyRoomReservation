async function importRooms() {
    const csvData = document.getElementById('roomsCsv').value;
    const resultDiv = document.getElementById('roomsResult');

    if (!csvData.trim()) {
        showResult(resultDiv, 'Please paste CSV data', false);
        return;
    }

    try {
        const response = await fetch('/api/import/rooms', {
            method: 'POST',
            headers: { 'Content-Type': 'text/plain' },
            body: csvData
        });

        const data = await response.json();

        if (response.ok) {
            showResult(resultDiv, data.message, true);
            document.getElementById('roomsCsv').value = '';
        } else {
            showResult(resultDiv, data.error, false);
        }
    } catch (error) {
        showResult(resultDiv, 'Error: ' + error.message, false);
    }
}

async function importEquipment() {
    const csvData = document.getElementById('equipmentCsv').value;
    const resultDiv = document.getElementById('equipmentResult');

    if (!csvData.trim()) {
        showResult(resultDiv, 'Please paste equipment names', false);
        return;
    }

    try {
        const response = await fetch('/api/import/equipment', {
            method: 'POST',
            headers: { 'Content-Type': 'text/plain' },
            body: csvData
        });

        const data = await response.json();

        if (response.ok) {
            showResult(resultDiv, data.message, true);
            document.getElementById('equipmentCsv').value = '';
        } else {
            showResult(resultDiv, data.error, false);
        }
    } catch (error) {
        showResult(resultDiv, 'Error: ' + error.message, false);
    }
}

function showResult(div, message, success) {
    div.innerHTML = `<div class="result ${success ? 'success' : 'error'}">${message}</div>`;
    setTimeout(() => {
        div.innerHTML = '';
    }, 5000);
}