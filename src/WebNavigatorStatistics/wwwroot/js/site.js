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

        let ipCell = row.insertCell(2);
        ipCell.appendChild(document.createTextNode(data.ipAddress));

        let timeCell = row.insertCell(3);
        timeCell.appendChild(document.createTextNode(httpStat.time));

        let statusCode2XXCell = row.insertCell(4);
        statusCode2XXCell.appendChild(document.createTextNode(httpStat.statusCode2XX));

        let statusCode3XXCell = row.insertCell(5);
        statusCode3XXCell.appendChild(document.createTextNode(httpStat.statusCode3XX));

        let statusCode4XXCell = row.insertCell(6);
        statusCode4XXCell.appendChild(document.createTextNode(httpStat.statusCode4XX));

        let statusCode5XXCell = row.insertCell(7);
        statusCode5XXCell.appendChild(document.createTextNode(httpStat.statusCode5XX));

        let lastUpdatedCell = row.insertCell(8);
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

    // Totals accumulators
    let total2XX = 0;
    let total3XX = 0;
    let total4XX = 0;
    let total5XX = 0;

    for (let i = 0; i < stats.length; i++) {
        addTableRow(statsTableElement, stats[i]);
        // Sum up all status codes for totals
        for (let j = 0; j < stats[i].httpStats.length; j++) {
            let httpStat = stats[i].httpStats[j];
            total2XX += Number(httpStat.statusCode2XX) || 0;
            total3XX += Number(httpStat.statusCode3XX) || 0;
            total4XX += Number(httpStat.statusCode4XX) || 0;
            total5XX += Number(httpStat.statusCode5XX) || 0;
        }
    }

    // Add Totals row at the end
    let totalsRow = statsTableElement.insertRow(-1);

    let totalsLabelCell = totalsRow.insertCell(0);
    totalsLabelCell.colSpan = 4;
    totalsLabelCell.style.fontWeight = "bold";
    totalsLabelCell.appendChild(document.createTextNode("Totals"));

    let totals2XXCell = totalsRow.insertCell(1);
    totals2XXCell.appendChild(document.createTextNode(total2XX));

    let totals3XXCell = totalsRow.insertCell(2);
    totals3XXCell.appendChild(document.createTextNode(total3XX));

    let totals4XXCell = totalsRow.insertCell(3);
    totals4XXCell.appendChild(document.createTextNode(total4XX));

    let totals5XXCell = totalsRow.insertCell(4);
    totals5XXCell.appendChild(document.createTextNode(total5XX));

    let totalSum = total2XX + total3XX + total4XX + total5XX;
    let totalsSumCell = totalsRow.insertCell(5);
    totalsSumCell.style.fontWeight = "bold";
    totalsSumCell.appendChild(document.createTextNode(totalSum));

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
