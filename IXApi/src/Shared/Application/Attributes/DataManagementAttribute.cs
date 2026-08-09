using System;

namespace IAX.IXApi.Shared.Application.Attributes
{
    /// <summary>
    /// Attribute used to mark entities as available for generic Data Management (Import/Export).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DataManagementAttribute : Attribute
    {
        public string? DisplayName { get; }

        public DataManagementAttribute(string? displayName = null)
        {
            DisplayName = displayName;
        }
    }
}
