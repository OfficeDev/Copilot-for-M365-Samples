import 'https://res.cdn.office.net/teams-js/2.24.0/js/MicrosoftTeams.min.js'
import config from '../config.js';

async function displayUI() {
    const urlParams = new URLSearchParams(window.location.search);
    if (urlParams) {
        const paramObj = urlParams.get('p');
        if (paramObj) {
            const obj = JSON.parse(paramObj);
            document.getElementById('companyName').value = obj.companyName ? obj.companyName : "";
            document.getElementById('contactName').value = obj.contactName ? obj.contactName : "";
        }
    }
    microsoftTeams.app.initialize().then(() => {
        document.getElementById('supplierForm').addEventListener("submit", async (e) => {
            let supplierInfo = {
                companyName: document.forms["supplierForm"]["companyName"].value,
                contactName: document.forms["supplierForm"]["contactName"].value,
                action: "submit"
            };
            await microsoftTeams.dialog.url.submit(supplierInfo, config.teamsAppId);
            return true;
        });
    });
}
displayUI();

