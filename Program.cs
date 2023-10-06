using System.Text.Json;

static void TestJSON(){
   var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    string text = File.ReadAllText("config.json");
    var config = JsonSerializer.Deserialize<Config>(text, options); 

    Console.WriteLine($"MimeMappings: {config.MimeTypes[".html"]}");
    Console.WriteLine($"IndexFiles: {config.IndexFiles[0]}");

}
static void TestJSON2() {
    var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    string text = File.ReadAllText(@"json/books.json");
    var books = JsonSerializer.Deserialize<List<Book>>(text, options);

    Book book = books[4];
    Console.WriteLine($"title: {book.Title}");
    Console.WriteLine($"authors: {book.Authors[0]}");
}

static void TestServer() {
    SimpleHTTPServer server = new SimpleHTTPServer("files", 8080);
    string helpMessage = @"You can try the following commands:
    stop - stop the server
    help - view this message
    number of requests - return number of requests
    path - return number of requests for each URL
";

    Console.WriteLine($"Server started! {helpMessage}");
    while (true)
    {
        // read line from console
        //Console.WriteLine("> ");
        String command = Console.ReadLine();
        if (command.Equals("stop"))
        {
            server.Stop();
            break;
        }
        else if( command.Equals("help"))
        {
           Console.WriteLine(helpMessage); 
           // String command = Console.ReadLine();
        }
        else if(command.Equals("number of requests")){
            Console.WriteLine(server.NumRequests);
        }
        else if(command.Equals("path")){
            foreach(var path in server.Pathreqs)
            {
                Console.WriteLine($"{path.Key}: {path.Value}");
            }
        }
        else
        {
            Console.WriteLine($"Unknown Message : {command}"); 
        }
    }
}

TestJSON();
//TestServer();
