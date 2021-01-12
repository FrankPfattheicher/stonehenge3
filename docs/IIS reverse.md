
## Use IIS as Reverse Proxy


### 1. Install the Application Request Routing (ARR) Extension

For details, see http://www.iis.net/downloads/microsoft/application-request-routing.


### 2. Open the IIS Manager

* Create a new IIS website, or use the default website.
* Select the URL Rewrite Icon.
* Chose the ‘Add Rule’ action from the right pane.
* Select the ‘Reverse Proxy Rule’ from the ‘Inbound and Outbound Rules’ category.
* Enter localhost:*portNumber* in filed "Enter the server name ... where requests will be forwarded:". 
* Optionally check "Enable SSL Offloading" to let IIS handle SSL.

