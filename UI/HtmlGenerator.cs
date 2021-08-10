using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis.Sarif;

namespace baselining_tool
{
    internal class HtmlGenerator
    {

        private static string stringify(ResultStates rs)
        {

            return (rs == ResultStates.SingleDefect) ? "Single Defect" :
            (rs == ResultStates.MultipleDefect) ? "Multiple Defect" :
            (rs == ResultStates.RulePasses) ? "Rule Passes" :
            "Undetermined";

        }


        internal static void generateHTML(Dictionary<DRRun, List<ResultInfo>> mDict)
        {
            string html = "<html>";
            html += "<head>";
            html += "<h1>Changes Report</h1>";

            html += "<script src=\"https://cdnjs.cloudflare.com/ajax/libs/jquery/3.3.1/jquery.min.js\"></script>";
            html += "<script src=\"https://stackpath.bootstrapcdn.com/bootstrap/4.1.2/js/bootstrap.min.js\"></script>";
            html += "<link rel=\"stylesheet\" href=\"https://stackpath.bootstrapcdn.com/bootstrap/4.1.2/css/bootstrap.min.css\">";
            html += "<link rel=\"stylesheet\" type=\"text/css\" href=\"https://use.fontawesome.com/releases/v5.6.3/css/all.css\">";
            html += "<script type=\"text/javascript\"> function show_hide_row(row) {$(\"#\" + row).toggle();}</script>";

            html += @"
            <style>
                body {
                    margin: 0 auto;
                    padding: 0px;
                    text-align: left;
                    font-family: Helvetica, Arial, Sans-Serif;
                }
                #wrapper {
                    margin: 0 auto;
                    padding: 0px;
                }
                #wrapper h1 {
                    margin-top: 50px;
                    font-size: 45px;
                    color: #585858;
                }
                #wrapper h1 p {
                    font-size: 20px;
                }
                #table_detail {
                    text-align: left;
                    border-collapse: collapse;
                    color: #2E2E2E;
                    border: #A4A4A4;
                }
                #table_detail tr:hover {
                    background-color: #00EEFF;
                }
                #table_detail .hidden_row {
                    display: none; 
                    background-color: #F2F2F2;
                }
            </style>
            ";
            html += "</head> <body>";
            html += "<div id=\"wrapper\">";


            //Table output.
            html += "<table border=1 id=\"table_detail\" align=left cellpadding=10>";
            //Adding HeaderRow.
            html += "<tr>";
            html += "<th style='background-color: #B8DBFD;border: 1px solid #ccc'>Source Path</th>";
            html += "<th style='background-color: #B8DBFD;border: 1px solid #ccc'>Rule Name</th>";
            html += "<th style='background-color: #B8DBFD;border: 1px solid #ccc'>Previous Report</th>";
            html += "<th style='background-color: #B8DBFD;border: 1px solid #ccc'>Current Report</th>";
            html += "</tr>";

            int ctr = 0;
            foreach (DRRun run in mDict.Keys)
            {
                if (run.CurrentState != Program.RS && Program.RS != ResultStates.Undetermined)
                {
                    continue;
                }
                //Adding row data for a Run
                html += "<tr onclick=\"show_hide_row('hidden_row" + ctr + "');\">";
                html += "<td style='border: 1px solid #ccc'>" + run.DriverInformation.Name + "</td>";
                html += "<td style='border: Ï1px solid #ccc'>" + run.RuleInformation.Name + "</td>";
                if (Program.Prev)
                {
                    html += "<td style='border: Ï1px solid #ccc'>" + stringify(run.PreviousState) + "</td>";
                }
                else
                {
                    html += "<td style='border: Ï1px solid #ccc'>" + " Not applicable " + "</td>";

                }
                html += "<td style='border: Ï1px solid #ccc'>" + stringify(run.CurrentState) + "</td>";
                html += "</tr>";

                //header info for a Result

                html += "<tr id=\"hidden_row" + ctr + "\" class=\"hidden_row\"> " +
                            " <td colspan=4> " +
                                " <table border=1 id=\"table_detail\" align=left cellpadding=10>";
                html += "<tr>";
                html += "<th> File Name</th>";
                html += "<th> Line Number</th>";
                html += "<th> Status </th>";
                html += "<th> Rule Id </th>";
                html += "</tr>";

                //load each Result

                foreach (ResultInfo resInfo in mDict[run])
                {
                    html += "<tr>";
                    html += "<td style='border: 1px solid #ccc'>" + resInfo.FileLocation + "</td>";
                    html += "<td style='border: Ï1px solid #ccc'>" + resInfo.LineNum + "</td>";
                    html += "<td style='border: Ï1px solid #ccc'>" + bsStringify(resInfo.BState) + "</td>";
                    html += "<td style='border: Ï1px solid #ccc'>" + resInfo.RuleId + "</td>";
                    html += "</tr>";
                }


                html += "</table> </td> </tr>";

                ctr += 1;
            }
            html += "</table> </div> </body> </html>";

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WDSBaselining");
            Directory.CreateDirectory(path);

            if (File.Exists(Path.Combine(path, "resultHtml.html")))
            {
                File.Delete(Path.Combine(path, "resultHtml.html"));
                BLogger.baselineLogger("Existing HTML result deleted\n");
            }

            File.WriteAllText(path + "/resultHtml.html", html);

            BLogger.baselineLogger("Html Result File Created\n");

            Console.WriteLine("resultHTML created in ApplicationData directory\n");
        }

        private static string bsStringify(BaselineState bState)
        {


            return (bState == BaselineState.Absent) ? "Absent" :
            (bState == BaselineState.New) ? "New" :
            (bState == BaselineState.Unchanged) ? "Unchanged" :
            (bState == BaselineState.Updated) ? "Updated" :
            "None";


        }
    }
}