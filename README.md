A simple implementation of long pooling for asp.net with an example. LognPoolingLib is actually the library itself, WebApplicationTestLongPooling is an example of usage.



Client Implementation

```C#
var url = "http://localhost:5076/loongPooling";

var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Add("user", "user1");
while (true)
{
    try
    {
        var stream = await httpClient.GetStringAsync(url);

        Console.WriteLine(stream);
    }
    catch (Exception ex) { }
}
```

