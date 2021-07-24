Documentation for baseliningtool go here:

-----------------------------

current command looks like:

baselineing-tool baseline -p "path-to-file" -f "filter-option"

arguments for -f can be:
* "new"
* "absent"
* "updated"
* "unchanged"
* "all"



-----------------------------
Next Task

- Filter results by property/name ...
- handle other kinds of defects like Not Applicable, Not error due to known limitation, ...
- Marks for a change, ex, a new baseline state can refer to two things. Either a query was updated or a new bug was introduced in driver code. Next taks would be creating a column for this information in the output file and figuring out how to determine which of these cases a new baselining state refers to.


----------------------------

updated plan while running baseline-tool

- previous: baselining-tool baseline -p "path-to-baseline-file" -f "output-option"

- target: baselining-tool baseline -p "directory-to-analysis-file" -f "output-option"

- stretch: accepting directory to queries and database. All of these assume previous installation of Sarif-Sdk and CodeQL CLI

command for stretch case : 
1) baselining-tool analyseAndBaseline -p "direcotry-to-database" -q "directory-to-query" -f "filter-option"
2) baselining-tool baseline -p "path-to-baseline-file" -f "ouput-option" 