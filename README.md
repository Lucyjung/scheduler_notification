# Scheduler Notification 
This is a quick solution for send notification periodically via LINE Application for monitoring something (eg. status from excel file or SQL Database



## What do we have here
1. Utilities for HTTP request, LINE notify, Excel to DataTable, SQL, Timer 
2. Logic to read status for each business 




## Flow diagrams

Here is the overall flow for this application

```mermaid
sequenceDiagram
Initialization ->> Utility Config: Read Configuration Parameter
Initialization-->>Utility Scheduler: Enable Scheduler
loop Every Interval
Utility Scheduler-->>Callback: Trigger....
end

Callback->> RPA: Get the Status
RPA->> Utility ExcelData/SQL: Read the Status from Excel or SQL
RPA->> Callback: Return the status in String Array
Callback->> Utility LINE: Send Notification
Utility LINE->> Utility HTTP Request: Send HTTP Request 
```
To render diagram : [Stackedit](https://stackedit.io/app#)
Reference for Diagram : [Mermaid](https://mermaidjs.github.io/)
```