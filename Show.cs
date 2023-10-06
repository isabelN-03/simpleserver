class Show
{
    public int Id {get; set;}
    public string Url {get; set;}
    public string Name {get; set;}
    public int Season {get; set;}
    public int Number {get; set;}
    public string Type {get; set;}
    public string Airdate {get; set;}
    public string Airtime {get; set;}
    public int Runtime {get; set;}
    public string Summary {get; set;}
    public Image Image {get; set;}
    public Rating Rating {get; set;}
}
    public class Image//to access the image
    {
        public string Medium {get; set;}
        public string Original {get; set;}
    }
    public class Rating //to access the rating
    {
        public double Average {get; set;}
    }

