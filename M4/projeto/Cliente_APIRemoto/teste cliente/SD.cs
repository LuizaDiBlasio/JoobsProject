namespace teste_cliente
{
    public static class SD
    {
        public enum ApiType
        {
            GET,
            POST,
            PUT,
            DELETE
        }
        public static string SessionToken = "JWTToken";
        public const string Role_Admin = "Admin";
        public const string Role_SysAdmin = "SysAdmin";
        public const string Role_Empresa = "Empresa";
        public const string Role_Candidato = "Candidato";

    }
}