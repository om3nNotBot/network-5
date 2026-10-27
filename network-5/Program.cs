using System;
using System.IO;
using System.Net;

#pragma warning disable SYSLIB0014 

class Program
{
    static string server = "ftp://172.20.208.126/";
    static string user = "test";
    static string pass = "1234";

    static void Main()
    {
        while (true)
        {
            Console.WriteLine("1 - List   (faylların siyahısı)");
            Console.WriteLine("2 - Get    (fayl endir / download)");
            Console.WriteLine("3 - Put    (fayl yüklə / upload)");
            Console.WriteLine("4 - Delete (fayl və ya qovluq sil)");
            Console.WriteLine("5 - Mkdir  (qovluq yarat)");
            Console.WriteLine("0 - Çıxış");
            Console.Write("Seçim: ");
            string secim = Console.ReadLine() ?? "";

            try
            {
                if (secim == "1") List();
                else if (secim == "2") Get();
                else if (secim == "3") Put();
                else if (secim == "4") Delete();
                else if (secim == "5") Mkdir();
                else if (secim == "0") break;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Xəta: " + ex.Message);
            }
        }
    }

    // Hər əməliyyat üçün sorğu yaradan köməkçi metod
    static FtpWebRequest Sorgu(string yol, string metod)
    {
        var req = (FtpWebRequest)WebRequest.Create(server + yol);
        req.Method = metod;
        req.Credentials = new NetworkCredential(user, pass);
        req.UseBinary = true;
        req.UsePassive = true;
        return req;
    }

    // 1. LIST - serverdəki faylları göstər
    static void List()
    {
        Console.Write("Qovluq (boş = əsas qovluq): ");
        string qovluq = (Console.ReadLine() ?? "").Trim();
        if (qovluq != "" && !qovluq.EndsWith("/")) qovluq += "/";

        var req = Sorgu(qovluq, WebRequestMethods.Ftp.ListDirectory);
        using var res = (FtpWebResponse)req.GetResponse();
        using var reader = new StreamReader(res.GetResponseStream());
        Console.WriteLine("--- Fayllar ---");
        Console.WriteLine(reader.ReadToEnd());
    }

    // 2. GET - serverdən kompüterə fayl endir
    static void Get()
    {
        Console.Write("Endiriləcək faylın adı: ");
        string ad = (Console.ReadLine() ?? "").Trim();

        var req = Sorgu(ad, WebRequestMethods.Ftp.DownloadFile);
        using var res = (FtpWebResponse)req.GetResponse();
        using var stream = res.GetResponseStream();
        using var file = File.Create(Path.GetFileName(ad));
        stream.CopyTo(file);
        Console.WriteLine("Endirildi: " + Path.GetFullPath(Path.GetFileName(ad)));
    }

    // 3. PUT - kompüterdən serverə fayl göndər (STOR)
    static void Put()
    {
        Console.Write("Yüklənəcək faylın yolu: ");
        string yol = (Console.ReadLine() ?? "").Trim().Trim('"');

        if (!File.Exists(yol))
        {
            Console.WriteLine("Fayl tapılmadı: " + yol);
            return;
        }

        Console.Write("Serverdə ad/yol (boş = " + Path.GetFileName(yol) + "): ");
        string uzaq = (Console.ReadLine() ?? "").Trim();
        if (uzaq == "") uzaq = Path.GetFileName(yol);

        var req = Sorgu(uzaq, WebRequestMethods.Ftp.UploadFile);
        using (var fayl = File.OpenRead(yol))
        using (var stream = req.GetRequestStream())
            fayl.CopyTo(stream);

        using var res = (FtpWebResponse)req.GetResponse();
        Console.WriteLine("Yükləndi (PUT): " + uzaq + " - " + res.StatusDescription);
    }

    // 4. DELETE - serverdən fayl və ya qovluq sil
    static void Delete()
    {
        Console.Write("Silinəcək faylın/qovluğun adı: ");
        string ad = (Console.ReadLine() ?? "").Trim();
        if (ad == "") return;

        Console.Write("Növ: f - fayl, d - qovluq: ");
        bool qovluq = (Console.ReadLine() ?? "").Trim().ToLower() == "d";

        Console.Write($"'{ad}' silinsin? (b/x): ");
        if ((Console.ReadLine() ?? "").Trim().ToLower() != "b")
        {
            Console.WriteLine("Ləğv edildi.");
            return;
        }

        var req = Sorgu(ad, qovluq ? WebRequestMethods.Ftp.RemoveDirectory : WebRequestMethods.Ftp.DeleteFile);
        using var res = (FtpWebResponse)req.GetResponse();
        Console.WriteLine("Silindi (DELETE): " + res.StatusDescription);
    }

    // 5. MKDIR - serverdə qovluq yarat (Put üçün faydalıdır)
    static void Mkdir()
    {
        Console.Write("Yaradılacaq qovluğun adı: ");
        string ad = (Console.ReadLine() ?? "").Trim();
        if (ad == "") return;

        var req = Sorgu(ad, WebRequestMethods.Ftp.MakeDirectory);
        using var res = (FtpWebResponse)req.GetResponse();
        Console.WriteLine("Yaradıldı: " + res.StatusDescription);
    }
}