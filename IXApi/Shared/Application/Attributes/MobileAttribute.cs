namespace IAX.IXApi.Shared.Application.Attributes
{
    public class MobileAttribute : ContactNumberAttribute
    {
        protected override int MinLength => 10;

        protected override int MaxLength => 20;

        protected override string TypeName => "Mobile Number";
    }
}
