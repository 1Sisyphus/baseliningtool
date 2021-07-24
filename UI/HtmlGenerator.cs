using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis.Sarif;

namespace baselining_tool
{
    internal class HtmlGenerator
    {


        internal static void generateHTML(Dictionary<DRRun, List<ResultInfo>> mDict)
        {

            string html = "<h1>Changes Report</h1>";
            //Table output.
            html += "<table cellpadding='5' cellspacing='0' style='border: 1px solid #ccc;font-size: 9pt;font-family:arial'>";

            //Adding HeaderRow.
            html += "<tr>";
            html += "<th style='background-color: #B8DBFD;border: 1px solid #ccc'>Driver Name</th>";
            html += "<th style='background-color: #B8DBFD;border: 1px solid #ccc'>Rule Name</th>";
            html += "<th style='background-color: #B8DBFD;border: 1px solid #ccc'>Previous Baseline State</th>";
            html += "<th style='background-color: #B8DBFD;border: 1px solid #ccc'>Current Baseline State</th>";

            html += "</tr>";

            foreach (DRRun run in mDict.Keys)
            {
                //Adding row data.
                html += "<tr>";
                html += "<td style='border: 1px solid #ccc'>" + run.DriverInformation.Name + "</td>";
                html += "<td style='border: Ï1px solid #ccc'>" + run.RuleInformation.Name + "</td>";
                html += "<td style='border: Ï1px solid #ccc'>" + stringify(run.PreviousState) + "</td>";
                html += "<td style='border: Ï1px solid #ccc'>" + stringify(run.CurrentState) + "</td>";

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

        private static string stringify(ResultStates rs){
                
                return (rs == ResultStates.SingleDefect) ? "Single Defect" : 
                (rs == ResultStates.MultipleDefect)? "Multiple Defect" : 
                (rs == ResultStates.RulePasses)? "Rule Passes" : 
                "Undetermined";

        }
    }
}