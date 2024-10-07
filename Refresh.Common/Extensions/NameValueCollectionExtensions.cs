using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace Refresh.Common.Extensions;

public static class NameValueCollectionExtensions
{
    public static string ToQueryString(this NameValueCollection queryParams)
    {
        StringBuilder builder = new();

        if (queryParams.Count == 0)
            return string.Empty;
        
        builder.Append('?');
        for (int i = 0; i < queryParams.Count; i++)
        {
            string? key = queryParams.GetKey(i);
            string? val = queryParams.Get(i);

            if (key == null)
                continue;

            builder.Append(HttpUtility.UrlEncode(key));
            builder.Append('=');
            if(val != null)
                builder.Append(HttpUtility.UrlEncode(val));
            
            if(i < queryParams.Count - 1)
                builder.Append('&');
        }
        
        return builder.ToString();
    }
}
