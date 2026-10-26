namespace SigaM.app
{
    public static class Globals
    {
        // Propriedades com 'private set' para evitar que as telas alterem os dados por acidente
        public static int UsuarioId { get; private set; } = 0;
        public static string UsuarioLogado { get; private set; } = string.Empty;
        public static string NomeUsuario { get; private set; } = string.Empty;

        // Regra de negócio: Só é admin se o prontuário for 'admin'
        public static bool IsAdmin => UsuarioLogado.ToLower() == "admin";

        // Método que será chamado pelo LoginViewModel logo após o login dar certo
        public static void DefinirUsuario(int id, string prontuario, string nome)
        {
            UsuarioId = id;
            UsuarioLogado = prontuario ?? string.Empty;
            NomeUsuario = nome ?? string.Empty;
        }

        // Método para limpar os dados ao sair (Logout)
        public static void LimparSessao()
        {
            UsuarioId = 0;
            UsuarioLogado = string.Empty;
            NomeUsuario = string.Empty;
        }
    }
}
