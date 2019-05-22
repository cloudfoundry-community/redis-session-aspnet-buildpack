using System;
using System.Web.UI;

namespace sample_test_session_section
{
    public partial class _Default : Page
    {
        public _Default()
        {
            Session["testkey"] = Session["testkey"] ?? $"Some random GUID({new Guid().ToString()})";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Console.WriteLine($"Session['testkey'] = {Session["testkey"]}");
            System.Diagnostics.Debug.WriteLine($"Session['testkey'] = {Session["testkey"]}");
            Label1.Text = $"Session['testkey'] = {Session["testkey"]}";
        }
    }
}