# depviz
Item dependency visualization tool, based on [Graphviz](https://www.graphviz.org/).

## Full description
Tool that allows a graphical visualization of dependencies between items (i.g. nodes on a directed graph) using [Graphviz](https://www.graphviz.org/), without the need of using the [Dot language](https://www.graphviz.org/doc/info/lang.html).

**TFS:** ![Configure TFS](images/tfs_example1.png)
**CSV:** ![Configure TFS](images/csv_example1.png)

## How does it do it?
The tool uses different (currently **TFS** and **CSV**) repositories in order to obtain the **Items** (nodes), its **Successors**, **States** (optional), **Tags** (optional) and **Comments** (optional).

* **Items**: the "box" with its ID and Title.
* **Succesors**: the successor (connected with an arrow) "box"
* **States**: possible states (the default are the TFS ones: _New_, _Approved_, _Committed_, _In Progress_ and _Done_. These are used to color the "box"
* **Tags**: item tags. These are used to change the line/color of the line "box"
* **Comments**: comments. If present, it is an extra text shown on the node (currently on TFS connector is the Iteration/Sprint)

# Getting Started

## Prerequisites
* Windows 7 or 10
* Install [Graphviz](https://graphviz.gitlab.io/download/)
   * Annotate the binaries installation path (i.g. C:\Program Files (x86)\Graphviz2.38\bin)
* Install the [Team Foundation Server 2012 Update 4 Object Model](https://marketplace.visualstudio.com/items?itemName=ErinDormierMSFT.TeamFoundationServer2012Update4ObjectModelInstalle)
   * If you experience problems installing the Object Model 2012 (above), then install first [Visual Studio Team Foundation Server 2012 - Update 4](https://www.microsoft.com/es-es/download/details.aspx?id=38185). Thanks *Antonio J. Ríos* for figuring this out! 

## Installation
1. [Download the binaries](https://github.com/roberlamerma/depviz/releases/download/v0.3.5/Depviz_v0.3.5.zip)
1. Or build the tool yourself

## Configuration
1. Start the tool. You will be prompted for the Graphviz path (select the path 'bin', which is where the binaries that the tool uses are located)
1. Depending on the repository/connector you will use (TFS, CSV, ...), follow the instructions below:

### TFS
1. Open the tool
1. File -> Select Connector... -> TFS
1. Configure TFS: ![Configure TFS](images/tfs_config.png)
   1. _(You could also go to: File -> Configure (TFS))_
   1. Add your TFS **uri** (i.g. http://yourtfs:8080/tfs/defaultcollection) and the **project name** you want to connect to.

### CSV
1. Open the tool
1. File -> Select Connector... -> CSV
1. Select CSV file: ![Select CSV file](images/csv_config.png)

# Add support for other repositories
You could write your own "connector" (and connect the tool with [Jira](https://www.atlassian.com/software/jira), for example). Contact me if you have doubts on how to do this.
