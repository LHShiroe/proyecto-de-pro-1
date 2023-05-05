using System.Text;
using System.Linq;
namespace MoogleEngine;


public static class Moogle
{

    public static BaseItem DB = CargarDatos();
    public static BaseItem Inicializar() { return DB; }
    public static List<string> Remove = new List<string>();
    public static List<string> Alll = new List<string>();
    public static List<string> Importante = new List<string>();
    public static int cont = 1;
    public static int[,] Optimo = new int[2, 3];
    public static bool Zero(SearchItem isZero)
    {
        return isZero.Score == 0;
    }
    public static SearchResult Query(string query)
    {
        // Modifique este método para responder a la búsqueda

        string busqueda = Operator(query);
        string[] CadenadeBusqueda = Normalizar(busqueda).Split();
        StringBuilder Sugerencia = new StringBuilder();
        for (var i = 0; i < CadenadeBusqueda.Length; i++)
        {
            string MenVE = EditDicc(CadenadeBusqueda[i]);
            Sugerencia.Append(MenVE);
            Sugerencia.Append(" ");
        }
        Sugerencia.Remove(Sugerencia.Length - 1, 1);

        //Inicia la bsqueda
        var ArrayBusqueda = Sugerencia.ToString().Split();
        Console.WriteLine(ArrayBusqueda[0]);
        List<SearchItem> items = new List<SearchItem>();
        for (var i = 0; i < DB.Archivos.Length; i++)
        {
            string Snippet;
            double scr = 0;
            foreach (var item in ArrayBusqueda)
            {
                scr = scr + DB.TfIdf[DB.Dicc[item], i];
            }
            if(scr < 0){continue;}
            if (File.ReadAllText(DB.Archivos[i]).ToString().Length < 1000) { Snippet = File.ReadAllText(DB.Archivos[i]); }
            else { Snippet = (File.ReadAllText(DB.Archivos[i]).Substring(0, 1000)).ToString(); };
            items.Add(new SearchItem(Path.GetFileNameWithoutExtension(DB.Archivos[i]), Snippet, scr));
            
        }

        var OrdenItems = items.OrderByDescending(x => x.Score).ToList();
        OrdenItems.RemoveAll(Zero);



        return new SearchResult(OrdenItems.ToArray(), Sugerencia.ToString());
    }

    static BaseItem CargarDatos()
    {
        Console.WriteLine("doing");
        var star = DateTime.Now.Ticks;
        //Guardar el nombre de las carpetas y sus archivos. CAMBIAR= \=/ Y @
        string[] Archivos = Directory.GetFiles(@"..\Content", "*", SearchOption.AllDirectories);
        string[][] PalabrasporArchivos = new string[Archivos.Length][];

        for (var i = 0; i < Archivos.Length; i++)
        {
            PalabrasporArchivos[i] = Normalizar(File.ReadAllText(Archivos[i])).Split();
        }
        List<string> Palabra = new List<string>();
        foreach (var item in PalabrasporArchivos)
        {
            Palabra.AddRange(item);
        }
        List<string> Palabras = new List<string>();
        Palabras = Palabra.Distinct().ToList();
        Dictionary<string, int> DiccionarioPalabras = new Dictionary<string, int>();
        for (var i = 0; i < Palabras.Count; i++)
        {
            DiccionarioPalabras.Add(Palabras[i], i);
        }

        int[,] RepetecionPorArchivo = new int[Archivos.Length, Palabras.Count];
        for (var i = 0; i < Archivos.Length; i++)
        {
            for (var j = 0; j < PalabrasporArchivos[i].Length; j++)
            {
                RepetecionPorArchivo[i, DiccionarioPalabras[PalabrasporArchivos[i][j]]]++;
            }
        }
        int[] Archivosporpalabra = new int[Palabras.Count];
        for (int r = 0; r < Palabras.Count; r++)
        {
            for (int s = 0; s < Archivos.Length; s++)
            {
                if (RepetecionPorArchivo[s, r] != 0) Archivosporpalabra[r]++;
            }
        }
        double[,] TfIdf = new double[Palabras.Count, Archivos.Length];
        for (var o = 0; o < Palabras.Count; o++)
        {
            for (var u = 0; u < Archivos.Length; u++)
            {
                TfIdf[o, u] = (Convert.ToDouble(RepetecionPorArchivo[u, o]) / Convert.ToDouble(PalabrasporArchivos[u].Length)) * (Convert.ToDouble(Archivos.Length) / Convert.ToDouble(Archivosporpalabra[o]));
            }
        }

        Console.WriteLine("Listo");
        var end = DateTime.Now.Ticks;
        var sec = ((end - star) / 10000) / 1000;
        Console.WriteLine(sec);

        return new BaseItem(Palabras, Archivos, TfIdf, DiccionarioPalabras, PalabrasporArchivos);
    }
    static string Operator(string a)
    {
        int cont = 1;
        string[] pal = a.Split();
        for (var i = 0; i < pal.Length; i++)
        {
            char[] b = pal[i].ToCharArray();
            if (b[0] == '!')
            {
                int re = DB.Dicc[Normalizar(pal[i].Substring(1))];
                for (var e = 0; e < DB.Archivos.Length; e++)
                {
                    DB.TfIdf[re, e] = -1000;
                }
                pal[i] = DB.Palabras[re];
            }
            else if (b[0] == '^')
            {
                int all = DB.Dicc[Normalizar(pal[i].Substring(1))];
                for (int o = 0; o < DB.Archivos.Length; o++)
                {
                    bool exist = (DB.PalabrasporArchivos[o].Contains(DB.Palabras[all]));
                    if (exist == false)
                    {
                        DB.TfIdf[all, o] = -1000;
                    }
                }
                pal[i] = DB.Palabras[all];
            }
            else if (b[0] == '*')
            {
                for (var j = 0; j < b.Length; j++)
                {
                    if (b[0] == '*')
                    {
                        cont++;
                    }
                    else { break; }
                }
                int important = DB.Dicc[Normalizar(pal[i].Substring(cont))];
                cont = (10 * cont);

                for (int n = 0; n < DB.Archivos.Length; n++)
                {
                    DB.TfIdf[important, n] = +cont;
                }
                pal[i] = DB.Palabras[important];
            }
            else if (b[0] == '~')
            {
                int pri = DB.Dicc[Normalizar(pal[i - 1])];
                int sec = DB.Dicc[Normalizar(Operator(pal[i + 1]))];
               
                for (var x = 0; x < DB.Archivos.Length; x++)
                {
                    if(DB.PalabrasporArchivos[x].Contains(DB.Palabras[pri]) == DB.PalabrasporArchivos[x].Contains(DB.Palabras[sec]));
                    {
                        List<string> xan =DB.PalabrasporArchivos[x].ToList();
                        int ta = xan.Count;
                        List<int> a1=new List<int>();
                        List<int> b1=new List<int>();
                        for (var el = 0; el < ta; el++)
                        {
                            if(DB.PalabrasporArchivos[x][el] == DB.Palabras[pri])
                            {
                                a1.Add(el);
                            }
                            else if(DB.PalabrasporArchivos[x][el] == DB.Palabras[sec])
                            {
                                b1.Add(el);
                            } 
                        }
                        List<int> d = new List<int>();
                        foreach (var ita in a1)
                        {
                            foreach (var itb in b1)
                            {
                                if (ita > itb)
                                {
                                    d.Add(ita - itb);
                                }
                                else
                                {
                                    d.Add(ita - itb);
                                }

                            }
                        }
                        double e = d.Min();
                        
                        DB.TfIdf[pri, x] =DB.TfIdf[pri, x] * Math.Log(ta/e);
                    }
                }
                

            }
        }
        return pal.ToString();
    }
    static string Normalizar(string a)
    {
        //poner los sting en minuscula, sin tilde o caracter extraño.
        StringBuilder b = new StringBuilder();
        foreach (var item in a.ToLower())
        {
            if ("abcdefghijklmnopqrstuvwxyzñ ".Contains(item))
            {
                b.Append(item);
            }
            else if ("äáâãà".Contains(item))
            {
                b.Append('a');
            }

            else if ("ëéêè".Contains(item))
            {
                b.Append('e');
            }

            else if ("ïíìî".Contains(item))
            {
                b.Append('i');
            }
            else if ("öóòôõ".Contains(item))
            {
                b.Append('o');
            }
            else if ("üúùû".Contains(item))
            {
                b.Append('u');
            }
            else if ("`@#$%&()-_=+|]}[{';:/><.,¿°?".Contains(item))
            {
                b.Append(' ');
            }

        }
        return b.ToString();
    }
    static string EditDicc(string edit)
    {
        string menVE = "";
        int Edit = int.MaxValue;
        for (var i = 0; i < DB.Palabras.Count; i++)
        {
            int actualEdit = EditDist(edit, DB.Palabras[i]);
            if (actualEdit < Edit)
            {
                menVE = DB.Palabras[i];
                Edit = actualEdit;
            }
        }
        return menVE;
    }
    static int EditDist(string a, string b)
    {
        char[] ho = a.ToCharArray();
        char[] ve = b.ToCharArray();
        int[,] mat = new int[ho.Length + 1, ve.Length + 1];
        mat[0, 0] = 0;
        for (int i = 0; i < ho.Length + 1; i++)
        {
            mat[i, 0] = i;
        }
        for (int t = 0; t < ve.Length + 1; t++)
        {
            mat[0, t] = t;
        }
        for (int h = 1; h < ho.Length + 1; h++)
        {
            for (int v = 1; v < ve.Length + 1; v++)
            {
                if (ve[v - 1] == ho[h - 1]) mat[h, v] = mat[h - 1, v - 1];
                else
                {
                    mat[h, v] = ((Math.Min(mat[h, v - 1], Math.Min(mat[h - 1, v], mat[h - 1, v - 1]))) + 1);
                }
            }
        }
        return mat[ho.Length, ve.Length];
    }
}