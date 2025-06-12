let protocol = new signalR.JsonHubProtocol();
let hubRoute = "ServerStatistics";
let connection = new signalR.HubConnectionBuilder()
    .withUrl(hubRoute)
    .withAutomaticReconnect()
    .withHubProtocol(protocol)
    .build();

connection.start()
    .then(() => {
        console.log("Connected!");
    })
    .catch((err) => {
        console.log(err);
    });

const addTableRow = (table, data) => {

    for (let i = 0; i < data.httpStats.length; i++) {

        let row = table.insertRow(1);
        let httpStat = data.httpStats[i];

        let locationCell = row.insertCell(0);
        locationCell.appendChild(document.createTextNode(data.location));

        let machineNameCell = row.insertCell(1);
        machineNameCell.appendChild(document.createTextNode(data.machineName));

        let timeCell = row.insertCell(2);
        timeCell.appendChild(document.createTextNode(httpStat.time));

        let statusCode2XXCell = row.insertCell(3);
        statusCode2XXCell.appendChild(document.createTextNode(httpStat.statusCode2XX));

        let statusCode3XXCell = row.insertCell(4);
        statusCode3XXCell.appendChild(document.createTextNode(httpStat.statusCode3XX));

        let statusCode4XXCell = row.insertCell(5);
        statusCode4XXCell.appendChild(document.createTextNode(httpStat.statusCode4XX));

        let statusCode5XXCell = row.insertCell(6);
        statusCode5XXCell.appendChild(document.createTextNode(httpStat.statusCode5XX));

        let lastUpdatedCell = row.insertCell(7);
        lastUpdatedCell.appendChild(document.createTextNode(data.lastUpdated));
    }
}

connection.on("UpdateStats", stats => {
    console.log("Statistics:");
    console.log(stats);

    const statsElement = document.getElementById("stats");
    const statsTableElement = document.getElementById("statsTable");

    while (statsTableElement.rows.length > 1) {
        statsTableElement.deleteRow(-1);
    }

    for (let i = 0; i < stats.length; i++) {
        addTableRow(statsTableElement, stats[i]);
    }

    let totalClients = 0;
    let machineNames = new Set();
    for (let i = 0; i < stats.length; i++) {
        if (!machineNames.has(stats[i].machineName)) {
            machineNames.add(stats[i].machineName);
            totalClients++;
        }
    }

    statsElement.innerHTML = `${totalClients} total connected clients`;
});
