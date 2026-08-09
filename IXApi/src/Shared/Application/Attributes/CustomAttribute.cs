namespace IAX.IXApi.Shared.Application.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class CustomAttribute : Attribute
    {
        public string CustomAtt { get; }
        public CustomAttribute(string _CustomAtt)
        {
            CustomAtt = _CustomAtt;
        }

       
    }
}
