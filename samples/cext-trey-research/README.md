# Trey Research Copilot Declarative Agent (anonymous version)

Trey Research is a fictitious consulting company that supplies talent in the software and pharmaceuticals industries. The vision for this demo is to show the full potential of Copilot extensions in a relatable business environment.

> NOTE: Declarative agents were called "declarative copilots" during private preview; you may find the old terminology in some of our documentation and tools as they are being updated.

> NOTE: This version of the Trey Research sample doesn't do authentication, but may be useful for demos
and experimentation. See the [cext-trey-research-auth](../cext-trey-research-auth/) sample for a version that works with an authenticated API.

The solution consists of an API plugin that calls a set of Azure functions, which store the consulting data in a Azure Table storage (it uses the Azurite storage emulator when running locally).
A declarative agent is provided to converse with users and to call the API plugin, as well as to reference the correct SharePoint document library.

### Prompts that work

  * what projects am i assigned to?
    (NOTE: In this "anonymous" version of the sample, the user is assumed to be consultant "Avery Howard". If Copilot decides to request information using your real name, the request will fail. Unless your name happens to be "Avery Howard".)
  * what projects is domi working on?
  * do we have any consultants with azure certifications?
  * what projects are we doing for relecloud?
  * which consultants are working with woodgrove bank?
  * how many hours has avery delivered this month?
  * please find a consultant with python skills who is available immediately
  * are any consultants available who are AWS certified? (multi-parameter!)
  * does trey research have any architects with javascript skills? (multi-parameter!)
  * what designers are working at woodgrove bank? (multi-parameter!)
  * please charge 10 hours to woodgrove bank (POST request)
  * please add sanjay to the contoso project (POST request with easy to forget entities, hoping to prompt the user; for now they are defaulted)

If the sample files are uploaded, these will work as well:

   * find my hours spreadsheet and get the hours for woodgrove, then bill the client
   * make a list of my projects, then write a summary of each based on the statement of work.

## Plugin Features

The sample showcases the following features:

  1. Declarative agent with branding and instructions, access to relevant SharePoint documents and the API plugin
  1. API based plugin works with any platform that supports REST requests
  1. Copilot will construct queries for specific data using GET requests
  1. Copilot updates and adds data using POST requests
  1. Multi-parameter queries to filter results
  1. Show a confirmation card before POSTing data; capture missing parameters
  1. Display rich adaptive cards

## Setup

### Prerequisites

* [Visual Studio Code](https://code.visualstudio.com/Download)
* [NodeJS 18.x](https://nodejs.org/en/download)
* [Teams Toolkit extension for VS Code](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension)
  NOTE: If you want to build new projects of this nature, you'll need Teams Toolkit v5.6.1-alpha.039039fab.0 or newer
* [Teams Toolkit CLI](https://learn.microsoft.com/microsoftteams/platform/toolkit/teams-toolkit-cli?pivots=version-three)
  (`npm install -g @microsoft/teamsapp-cli`)
* (optional) [Postman](https://www.postman.com/downloads/)

### Setup instructions (one-time setup)

1. Log into Teams Toolkit using the tenant where you will run the sample.

1. If your project doesn't yet have a file **env/.env.local.user**, then create one by copying **env/.env.local.user.sample**. If you do have such a file, ensure it includes these lines.

~~~text
SECRET_STORAGE_ACCOUNT_CONNECTION_STRING=UseDevelopmentStorage=true
~~~

1. OPTIONAL: Copy the files from the **/sampleDocs** folder to OneDrive or SharePoint. Add the location of these files in the `OneDriveAndSharePoint` capability in the declarative copilot (**/appPackage/trey-declarative-copilot.json**).

### Running the solution (after each build)

1. Press F5 to start the application. It will take a while on first run to download the dependencies. Eventually a browser window will open up and your package is installed.

2. Navigate to Copilot as shown below 1️⃣
![Running in Copilot](./assets/images/startsample.png)

3. Access the declarative agent by opening the flyout 4️⃣, then select the Trey Genie Local solution 5️⃣.

## API Summary

![postman](https://voyager.postman.com/logo/postman-logo-icon-orange.svg) 

We have a [Postman collection](https://documenter.getpostman.com/view/5938178/2sA3JJ8hfn) for you to try out the APIs. It's a great way to get to know the data that Copilot is accessing.

All API operations are included in the collection, with parameters and body provided to make it easier for you to test our GET and POST calls. 

> Make sure you have [Postman desktop](https://www.postman.com/downloads/) to be able to test urls with `localhost` domain. 
Or simply replace part of the URL `http://localhost:7071` with your tunnel/host URL.

#### GET Requests

~~~javascript

 GET /api/me/ - get my consulting profile and projects

GET /api/consultants/ - get all consultants
// Query string params can be used in any combination to filter results
GET /api/consultants/?consultantName=Avery - get consultants with names containing "Avery"
GET /api/consultants/?projectName=Foo - get consultants on projects with "Foo" in the name
GET /api/consultants/?skill=Foo - get consultants with "Foo" in their skills list
GET /api/consultants/?certification=Foo - get consultants with "Foo" in their certifications list
GET /api/consultants/?role=Foo - get consultants who can serve the "Foo" role on a project
GET /api/consultants/?availability=x - get consultants with x hours availability this month or next month
~~~

The above requests all return an array of consultant objects, which are defined in the ApiConsultant interface in /model/apiModel.ts.

~~~javascript
GET /api/projects/ - get all projects
// Query string params can be used in any combination to filter results
GET /api/projects/?projectName=Foo - get projects with "Foo" in the name
GET /api/projects/?consultantName=Avery - get projects where a consultant containing "Avery" is assigned

~~~

The above requests all return an array of project objects, which are defined in the ApiProject interface in /model/apiModel.ts.

#### POST Requests

~~~javascript
POST /api/me/chargeTime - Add hours to project with "Foo" in the name

Request body:
{
  projectName: "foo",
  hours: 5
}
Response body:
{
    status: 200,
    message: "Charged 3 hours to Woodgrove Bank on project \"Financial data plugin for Microsoft Copilot\". You have 17 hours remaining this month";
}

POST /api/projects/assignConsultant - Add consultant to project with "Foo" in the name
Request body:
{
    projectName: "foo",
    consultantName: "avery",
    role: "architect",
    forecast: number
}
Response body:
{
    status: 200
    message: "Added Alice to the \"Financial data plugin for Microsoft Copilot\" project at Woodgrove Bank. She has 100 hours remaining this month.";
}
~~~

## API Design considerations

The process began with a bunch of sample prompts that serve as simple use cases for the service. The API is designed specifically to serve those use cases and likely prompts. In order to make it easier for use in the RAG orchestration, the service:

1. Completes each prompt / use case in a single HTTP request

    * accept names or partial names that might be stated in a user prompt rather than requiring IDs which must be looked up
    * return enough information to allow for richer responses; err on the side of providing more detail including related entities

2. For best Copilot performance, limit the number of parameter options to 10-15 

3. Ensure that parameters, properties, messages, etc. are human readable, as they will be interpreted by a large language model

4. Return all the data Copilot might need to fulfull a user prompt. For example, when retrieving a
consultant, the API has no way to know if the user was seeking the consultant's skills, location, project list, or something else. Thus, the API returns all this information.

5. In GET requests, use the resource that corresponds to the entity the user is asking for. Don't expect Copilot to figure out that some data is buried in another entity.

6. In POST requests, use a command style such as `/me/chargeTime`, as opposed to asking the API to update a data structure

7. Don't expect Copilot to filter data; instead provide parameters and filter it server side. (I have seen some filtering by Copilot however - this is for further study)