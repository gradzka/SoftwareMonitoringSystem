# SoftwareMonitoringSystem

## Overview
Management Information System (MIS) for monitoring installed software on computers in LAN. Follows client-server architecture.  Server app implemented in ASP.NET MVC5 technology and available in Polish. Link to client app: https://github.com/Tiamoon/SoftwareMonitoringClient.

## Description
Monitoring in the meaning of collecting data about the installed software and checking the availability of devices in LAN.

Client computers send data on server request. The system administrator can start scanning computers, view and manage obtained information through the web application.

Data obtained from computers is stored in JSON files which name is date and time of scanning. The files are located in the folder named the MAC address of the device.

Data related to devices, history of scans and operations performed in the web application are stored in the local database. More information in Dokumentacja.pdf.

### Third Party Libraries
- Bootstrap v3.4.1
- EntityFramework v6.2.0
- System.Data.SQLite v1.0.108
- Newtonsoft.Json v6.0.4


![Home view](https://github.com/gradzka/SoftwareMonitoringSystem/blob/master/SoftwareMonitoringSystem/SoftwareMonitoringSystem/Assets/Screenshots/image18.png)
![Defined devices and agents](https://github.com/gradzka/SoftwareMonitoringSystem/blob/master/SoftwareMonitoringSystem/SoftwareMonitoringSystem/Assets/Screenshots/image7.png)
![Device edition](https://github.com/gradzka/SoftwareMonitoringSystem/blob/master/SoftwareMonitoringSystem/SoftwareMonitoringSystem/Assets/Screenshots/image4.png)
![Scan details](https://github.com/gradzka/SoftwareMonitoringSystem/blob/master/SoftwareMonitoringSystem/SoftwareMonitoringSystem/Assets/Screenshots/image10.png)
![Scans hictory](https://github.com/gradzka/SoftwareMonitoringSystem/blob/master/SoftwareMonitoringSystem/SoftwareMonitoringSystem/Assets/Screenshots/image17.png)
![Restoring factory settings](https://github.com/gradzka/SoftwareMonitoringSystem/blob/master/SoftwareMonitoringSystem/SoftwareMonitoringSystem/Assets/Screenshots/image14.png)

## Attributions
- https://stackoverflow.com/a/24814027
- https://stackoverflow.com/a/36083042
- https://stackoverflow.com/a/32555021
- https://stackoverflow.com/a/24505794
- https://codepen.io/cbracco/pen/zekgx
- https://stackoverflow.com/a/25160044 
- https://stackoverflow.com/a/6941582

## Credits
* Monika Grądzka
* Robert Kazimierczak
* Kamil Szulc (client app)
