// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
let sensors = [];

// =====================================
// START APPLICATION
// =====================================

document.addEventListener(
    "DOMContentLoaded",
    async () => {

        if (!document.getElementById("sensorForm")) {
            return;
        }

        setupEvents();

        await refreshDashboard();
    }
);


// =====================================
// EVENTS
// =====================================

function setupEvents() {

    document
        .getElementById("sensorForm")
        .addEventListener(
            "submit",
            registerSensor);


    document
        .getElementById("telemetryForm")
        .addEventListener(
            "submit",
            submitTelemetry);


    document
        .getElementById("fileForm")
        .addEventListener(
            "submit",
            uploadFile);


    document
        .getElementById("refreshSensors")
        .addEventListener(
            "click",
            refreshDashboard);


    document
        .getElementById("historySensor")
        .addEventListener(
            "change",
            loadTelemetryHistory);


    document
        .getElementById("simulateBatchButton")
        .addEventListener(
            "click",
            simulateBatch);


    document
        .getElementById("validateTreeButton")
        .addEventListener(
            "click",
            validateDeploymentTree);


    document
        .getElementById("telemetryType")
        .addEventListener(
            "change",
            updateTelemetryPlaceholder);
}


// =====================================
// REGISTER SENSOR
// =====================================

async function registerSensor(event) {

    event.preventDefault();


    const request = {

        deviceId:
            document
                .getElementById("deviceId")
                .value
                .trim(),

        location:
            document
                .getElementById("location")
                .value
                .trim(),

        category:
            document
                .getElementById("category")
                .value
    };


    const response =
        await fetch(
            "/api/sensors",
            {
                method: "POST",

                headers: {
                    "Content-Type":
                        "application/json"
                },

                body:
                    JSON.stringify(request)
            });


    const result =
        await response.json();


    if (!response.ok) {

        showMessage(
            "registrationMessage",
            result.message ??
            "Could not register sensor.",
            "error");

        return;
    }


    showMessage(
        "registrationMessage",
        "Sensor registered successfully.",
        "success");


    document
        .getElementById("sensorForm")
        .reset();


    await refreshDashboard();
}


// =====================================
// LOAD SENSORS
// =====================================

async function loadSensors() {

    const response =
        await fetch(
            "/api/sensors");


    sensors =
        await response.json();


    renderSensorList();

    updateSensorSelects();
}


// =====================================
// RENDER SENSOR CARDS
// =====================================

function renderSensorList() {

    const container =
        document
            .getElementById("sensorList");


    if (sensors.length === 0) {

        container.innerHTML = `

            <div class="empty-state">

                No sensors registered yet.

            </div>
        `;

        return;
    }


    container.innerHTML =
        sensors.map(sensor => {

            const fileCount =
                sensor.attachments?.length ?? 0;


            return `

                <div class="sensor-item">

                    <div class="sensor-status-dot">
                    </div>

                    <div class="sensor-info">

                        <strong>
                            ${escapeHtml(sensor.deviceId)}
                        </strong>

                        <span>
                            ${escapeHtml(sensor.location)}
                        </span>

                        <small>
                            ${formatCategory(sensor.category)}
                            ·
                            ${fileCount} attachment(s)
                        </small>

                    </div>

                </div>
            `;

        }).join("");
}


// =====================================
// UPDATE SENSOR DROPDOWNS
// =====================================

function updateSensorSelects() {

    const selectIds = [
        "telemetrySensor",
        "fileSensor",
        "historySensor"
    ];


    selectIds.forEach(id => {

        const select =
            document.getElementById(id);


        const currentValue =
            select.value;


        select.innerHTML = `

            <option value="">
                Select sensor
            </option>

        `;


        sensors.forEach(sensor => {

            const option =
                document.createElement(
                    "option");


            option.value =
                sensor.id;


            option.textContent =
                `${sensor.deviceId} — ${sensor.location}`;


            select.appendChild(option);
        });


        if (
            sensors.some(
                sensor =>
                    sensor.id.toString() ===
                    currentValue)
        ) {

            select.value =
                currentValue;
        }
    });
}


// =====================================
// SEND TELEMETRY
// =====================================

async function submitTelemetry(event) {

    event.preventDefault();


    const sensorId =
        Number(
            document
                .getElementById(
                    "telemetrySensor")
                .value);


    const sensor =
        sensors.find(
            item =>
                item.id === sensorId);


    if (!sensor) {

        showMessage(
            "telemetryMessage",
            "Please select a sensor.",
            "error");

        return;
    }


    const type =
        document
            .getElementById(
                "telemetryType")
            .value;


    const rawValue =
        document
            .getElementById(
                "telemetryValue")
            .value
            .trim();


    let value;


    if (type === "float") {

        value =
            parseFloat(rawValue);


        if (Number.isNaN(value)) {

            showMessage(
                "telemetryMessage",
                "Enter a valid float value.",
                "error");

            return;
        }

    }
    else if (type === "int") {

        value =
            Number(rawValue);


        if (
            !Number.isInteger(value)
        ) {

            showMessage(
                "telemetryMessage",
                "Enter a whole number.",
                "error");

            return;
        }

    }
    else {

        const boolValue =
            rawValue.toLowerCase();


        if (
            boolValue !== "true" &&
            boolValue !== "false"
        ) {

            showMessage(
                "telemetryMessage",
                "Boolean values must be true or false.",
                "error");

            return;
        }


        value =
            boolValue === "true";
    }


    const packet = {

        deviceId:
            sensor.deviceId,

        value:
            value,

        unit:
            document
                .getElementById(
                    "telemetryUnit")
                .value
                .trim(),

        timestamp:
            new Date().toISOString()
    };


    const response =
        await fetch(
            `/api/sensors/${sensorId}/telemetry/${type}`,
            {
                method: "POST",

                headers: {
                    "Content-Type":
                        "application/json"
                },

                body:
                    JSON.stringify(packet)
            });


    const result =
        await response.json();


    if (!response.ok) {

        showMessage(
            "telemetryMessage",
            result.message ??
            "Telemetry submission failed.",
            "error");

        return;
    }


    showMessage(
        "telemetryMessage",
        result.message,
        "success");


    document
        .getElementById(
            "telemetryValue")
        .value = "";


    await refreshDashboard();


    document
        .getElementById(
            "historySensor")
        .value =
        sensorId;


    await loadTelemetryHistory();
}


// =====================================
// TELEMETRY HISTORY
// =====================================

async function loadTelemetryHistory() {

    const sensorId =
        document
            .getElementById(
                "historySensor")
            .value;


    const table =
        document
            .getElementById(
                "telemetryTable");


    if (!sensorId) {

        table.innerHTML = `

            <tr>

                <td colspan="4"
                    class="table-empty">

                    Select a sensor.

                </td>

            </tr>

        `;

        return;
    }


    const response =
        await fetch(
            `/api/sensors/${sensorId}/telemetry`);


    const data =
        await response.json();


    let readings = [];


    data.floatReadings.forEach(
        packet => {

            readings.push({

                type:
                    "Float",

                value:
                    packet.value,

                unit:
                    packet.unit,

                timestamp:
                    packet.timestamp
            });
        });


    data.integerReadings.forEach(
        packet => {

            readings.push({

                type:
                    "Integer",

                value:
                    packet.value,

                unit:
                    packet.unit,

                timestamp:
                    packet.timestamp
            });
        });


    data.booleanReadings.forEach(
        packet => {

            readings.push({

                type:
                    "Boolean",

                value:
                    packet.value,

                unit:
                    packet.unit,

                timestamp:
                    packet.timestamp
            });
        });


    readings.sort(
        (first, second) =>
            new Date(second.timestamp) -
            new Date(first.timestamp));


    readings =
        readings.slice(0, 15);


    if (readings.length === 0) {

        table.innerHTML = `

            <tr>

                <td colspan="4"
                    class="table-empty">

                    No telemetry received yet.

                </td>

            </tr>
        `;

        return;
    }


    table.innerHTML =
        readings.map(reading => `

            <tr>

                <td>
                    <span class="type-badge">
                        ${reading.type}
                    </span>
                </td>

                <td>
                    ${reading.value}
                </td>

                <td>
                    ${escapeHtml(reading.unit)}
                </td>

                <td>
                    ${formatDate(reading.timestamp)}
                </td>

            </tr>

        `).join("");
}


// =====================================
// FILE UPLOAD
// =====================================

async function uploadFile(event) {

    event.preventDefault();


    const sensorId =
        document
            .getElementById(
                "fileSensor")
            .value;


    const fileInput =
        document
            .getElementById(
                "deviceFile");


    if (!sensorId) {

        showMessage(
            "fileMessage",
            "Please select a sensor.",
            "error");

        return;
    }


    if (
        fileInput.files.length === 0
    ) {

        showMessage(
            "fileMessage",
            "Please choose a file.",
            "error");

        return;
    }


    const formData =
        new FormData();


    formData.append(
        "file",
        fileInput.files[0]);


    const response =
        await fetch(
            `/api/sensors/${sensorId}/files`,
            {
                method: "POST",

                body:
                    formData
            });


    const result =
        await response.json();


    if (!response.ok) {

        showMessage(
            "fileMessage",
            result.message ??
            "Upload failed.",
            "error");

        return;
    }


    showMessage(
        "fileMessage",
        `Uploaded ${result.originalFileName}`,
        "success");


    fileInput.value = "";


    await loadSensors();
}


// =====================================
// RAW TELEMETRY BATCH
// =====================================

async function simulateBatch() {

    const sensorId =
        document
            .getElementById(
                "telemetrySensor")
            .value;


    if (!sensorId) {

        showMessage(
            "advancedMessage",
            "Select a sensor in the telemetry form first.",
            "error");

        return;
    }


    const response =
        await fetch(
            `/api/sensors/${sensorId}/simulate-batch`,
            {
                method: "POST"
            });


    const result =
        await response.json();


    if (!response.ok) {

        showMessage(
            "advancedMessage",
            result.message,
            "error");

        return;
    }


    showMessage(
        "advancedMessage",
        result.message,
        "success");


    await refreshDashboard();


    document
        .getElementById(
            "historySensor")
        .value =
        sensorId;


    await loadTelemetryHistory();
}


// =====================================
// RECURSIVE DEPLOYMENT VALIDATION
// =====================================

async function validateDeploymentTree() {

    const hydroponicDeploymentTree = {
        name: "Main Facility",
        type: "Facility",
        isConfigured: true,
        children: [
            {
                name: "Grow Room 1",
                type: "Room",
                isConfigured: true,
                children: [
                    {
                        name: "NFT Loop A",
                        type: "GrowingChannel",
                        isConfigured: true,
                        children: [
                            {
                                name: "Main Reservoir",
                                type: "Reservoir",
                                isConfigured: true,
                                children: []
                            }
                        ]
                    }
                ]
            }
        ]
    };


    const response =
        await fetch(
            "/api/deployment/validate",
            {
                method:
                    "POST",

                headers: {
                    "Content-Type":
                        "application/json"
                },

                body:
                    JSON.stringify(
                        hydroponicDeploymentTree)
            });


    const result =
        await response.json();


    if (result.isValid) {

        showMessage(
            "advancedMessage",
            "Hydroponic topology successfully validated.",
            "success");

    }
    else {

        showMessage(
            "advancedMessage",
            result.errors.join(" | "),
            "error");
    }
}


// =====================================
// DASHBOARD SUMMARY
// =====================================

async function loadSummary() {

    const response =
        await fetch(
            "/api/sensors/summary");


    const summary =
        await response.json();


    document
        .getElementById("sensorCount")
        .textContent =
        summary.sensorCount;


    document
        .getElementById("floatCount")
        .textContent =
        summary.floatReadingCount;


    document
        .getElementById("integerCount")
        .textContent =
        summary.integerReadingCount;


    document
        .getElementById("booleanCount")
        .textContent =
        summary.booleanReadingCount;


    const powerElement =
        document
            .getElementById("powerLoad");


    powerElement.textContent =
        summary.combinedPowerWatts;


    if (summary.highPowerLoad) {

        powerElement.classList.add(
            "danger-text");

    }
    else {

        powerElement.classList.remove(
            "danger-text");
    }
}


// =====================================
// ALERT FEED
// =====================================

async function loadAlerts() {

    const response =
        await fetch(
            "/api/sensors/alerts");


    const alerts =
        await response.json();


    const container =
        document
            .getElementById(
                "alertFeed");


    if (alerts.length === 0) {

        container.innerHTML = `

            <div class="empty-state">

                No alerts detected.

            </div>
        `;

        return;
    }


    container.innerHTML =
        alerts.map(alert => `

            <div class="
                alert-item
                ${alert.severity.toLowerCase()}
            ">

                <div>

                    <strong>
                        ${escapeHtml(alert.severity)}
                    </strong>

                    <p>
                        ${escapeHtml(alert.message)}
                    </p>

                    <small>
                        ${escapeHtml(alert.deviceId)}
                    </small>

                </div>

                <time>
                    ${formatDate(alert.timestamp)}
                </time>

            </div>

        `).join("");
}


// =====================================
// REFRESH DASHBOARD
// =====================================

async function refreshDashboard() {

    await Promise.all([

        loadSensors(),

        loadSummary(),

        loadAlerts()

    ]);
}


// =====================================
// INPUT HELPERS
// =====================================

function updateTelemetryPlaceholder() {

    const type =
        document
            .getElementById(
                "telemetryType")
            .value;


    const input =
        document
            .getElementById(
                "telemetryValue");

    const unitInput =
        document
            .getElementById(
                "telemetryUnit");


    if (type === "float") {
        input.placeholder = "6.1";
        unitInput.placeholder = "pH";
    } else if (type === "int") {
        input.placeholder = "1400";
        unitInput.placeholder = "µS/cm";
    } else {
        input.placeholder = "true or false";
        unitInput.placeholder = "ON/OFF";
    }
}


// =====================================
// MESSAGE HELPER
// =====================================

function showMessage(
    elementId,
    message,
    type) {

    const element =
        document
            .getElementById(
                elementId);


    element.textContent =
        message;


    element.className =
        `message-box ${type}`;


    element.style.display =
        "block";
}


// =====================================
// FORMAT HELPERS
// =====================================

function formatCategory(category) {

    if (
        category ===
        "PowerConsumption"
    ) {

        return "Power Consumption";
    }


    return category;
}


function formatDate(date) {

    return new Date(date)
        .toLocaleString();
}


function escapeHtml(value) {

    if (value === null ||
        value === undefined) {

        return "";
    }


    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll("\"", "&quot;")
        .replaceAll("'", "&#039;");
}