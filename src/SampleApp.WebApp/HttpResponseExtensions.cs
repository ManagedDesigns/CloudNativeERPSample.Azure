using Microsoft.AspNetCore.Http;

namespace SampleApp.WebApp
{
    public static class HttpResponseExtensions
    {
        public static void Write(this HttpResponse response, string text)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(text);
            response.Body.Write(bytes);
        }
        public static async void WriteAsync(this HttpResponse response, string text)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(text);
            await response.Body.WriteAsync(bytes);
        }
    }
}
