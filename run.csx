#r "Microsoft.Azure.WebJobs.Extensions.Storage"
#r "Newtonsoft.Json"

using Microsoft.Azure.WebJobs.Host.Bindings.Runtime;
using Microsoft.Azure.WebJobs.Extensions.Storage;
using System.Dynamic;

using Newtonsoft.Json;


public static void Run(Stream myBlob, string filename, Microsoft.Azure.WebJobs.Binder binder, ILogger log)
{
    log.LogInformation($"C# Blob trigger function Processed blob\n Name:{filename} \n Size: {myBlob.Length} Bytes");
    StreamReader sr = new StreamReader(myBlob);
    string jsonstring = sr.ReadToEnd();
    var result = JsonToCsv(jsonstring,",",log);

    using (var writer = binder.Bind<TextWriter>(new BlobAttribute("csv/"+filename.Replace(".json",".csv"))))
    {
        writer.Write(result);
    }

    
}
public static void DumpObject(dynamic thing, ILogger log, string parentName, ref string header, ref string value){
    foreach (var property in (IDictionary<String, Object>)thing)
{
    log.LogInformation(property.Key + ": " + property.Value);
log.LogInformation(value);
    if(property.Value.GetType().ToString() == "System.Dynamic.ExpandoObject"){
        DumpObject(property.Value,log, property.Key, ref header, ref value);
    }else{
        if(String.IsNullOrEmpty(parentName)){
           header +=   property.Key+",";
        }else{
        header += parentName+"-"+property.Key+",";
        }
        value +=  property.Value+",";
    }
}
}
public static string JsonToCsv(string jsonContent, string delimiter, ILogger log)
    {

        dynamic expandos = JsonConvert.DeserializeObject<ExpandoObject>(jsonContent);
        string header = "";
        string value = "";
       DumpObject(expandos,log,"",ref header, ref value);
       header = header.TrimEnd(',');
       value = value.TrimEnd(',');
       log.LogInformation(header);
       log.LogInformation(value);
       return header+"\n"+value;
    }
