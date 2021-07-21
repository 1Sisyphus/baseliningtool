using System;
using System.Collections.Generic;
using System.IO;

namespace baselining_tool
{
    internal class HtmlGenerator
    {

        public static void generateHTML(List<Run> runList)
        {
            //Table output.
            string html = "<table cellpadding='5' cellspacing='0' style='border: 1px solid #ccc;font-size: 9pt;font-family:arial'>";

            //Adding HeaderRow.
            html += "<tr>";
            html += "<th style='background-color: #B8DBFD;border: 1px solid #ccc'>Driver name</th>";
            html += "<th style='background-color: #B8DBFD;border: 1px solid #ccc'>Rule name</th>";
            html += "<th style='background-color: #B8DBFD;border: 1px solid #ccc'>Previous Result</th>";
            html += "<th style='background-color: #B8DBFD;border: 1px solid #ccc'>Current Result</th>";
            html += "</tr>";

            for (int i = 0; i < runList.Count; i++)
            {
                //Adding row data.
                html += "<tr>";
                html += "<td style='width:120px;border: 1px solid #ccc'>" + runList[i].DriverInformation.Name + "</td>";
                html += "<td style='width:120px;border: Ï1px solid #ccc'>" + runList[i].RuleInformation.Name + "</td>";
                html += "<td style='width:120px;border: Ï1px solid #ccc'>" + runList[i].PreviousState + "</td>";
                html += "<td style='width:120px;border: Ï1px solid #ccc'>" + runList[i].CurrentState + "</td>";

                html += "</tr>";
            }
            html += "</table>";

            string filePath = AppDomain.CurrentDomain.BaseDirectory;
            if (File.Exists(filePath + "resultHtml.html"))
            {
                File.Delete(filePath + "resultHtml.html");
                BLogger.baselineLogger("Existing HTML result deleted");
            }

            File.WriteAllText(filePath + "resultHtml.html", html);

            BLogger.baselineLogger("Html Result File Created");
        }

    }
}