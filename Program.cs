using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using System.Xml.XPath;
namespace GeneracionPaginas
{
    // entrada lista de ficheros
    class EntradaFichero : IComparable
    {
        public string nombre;
        public string nombrecorto;
        public DateTime fecha;
        public int CompareTo(object other)
        {
            EntradaFichero tmp = (EntradaFichero)other;
            if (fecha == tmp.fecha)
                return 0;
            else if (fecha < tmp.fecha)
                return 1;
            else
                return -1;
        }


    }
    class Program
    {
        string DirectorioBase;
        XElement GetApplet(DirectoryInfo dir)
        {
            FileInfo v1 = dir.GetFiles().Where(i1 => i1.Name == "applets.xml").FirstOrDefault();
            if (v1 == null)
                return null;

            XElement e1 = XElement.Load(v1.FullName);

            var nodos = e1.XPathSelectElements("//fichero");

            XElement salida = new XElement("ul",
                     new XElement("li", new XAttribute("class", "applet"),
                         new XElement("a",
                                       new XAttribute("href", "#"),
                                       new XAttribute("onclick", "$(this).parent().children('ul').toggle(); return false;"),
                                        "Applets"),
                                        new XElement("ul",
                                         from i in nodos
                                         select
                                             new XElement("li", new XElement("a",
                                                 new XAttribute("target", "_blank"),
                                                 new XAttribute("class", "file"),
                                                 new XAttribute("href", i.Attribute("nombrereal").Value),
                                   i.Attribute("nombre").Value)))));
            return salida;
        }
        XElement GetDirectorio(DirectoryInfo dir)
        {

            try
            {

                return
                    new XElement("li", new XAttribute("class", "folder"),
                            new XElement("a",
                                          new XAttribute("href", "#"),
                                          new XAttribute("onclick", "$(this).parent().children('ul').toggle(); return false;"),
                                           dir.Name),
                         GetApplet(dir),
                         dir.GetFiles().Where(i => i.Name != "Thumbs.db" && i.Name != ".DS_Store" && i.Name != "applets.xml").Count() > 0 || dir.GetDirectories().Count() > 0 ?
                             new XElement("ul", new XAttribute("class", "hijos"),
                                 from i in dir.GetFiles().OrderBy(j => j.Name).Where(i => i.Name != ".DS_Store" && i.Name != "Thumbs.db" && i.Name != "applets.xml")
                                 select new XElement("li", new XAttribute("class", "file"),
                                      new XElement("a", new XAttribute("target", "_blank"),
                                          new XAttribute("href", EncodeUrl(i.FullName.Substring(i.FullName.IndexOf("datos")).Replace("\\", "/"))), i.Name)),
                                             from j in dir.GetDirectories().OrderBy(i => i.Name)

                                             select GetDirectorio(j)) : null
                         );
            }
            catch (Exception e)
            {
                return null;
            }

        }
        void GetDirectorio1(DirectoryInfo dir, List<EntradaFichero> ListaFicheros)
        {

            foreach (var fichero in dir.GetFiles())
            {
                Console.WriteLine(fichero.Name);
                ListaFicheros.Add(
                    new EntradaFichero
                    {
                        nombre = EncodeUrl(fichero.FullName.Substring(fichero.FullName.IndexOf("datos")).Replace("\\", "/")),
                        nombrecorto = fichero.Name,
                        fecha = fichero.LastWriteTime
                    }
                  );
            }
            foreach (var dir1 in dir.GetDirectories())
            {
                Console.WriteLine(dir1.Name);
                GetDirectorio1(dir1, ListaFicheros);
            }
            Console.WriteLine(ListaFicheros.Count());

        }

        protected void Execute()
        {
            DirectoryInfo dir = new DirectoryInfo(DirectorioBase);
            // generar lista con todos los ficheros ordenados por fecha
            XElement estadistica = new XElement("ul");
            List<EntradaFichero> ListaFicheros = new List<EntradaFichero>();
            foreach (DirectoryInfo d1 in dir.GetDirectories())
            {
                GetDirectorio1(d1, ListaFicheros);
            }

            Console.WriteLine(ListaFicheros.Count());
            foreach (var i in ListaFicheros.OrderByDescending(i => i.fecha).Where(i => i.nombrecorto != ".DS_Store"))
            {
                estadistica.Add(new XElement("li",
                    new XElement("a",
                    new XAttribute("href", i.nombre), "(" + i.fecha + ") " + i.nombrecorto)));
            }

            XElement pagina1 = XElement.Load("plantillas/plantillaestadistica.htm");
            XElement lista1 = pagina1.Element("body").Element("div");
            lista1.Add(estadistica);
            FileStream s1 = File.Create("generado/Estadistica.html");

            pagina1.Save(s1);
            s1.Close();


            XElement menu1 = XElement.Load("plantillas/plantillaindex.htm");
            XElement menu = menu1.XPathSelectElement("//*[@id = 'menu']").Element("ul");

            foreach (DirectoryInfo d1 in dir.GetDirectories().OrderBy(i => i.Name))
            {
                if (d1.FullName.IndexOf("\\Logos") >= 0)
                    continue;

                menu.Add(new XElement("li",
                      new XElement("a", new XAttribute("href", EncodeUrl(d1.Name + ".html")), d1.Name)));
                XElement pagina = XElement.Load("plantillas/pagina.htm");
                XElement lista = pagina.Element("body").Element("div").Element("ul");
                lista.Add(GetDirectorio(d1));
                FileStream s = File.Create("generado/" + d1.Name + ".html");
                pagina.Save(s);
                s.Close();
            }

            XElement estadisticas = menu1.XPathSelectElement("//*[@id = 'estadisticas']").Element("ul");
            XElement fecha = menu1.XPathSelectElement("//*[@id = 'fecha']");
            fecha.Value = DateTime.Now.ToString("dd/MM/yyyy");
            foreach (var i in ListaFicheros.OrderByDescending(i => i.fecha).Where(i => i.nombrecorto != ".DS_Store").Take(120))
            {
                estadisticas.Add(new XElement("li",
                    new XElement("a",
                    new XAttribute("href", i.nombre), i.nombrecorto)));
            }

            FileStream f = new FileStream("generado/index.html", FileMode.Create);
            XmlTextWriter t = new XmlTextWriter(f, Encoding.UTF8);
            t.Formatting = Formatting.Indented;
            menu1.Save(t);
            t.Close();

        }
        string EncodeUrl(string entrada)
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            byte[] b = enc.GetBytes(entrada);

            string cadena = "";
            for (int i = 0; i < b.Length; i++)
            {
                if (b[i] == 32)
                {
                    cadena += "%20";
                }
                else
                    if (b[i] > 127)
                {
                    cadena += "%" + b[i].ToString("X");
                }
                else
                    cadena += Convert.ToChar(b[i]);
            }
            return cadena;
        }
        static void Main(string[] args)
        {

            Program p1 = new Program();
            foreach (string s1 in Directory.EnumerateFiles(".", "*.html"))
            {
                File.Delete(s1);
            }
            foreach (string s1 in Directory.EnumerateFiles("plantillas/fijos", "*.*"))
            {
                FileInfo f1 = new FileInfo(s1);
                File.Copy(s1, "generado/" + f1.Name, true);
            }
            p1.DirectorioBase = System.Configuration.ConfigurationManager.AppSettings["dir"];
            p1.DirectorioBase = @"./generado/datos";
            p1.Execute();
        }
    }
}