import 'https://res.cdn.office.net/teams-js/2.24.0/js/MicrosoftTeams.min.js'
let teamsAppID='';
let teamsInitPromise;
async function ensureTeamsSdkInitialized() {
    if (!teamsInitPromise) {
        teamsInitPromise = microsoftTeams.app.initialize();
    }
    return teamsInitPromise;
}
async function displayUI() {

    await ensureTeamsSdkInitialized();
    const urlParams = new URLSearchParams(window.location.search);   
    if (urlParams) {
        const paramObj = urlParams.get('p');  
        teamsAppID=urlParams.get('appId');       
        if (paramObj) {
            const obj = JSON.parse(paramObj);
            document.getElementById('companyName').value = obj.companyName ? obj.companyName : "";
            document.getElementById('contactName').value = obj.contactName ? obj.contactName : "";
            document.getElementById('contactTitle').value = obj.contactTitle ? obj.contactTitle : "";
            document.getElementById('address').value = obj.address ? obj.address : "";
            document.getElementById('city').value = obj.city ? obj.city : "";           
        }     
    }
   
    microsoftTeams.app.initialize().then(() => {
       
        document.getElementById('supplierForm').addEventListener("submit", async (e) => {
            let supplierInfo = {
                companyName: document.forms["supplierForm"]["companyName"].value,
                contactName: document.forms["supplierForm"]["contactName"].value,
                contactTitle: document.forms["supplierForm"]["contactTitle"].value,
                address: document.forms["supplierForm"]["address"].value,
                city: document.forms["supplierForm"]["city"].value,               
                action: "submit"
            };
            await microsoftTeams.dialog.url.submit(supplierInfo,teamsAppID);
            return true;
        });
    });
}
displayUI();

