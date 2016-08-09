LoadFlow
========
LoadFlow - a framework for load flow calculation. Node voltages can be calculated with Current Iteration, Newton-Raphson, Fast-decoupled-load-flow and Holomorphic Embedding Load Flow.
As interface currently supported are a MS SQL database and the file format of PSS SINCAL.

### How do I build this software? ###

The whole project can be built using Visual Studio 2012. The solution contains several projects, including:

 * Interfaces/SincalConnector: a dedicated application which can be used to calculate power nets from PSS SINCAL
 * Interfaces/DatabaseUI: a UI for the custom file format, stored in a MS SQL database
 * Core/Calculation: core library, which implements the load flow calculation itself

### How do I use this software? ###

#### SincalConnector ####
The SincalConnector is a user interface for loading and calculating power nets from PSS SINCAL. The usage is pretty straightforward, it is necessary to open the Access database of a PSS SINCAL file (the *.mdb file in the *_files folder) and to select a calculation method. After pressing the Calculate button the node voltages are calculated and written back into the Access database.

#### DatabaseUI ####
This user interface enables the usage of the custom file format, which is stored in a MS SQL database. Thus, it is necessary to have access to a MS SQL Server (the Express edition is sufficient). After such a database is selected and opened one can add, modify and delete elements through the tables in the UI. The calculation results are stored in a separate table, also displayed in the UI.

#### Calculation ####
The project Calculation is a class-library, and it would be the starting point for custom projects and applications which should leverage the already implemented methods. For further information regarding the usage I would like to refer to the unit and integration tests.

To use either the SincalConnector or the DatabaseUI you have to select the respective project in the solution as the startup project.