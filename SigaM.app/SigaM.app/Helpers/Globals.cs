namespace SigaM.app
{
    public static class Globals
    {
        public static string UsuarioLogado { get; set; } = string.Empty;

        // Regra: Só é admin se o prontuário for 'admin'
        public static bool IsAdmin => UsuarioLogado.ToLower() == "admin";
    }
}
