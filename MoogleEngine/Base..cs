namespace MoogleEngine;
public class BaseItem
{
    public BaseItem(List<string> palabras, string[] archivos, double[,] tfidf,Dictionary<string,int> dicc, string[][] palabrasporArchivos  )
    {
        this.Palabras = palabras;
        this.Archivos = archivos;
        this.TfIdf = tfidf;
        this.Dicc = dicc;
        this.PalabrasporArchivos  = palabrasporArchivos;
    }

    public List<string> Palabras { get; private set; }

    public string[] Archivos { get; private set; }

    public double[,] TfIdf { get; private set; }
    public Dictionary<string,int> Dicc { get; private set; }
    public string[][] PalabrasporArchivos  { get; private set; }
}
