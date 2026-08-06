namespace IAX.IXApi.Shared.Application.Attributes
{
    public class LandlineAttribute : ContactNumberAttribute
    {
        protected override int MinLength => 6;

        protected override int MaxLength => 20;

        protected override string TypeName => "Landline";
    }
}
