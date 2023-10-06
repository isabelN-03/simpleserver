// SimpleServer based on code by Can Güney Aksakalli
// MIT License - Copyright (c) 2016 Can Güney Aksakalli
// https://aksakalli.github.io/2014/02/24/simple-http-server-with-csparp.html
// modifications by Jaime Spacco

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Web;
using System.Text.Json;


/// <summary>
/// Interface for simple servlets.
/// </summary>
interface IServlet {
    void ProcessRequest(HttpListenerContext context);
}
/// <summary>
/// BookHandler: Servlet that reads a JSON file and returns a random book
/// as an HTML table with one row.
/// TODO: search for specific books by author or title or whatever
/// </summary>
class BookHandler : IServlet {

    private List<Book> books;

    public BookHandler(){
         // we want to use case-insensitive matching for the JSON properties
        // the json files use lowercae letters, but we want to use uppercase in our C# code

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        string text = File.ReadAllText(@"json/books.json");
        books = JsonSerializer.Deserialize<List<Book>>(text, options);
    }
    public void ProcessRequest(HttpListenerContext context) {
        if(!context.Request.QueryString.AllKeys.Contains("cmd")){
            // if the client didn't specify a command, return a 400 Bad Request
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }
        string cmd = context.Request.QueryString["cmd"];
        if(cmd.Equals("list"))
        {
            //list books from start to end from JSON file
            int start =Int32.Parse(context.Request.QueryString["s"]); 
            int end = Int32.Parse(context.Request.QueryString["e"]);
            List<Book> booksList = books.GetRange(start,end - start +1);

            // build the HTML response
            // @ means a multiline string (Java doesn't have this)
            // $ means string interpolation (Java doesn't have this either)
            string response = $@"
                <table border=1>
                <tr>
                    <th>Title</th>
                    <th>Author</th>
                    <th>Short Description</th>
                    <th>Thumbnail</th>
                </tr>";
                foreach(Book book in booksList)
                {
                    string authors = String.Join(",<br>", book.Authors);
                    response += $@"
                    <tr>
                        <td>{book.Title}</td>
                        <td>{authors}</td>
                        <td>{book.ShortDescription}</td>
                        <td><img src ='{book.ThumbnailUrl}'</></td>
                    </tr>
                    ";
                }
                response += "</table>";
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(response);
                context.Response.ContentType = "text/html";
                context.Response.ContentLength64 = bytes.Length;
                context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                context.Response.OutputStream.Flush();
                context.Response.OutputStream.Close();

        }else if(cmd.Equals("random"))
        {
            // return a random book
            Random rand = new Random();
            int index = rand.Next(books.Count);
            Book book = books[index];
            string authors = String.Join(",<br>", book.Authors);
            string response = $@"
                <table border=1>
                <tr>
                    <th>Title</th>
                    <th>Author</th>
                    <th>Short Description</th>
                    <th>Thumbnail</th>
                </tr>
                <tr>
                    <td>{book.Title}</td>
                    <td>{authors}</td>
                    <td>{book.ShortDescription}</td>
                    <td><img src ='{book.ThumbnailUrl}'</></td>
                </tr>
                </table>";

                // write HTTP response to the output stream
                // all of the context.response stuff is setting the headers for the HTTP response
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(response);
                context.Response.ContentType = "text/html";
                context.Response.ContentLength64 = bytes.Length;
                context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                context.Response.OutputStream.Flush();
                context.Response.OutputStream.Close();

        }else 
        {
            // if the client specified an unknown command, return a 400 Bad Request
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }
        
    }
}
/// <summary>
/// FooHandler: Servlet that returns a simple HTML page.
/// </summary>
class FooHandler : IServlet {

    public void ProcessRequest(HttpListenerContext context) {
        string response = $@"
            <H1>This is a Servlet Test.</H1>
            <h2>Servlets are a Java thing; there is probably a .NET equivlanet but I don't know it</h2>
            <h3>I am but a humble Java programmer who wrote some Servlets in the 2000s</h3>
            <p>Request path: {context.Request.Url.AbsolutePath}</p>
";
        foreach ( String s in context.Request.QueryString.AllKeys )
            response += $"<p>{s} -> {context.Request.QueryString[s]}</p>\n";

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(response);

        context.Response.ContentType = "text/html";
        context.Response.ContentLength64 = bytes.Length;
        context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
        context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));
        context.Response.StatusCode = (int)HttpStatusCode.OK;

        context.Response.OutputStream.Write(bytes, 0, bytes.Length);

        context.Response.OutputStream.Flush();
        context.Response.OutputStream.Close();
    }
}

///<summary>
///BookFilter: Servlet that filters and retruns books by specific author
///</summary>

class BookFilter: IServlet{
     List<Book> books;
    public BookFilter(){
         // we want to use case-insensitive matching for the JSON properties
        // the json files use lowercae letters, but we want to use uppercase in our C# code

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        string text = File.ReadAllText(@"json/books.json");
        books = JsonSerializer.Deserialize<List<Book>>(text, options);
    }

    public void ProcessRequest(HttpListenerContext context) {
        if(!context.Request.QueryString.AllKeys.Contains("cmd")){
            // if the client didn't specify a command, return a 400 Bad Request
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }
        string cmd = context.Request.QueryString["cmd"];
        if(cmd.Equals("author")) //if command is author
        {
            //list books with author name from JSON file
            string authorN = context.Request.QueryString["name"]; 
            List<Book> booksList = new List<Book>();
            foreach(Book book in books){
                foreach(string author in book.Authors){
                    if(author.Contains(authorN.Substring(0,1).ToUpper() + authorN.Substring(1).ToLower())){
                        booksList.Add(book);
                    }
                }
            }

            // build the HTML response
            // @ means a multiline string (Java doesn't have this)
            // $ means string interpolation (Java doesn't have this either)
            string response = $@"
                <table border=1>
                <tr>
                    <th>Title</th>
                    <th>Author</th>
                    <th>Short Description</th>
                    <th>Thumbnail</th>
                </tr>";
                foreach(Book book in booksList)
                {
                    string authors = String.Join(",<br>", book.Authors);
                    response += $@"
                    <tr>
                        <td>{book.Title}</td>
                        <td>{authors}</td>
                        <td>{book.ShortDescription}</td>
                        <td><img src ='{book.ThumbnailUrl}'</></td>
                    </tr>
                    ";
                }
                response += "</table>";
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(response);
                context.Response.ContentType = "text/html";
                context.Response.ContentLength64 = bytes.Length;
                context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                context.Response.OutputStream.Flush();
                context.Response.OutputStream.Close();

        }else if(cmd.Equals("title")) //if command is title
        {
            //list books with title from JSON file
            string titleN = context.Request.QueryString["name"]; 
            List<Book> booksList = new List<Book>();
            foreach(Book book in books){
                if(book.Title.Contains(titleN)){
                    booksList.Add(book);
                }
            }

            // build the HTML response
            // @ means a multiline string (Java doesn't have this)
            // $ means string interpolation (Java doesn't have this either)
            string response = $@"
                <table border=1>
                <tr>
                    <th>Title</th>
                    <th>Author</th>
                    <th>Short Description</th>
                    <th>Thumbnail</th>
                </tr>";
                foreach(Book book in booksList)
                {
                    string authors = String.Join(",<br>", book.Authors);
                    response += $@"
                    <tr>
                        <td>{book.Title}</td>
                        <td>{authors}</td>
                        <td>{book.ShortDescription}</td>
                        <td><img src ='{book.ThumbnailUrl}'</></td>
                    </tr>
                    ";
                }
                response += "</table>";
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(response);
                context.Response.ContentType = "text/html";
                context.Response.ContentLength64 = bytes.Length;
                context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                context.Response.OutputStream.Flush();
                context.Response.OutputStream.Close();

        }else 
        {
            // if the client specified an unknown command, return a 400 Bad Request
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }
        
    }
}

/// <summary>
/// Errorpage: Servlet that returns custom 404 error page
/// </summary>

class ErrorPage : IServlet{

    public void ProcessRequest(HttpListenerContext context){
        string response = $@"
        <H1> Error: 404</H1>
        <h2>This page cannot be found</h2> 
        ";

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(response);

        context.Response.ContentType = "text/html";
        context.Response.ContentLength64 = bytes.Length;
        context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
        context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.OutputStream.Write(bytes, 0, bytes.Length);
        context.Response.OutputStream.Flush();
        context.Response.OutputStream.Close();
    }
}

/// <summary>
/// BuffyGenerator: Servlet that returns a random episode, selected season or selected episode of Buffy the Vampire Slayer
/// </summary>
class BuffyGenerator: IServlet{
     List<Show> episodes;
    public BuffyGenerator(){
         // we want to use case-insensitive matching for the JSON properties
        // the json files use lowercae letters, but we want to use uppercase in our C# code

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        string text = File.ReadAllText(@"json/buffy.json");
        episodes = JsonSerializer.Deserialize<List<Show>>(text, options);
    }

    public void ProcessRequest(HttpListenerContext context) {
        if(!context.Request.QueryString.AllKeys.Contains("cmd")){
            // if the client didn't specify a command, return a 400 Bad Request
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }
        string cmd = context.Request.QueryString["cmd"];
        if(cmd.Equals("random")) //if command is random
        {
            // return a random episode
            Random rand = new Random();
            int index = rand.Next(episodes.Count);
            Show random = episodes[index];
            
            //string rating = random.Rating;
            string response = $@"
                <table border=1>
                <tr>
                    <th>Season</th>
                    <th>Episode </th>
                    <th>Name</th>
                    <th>Summary</th>
                    <th>Rating</th>
                    <th>URL</th>
                    
                </tr>
                <tr>
                    <td>{random.Season}</td>
                    <td>{random.Number}</td>
                    <td>
                    {random.Name}
                    <img src='{random.Image.Medium}'/>
                    </td>
                    <td>{random.Summary}</td>
                    <td>{random.Rating.Average}</td>
                    <td>{random.Url}</td>
                    </tr>

                </table>";

                // write HTTP response to the output stream
                // all of the context.response stuff is setting the headers for the HTTP response
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(response);
                context.Response.ContentType = "text/html";
                context.Response.ContentLength64 = bytes.Length;
                context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                context.Response.OutputStream.Flush();
                context.Response.OutputStream.Close();

        }else if(cmd.Equals("season")) //if command is seasonList
        {
            //list specific episode from JSON file
            int season =Int32.Parse(context.Request.QueryString["s"]); 
            List<Show> seasonList = new List<Show>();
            foreach(Show s in episodes){
                if(s.Season == season){
                    seasonList.Add(s);
                }
            }

            // build the HTML response
            // @ means a multiline string (Java doesn't have this)
            // $ means string interpolation (Java doesn't have this either)
            string response = $@"
                <table border=1>
                <tr>
                    <th>Season</th>
                    <th>Episode </th>
                    <th>Name</th>
                    <th>Summary</th>
                    <th>Rating</th>
                    <th>URL</th>
                </tr>";
                foreach(Show s in seasonList)
                {
                    //string rating = s.Rating;
                    response += $@"
                    <tr>
                        <td>{s.Season}</td>
                        <td>{s.Number}</td>
                        <td>{s.Name}
                        <p></p>
                        <img src='{s.Image.Medium}'/>
                        </td>
                        <td>{s.Summary}</td>
                        <td>{s.Rating.Average}</td>
                        <td>{s.Url}</td>
                    </tr>
                    ";
                }
                response += "</table>";
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(response);
                context.Response.ContentType = "text/html";
                context.Response.ContentLength64 = bytes.Length;
                context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                context.Response.OutputStream.Flush();
                context.Response.OutputStream.Close();

        } else if(cmd == "episode") //if command is episode
        {
            //list specific episode from JSON file
            int season =Int32.Parse(context.Request.QueryString["s"]); 
            int episode = Int32.Parse(context.Request.QueryString["e"]);
            Show episodeN = new Show();
            foreach(Show e in episodes){
                if(e.Season == season && e.Number == episode){
                    episodeN = e;
                }
            }

            // build the HTML response
            // @ means a multiline string (Java doesn't have this)
            // $ means string interpolation (Java doesn't have this either)
            string response = $@"
                <table border=1>
                <tr>
                    <th>Season</th>
                    <th>Episode </th>
                    <th>Name</th>
                    <th>Summary</th>
                    <th>Rating</th>
                    <th>URL</th>
                </tr>
                <tr>
                    <td>{episodeN.Season}</td>
                    <td>{episodeN.Number}</td>
                    <td>{episodeN.Name}
                    <p></p>
                    <img src='{episodeN.Image.Medium}'/>
                    </td>
                    <td>{episodeN.Summary}</td>
                    <td>{episodeN.Rating.Average}</td>
                    <td>{episodeN.Url}</td>
                </tr>
                </table>";
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(response);
                context.Response.ContentType = "text/html";
                context.Response.ContentLength64 = bytes.Length;
                context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                context.Response.OutputStream.Flush();
                context.Response.OutputStream.Close();
        }
        else 
        {
            // if the client specified an unknown command, return a 400 Bad Request
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }
        
    }
}
class SimpleHTTPServer
{
    // bind servlets to a path
    // for example, this means that /foo will be handled by an instance of FooHandler
    // TODO: put these mappings into a configuration file
    private static IDictionary<string, IServlet> _servlets = new Dictionary<string, IServlet>() {
        {"foo", new FooHandler()},
        {"books", new BookHandler()},
        {"filter", new BookFilter()},
        {"buffy", new BuffyGenerator()}
    };

    // list of default index files
    // if the client requests a directory (e.g. http://localhost:8080/), 
    // we will look for one of these files
    private string[] _indexFiles;
    
    // map extensions to MIME types
    // TODO: put this into a configuration file
    private static IDictionary<string, string> _mimeTypeMappings;
    // instance variables
    private Thread _serverThread;
    private string _rootDirectory;
    private HttpListener _listener;
    private int _port;
    private int numreqs;
    private bool _done = false;
    private Dictionary<string, int> paths= new Dictionary<string, int>();
    private Dictionary<string, int> errorpages = new Dictionary<string, int>(); //dict to keep track of error pages returned for each url
    public int Port
    {
        get { return _port; }
        private set { _port = value;}
    }
    public int NumRequests
    {
        get {return numreqs; }
        private set { numreqs = value;}
    }

    public Dictionary<string,int> Pathreqs{
        get{return paths;}
    } 
    public Dictionary <string,int> Errors{ //method to return error dictionary
        get{return errorpages;}
    }
    /// <summary>
    /// Construct server with given port.
    /// </summary>
    /// <param name="path">Directory path to serve.</param>
    /// <param name="port">Port of the server.</param>

    /// <summary>
    /// Construct server with any open port.
    /// </summary>
    /// <param name="path">Directory path to serve.</param>
    /// <param name="configFile">the name of JSON configuration file</param> 

    public SimpleHTTPServer(string path, int port, string configFile)
    {
            this.Initialize(path, port, configFile);
        
    }
    public SimpleHTTPServer(string path, string configFile)
    {
        //get an empty port
        TcpListener l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        int port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        this.Initialize(path, port, configFile);
    }

    /// <summary>
    /// Stop server and dispose all functions.
    /// </summary>
    public void Stop()
    {
        _done = true;
        _listener.Close();
    }

    private void Listen()
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add("http://*:" + _port.ToString() + "/");
        _listener.Start();
        while (!_done)
        {
            Console.WriteLine("Waiting for connection...");
            try
            {
                HttpListenerContext context = _listener.GetContext();
                NumRequests++;
                Process(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        Console.WriteLine("Server stopped!");
    }

    /// <summary>
    /// Process an incoming HTTP request with the given context.
    /// </summary>
    /// <param name="context"></param>
    private void Process(HttpListenerContext context)
    {
        string filename = context.Request.Url.AbsolutePath;
        paths[filename] = paths.GetValueOrDefault(filename, 0) +1;
        filename = filename.Substring(1);
        Console.WriteLine($"{filename} is the path");

        // check if the path is mapped to a servlet
        if (_servlets.ContainsKey(filename))
        {
            _servlets[filename].ProcessRequest(context);
            return;
        }

        // if the path is empty (i.e. http://blah:8080/ which yields hte path /)
        // look for a default index filename
        if (string.IsNullOrEmpty(filename))
        {
            foreach (string indexFile in _indexFiles)
            {
                if (File.Exists(Path.Combine(_rootDirectory, indexFile)))
                {
                    filename = indexFile;
                    break;
                }
            }
        }

        // search for the file in the root directory
        // this means we are serving the file, if we can find it
        filename = Path.Combine(_rootDirectory, filename);

        if (File.Exists(filename))
        {
            try
            {
                Stream input = new FileStream(filename, FileMode.Open);
                
                //Adding permanent http response headers
                string mime;
                context.Response.ContentType = _mimeTypeMappings.TryGetValue(Path.GetExtension(filename), out mime) ? mime : "application/octet-stream";
                context.Response.ContentLength64 = input.Length;
                context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                context.Response.AddHeader("Last-Modified", System.IO.File.GetLastWriteTime(filename).ToString("r"));

                byte[] buffer = new byte[1024 * 16];
                int nbytes;
                while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                    context.Response.OutputStream.Write(buffer, 0, nbytes);
                input.Close();
                
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.OutputStream.Flush();
                context.Response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

        }
        else
        {
            // This sends a 404 if the file doesn't exist or cannot be read
            // TODO: customize the 404 page
            //context.Response.StatusCode = (int)HttpStatusCode.NotFound;
    
            IServlet error = new ErrorPage(); //create servlet for errorpage
            errorpages[filename] = errorpages.GetValueOrDefault(filename, 0) +1;//increment request numbers in dictionary for URL
            error.ProcessRequest(context);
            return;//return custom error page
            

        }
        
        context.Response.OutputStream.Close();
    }

    /// <summary>
    /// Initializes the server by setting up a listener thread on the given port
    /// </summary>
    /// <param name="path">the path of the root directory to serve files</param>
    /// <param name="port">the port to listen for connections</param>
    /// <param name="configFile">the name of JSON configuration file</param> 
    private void Initialize(string path, int port, string configFile)
    {
        this._rootDirectory = path;
        this._port = port;

        //read the configuration file
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        string text = File.ReadAllText("config.json");
        var config = JsonSerializer.Deserialize<Config>(text, options); 

        // assign the configuration values to instance variables
        _mimeTypeMappings = config.MimeTypes;
        _indexFiles = config.IndexFiles.ToArray();

        _serverThread = new Thread(this.Listen);
        _serverThread.Start();
    }


}
