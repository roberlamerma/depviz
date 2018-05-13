# depviz
Item dependency visualization tool, based on [Graphviz](https://www.graphviz.org/).

## Full description
Tool that allows a graphical visualization of dependencies between items (i.g. nodes on a directed graph) using [Graphviz](https://www.graphviz.org/), without the need of using the [Dot language](https://www.graphviz.org/doc/info/lang.html).

## How it does it?
The tool uses different (currently **TFS** and **CSV**) repositories in order to obtain the **Items** (nodes), its **Successors**, **States** and **Tags**.

* **Items**: the "box" with its ID and Title.
* **Succesors**: the successor (connected with an arrow) "box"
* **States**: possible states (the default are the TFS ones: _New_, _Approved_, _Committed_, _In Progress_ and _Done_. These are used to color the "box"
* **Tags**: item tags. These are used to change the shape/line of the "box"

# Setup 
## Installation
:information_source: _At some point I will create an installer_

Download the binaries (or build the tool yourself)

## Configuration

### TFS

1. Open the tool
1. File -> Select Connector... -> TFS
1. Configure TFS: ![Configure TFS](images/tfs_config.png)
   1. _(You could also go to: File -> Configure (TFS))_
   1. Add your TFS **uri** (i.g. http://yourtfs:8080/tfs/defaultcollection) and the **project name** you want to connect to.

:information_source: You can also achieve this by editing the **userSettings.tfsUrl** tag, on the **depviz.exe.config** file, before opening **depviz.exe**

### CSV

1. Open the tool
1. File -> Select Connector... -> CSV
1. Select CSV file: ![Select CSV file](images/csv_config.png)


# Add support for other repositories
