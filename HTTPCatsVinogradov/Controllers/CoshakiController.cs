using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace HTTPCatsVinogradov.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CoshakiController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private IMemoryCache _cache;
        //Конструктор
        public CoshakiController(IHttpClientFactory httpClientFactory, IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
        }
        //Получение котов
        [HttpGet(Name = "GetCoshka")]
        public IActionResult Get(string url)
        {
            Stream coshka;
            HttpResponseMessage httpResponseMessage;
            HttpClient httpClient = _httpClientFactory.CreateClient();
            int statusCode;
            try
            {
                //Получение статус-кода страницы по заданной ссылке
                statusCode = (int)httpClient.Send(new HttpRequestMessage(HttpMethod.Get, url)).StatusCode;
                //Получение картинки кота в зависимости от статус-кода
                httpResponseMessage = httpClient.Send(new HttpRequestMessage(HttpMethod.Get, $"https://http.cat/{statusCode}.jpg"));
                //Добавление изображения в кэш, если его там нет
                if (!_cache.TryGetValue(statusCode, out coshka))
                {
                    _cache.Set(statusCode, httpResponseMessage, new MemoryCacheEntryOptions() { SlidingExpiration = new TimeSpan(0, 1, 30) });
                    return File(httpResponseMessage.Content.ReadAsStream(), "image/jpg");
                }

                return File(coshka, "image/jpg");
            }
            //При исключении - котик-404
            catch (Exception ex)
            {
                httpResponseMessage = httpClient.Send(new HttpRequestMessage(HttpMethod.Get, $"https://http.cat/404.jpg"));
                return File(httpResponseMessage.Content.ReadAsStream(), "image/jpg");
            }

        }
    }
}
