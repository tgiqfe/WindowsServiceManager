
using System.ServiceProcess;
using System.Text.Json;
using WindowsService.WindowsService;

/*
var summaries = ServiceItem.Load();
var simpleSummaries = ServiceSummary.Load();

var json1 = JsonSerializer.Serialize(summaries,
    new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });
var json2 = JsonSerializer.Serialize(simpleSummaries,
    new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });

Console.WriteLine(json1);
Console.WriteLine(json2);
*/

var item = new ServiceItem("Fax");
item.ChangeStartupType("Disable");


Console.ReadLine();
