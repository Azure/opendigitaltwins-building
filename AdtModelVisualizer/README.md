# ADT Model Visualizer

Azure Digital Twins (ADT) Model Visualizer is a .NET core web application that allows you to visually view your DTDL model(s) in ADT. 

It visually depicts:

- Interfaces
- Interfaces that extend other Interfaces
- Relationships between Interfaces

You can also click on the Interface icons to view the Interface's DTDL.

![ADT Model Visualizer](ADTModelVisualizer.JPG)

## Prerequisites

- Create an Azure Digital Twins service instance with an Azure Active Directory client app registration according to the article [https://docs.microsoft.com/en-us/azure/digital-twins/how-to-set-up-instance-portal](https://docs.microsoft.com/en-us/azure/digital-twins/how-to-set-up-instance-portal). A few important aspects for your app registrations:
  - Make sure you add app registrations to the Web platform section of the app registration, not the desktop/mobile section.
  - When adding callback URLs to the app registration, please make sure to add `https://localhost:5001`.
  - Check the **Access Tokens** toggle in the **Implicit Grants** section a few paragraphs below the **Platform Configuration** section on the page. If this toggle is not checked, you will not get authorization tokens.
- Upload your DTDL model to your ADT instance using the [ADT CLI](https://docs.microsoft.com/en-us/cli/azure/ext/azure-iot/dt/model?view=azure-cli-latest#ext_azure_iot_az_dt_model_create), [REST API](https://docs.microsoft.com/en-us/rest/api/digital-twins/dataplane/models), or [SDKs](https://docs.microsoft.com/en-us/azure/digital-twins/how-to-manage-model#upload-models).

## Run ADT Model Visualizer locally

- Clone the repo
- In the ```ADTModelVisualizer``` folder, edit the ```appsettings.Development.json``` file to match your ADT instance and Azure Active Directory client app registration settings:
```
  "TenantId": "11111111-1111-1111-1111-111111111111",
  "ClientId": "22222222-2222-2222-2222-222222222222",
  "ClientSecret": "<your client secret>",
  "AdtApiUrl": "https://youradt.api.wus2.digitaltwins.azure.net"
```
- Build and run the application:
  - ```dotnet build```
  - ```dotnet run```
- Browse to [https://localhost:5001](https://localhost:5001)

## Credit
The ADT Model Visualizer was developed in collaboration with [Willow](https://www.willowinc.com/).
