namespace IAX.IXApi.Shared.Application.Contracts
{
    public class JwtSettings
    {
        public required string Secret { set; get; }

        public required string Issuer { get; set; }

        public required string Audience { get; set; }

        public int Expires { get; set; }

    }
}
