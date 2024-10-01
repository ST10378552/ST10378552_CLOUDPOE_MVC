using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ABCRetailApp.Services;
using ABCRetailApp.Models;
using System.Threading.Tasks;
using System.Net.Http.Headers;

public class FilesController : Controller
{
    private readonly FileStorageService _fileStorageService;
    private readonly HttpClient _httpClient;

    public FilesController(FileStorageService fileStorageService, HttpClient httpClient)
    {
        _fileStorageService = fileStorageService;
        _httpClient = httpClient;
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        /*if (file != null && file.Length > 0)
        {
            using (var stream = file.OpenReadStream())
            {
                await _fileStorageService.UploadFileAsync(file.FileName, stream);
            }
        }*/
        if (file != null && file.Length > 0)
        {
            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream)
            {
                Headers = { ContentType = new MediaTypeHeaderValue(file.ContentType) }
            };
            content.Add(fileContent, "file", file.FileName);

            var response = await _httpClient.PostAsync("https://st10378552cloud.azurewebsites.net/api/UploadFile?", content);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                // Handle error response
                return BadRequest("Error uploading file.");
            }
        }
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Download(string fileName)
    {
        var fileStream = await _fileStorageService.DownloadFileAsync(fileName);
        return File(fileStream, "application/octet-stream", fileName);
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var files = await _fileStorageService.ListFilesAsync();
        return View(files);
    }
}
