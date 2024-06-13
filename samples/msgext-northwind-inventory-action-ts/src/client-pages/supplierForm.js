import 'https://res.cdn.office.net/teams-js/2.24.0/js/MicrosoftTeams.min.js'
async function displayUI() {
   
    microsoftTeams.app.initialize().then(() => {  
        document.getElementById('supplierForm').addEventListener("submit", async (e) => {
        let supplierInfo = {
            companyName: document.forms["supplierForm"]["companyName"].value,
            contactName: document.forms["supplierForm"]["contactName"].value,
            action:"submit"
        };     
        await microsoftTeams.dialog.url.submit(supplierInfo,"e137b792-8945-4a20-963d-c511532c1dbd");
        return true;
    });
});
}
displayUI();
