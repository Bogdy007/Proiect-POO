// ============================================================
//  Evadare din Castelul Bran — proiect POO (echipă de 4)
//  AUTOR: Persoana 3 — Aplicația Editor (structura principală) — punct de intrare
// ============================================================
namespace EvadareBranEditor
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}