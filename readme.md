How to run the regression analysis tool:
Baselinining-tool.exe baseline -p “path-to-source-file” -q “path-to-query” -f “filter-option”
-	Filter option can be “single defect”, “multiple defect”, and “rule passes”. Passing wrong type of argument will set the result type to default filter status, which is showing all result types. 
-	Query files should have file extension of .ql or .qls. If one wants to run multiple queries, they should update/create a .qls file.
-	The path to source file should have a build-dir.cmd build script. If one wants to only build certain drivers/sources, they they should copy the build-dir.cmd to the directory that those selected drivers exist.

Requirements before using the tool
-	Users should download CodeQL CLI and add the codeql.exe file to user PATH in environment variables setting.
-	One should also install Sarif-sdk tool. Installing dotnet Core may be necessary.
-	The path to MSBuild should also be added to PATH. 

Output of running baselining-tool.exe 
-	the output of running the regression tool will be an html file in user’s ApplicationData directory. Ex, C:\Users\t-atamene\AppData\Roaming\WDSBaselining
