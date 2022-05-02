using System.Text;
using System.Text.Json;
using System.Reflection;

//Code for translation
//  https://docs.microsoft.com/en-us/azure/cognitive-services/translator/quickstart-translator?tabs=csharp

string key = "YOUR-KEY"; // Key from azure
string endpoint = "YOUR-ENDPOINT"; // End Point from azure
string location = "westeurope"; // Your API Location E.G "westeurope"

string route = "/translate?api-version=3.0&from=en&to=de";

var student = new Student { Name = "Hello World" };
Console.WriteLine(await Localize<Student>(student));



// Some cool Reflection code that works!
async ValueTask<T> Localize<T>(T MyObject) where T : SomeOpject
{
    PropertyInfo[] propertyInfos;
    propertyInfos = typeof(T).GetProperties();
    foreach (PropertyInfo propertyInfo in propertyInfos)
        MyObject[propertyInfo.Name] = await Translate((string)MyObject[propertyInfo.Name]);
    return MyObject;
}

async ValueTask<string> Translate(string textToTranslate)
{

    object[] body = new object[] { new { Text = textToTranslate } };
    var requestBody = JsonSerializer.Serialize(body);
    using (var client = new HttpClient())
    using (var request = new HttpRequestMessage())
    {
        // Build the request.
        request.Method = HttpMethod.Post;
        request.RequestUri = new Uri(endpoint + route);
        request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        request.Headers.Add("Ocp-Apim-Subscription-Key", key);
        request.Headers.Add("Ocp-Apim-Subscription-Region", location);
        // Send the request and get response.
        HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
        // Read response as a string.
        string result = await response.Content.ReadAsStringAsync();
        var Desrialized = JsonSerializer.Deserialize<Container[]>(result);
        return Desrialized[0].translations[0].text;
    }
}

// Used for Translation 
public record class Container
{
    public Translations[] translations { get; set; }
}
public record class Translations
{
    public string text { get; set; }
    public string to { get; set; }
}


// Used for Reflication 
public record class Student : SomeOpject
{
    public string Name { get; set; }

}
interface SomeOpject
{
    public object this[string propertyName]
    {
        get { return this.GetType().GetProperty(propertyName).GetValue(this, null); }
        set { this.GetType().GetProperty(propertyName).SetValue(this, value, null); }
    }
}