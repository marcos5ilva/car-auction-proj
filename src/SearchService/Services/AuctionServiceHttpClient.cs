using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services;

public class AuctionServiceHttpClient
{
    private HttpClient _httpClient;
    private IConfiguration _config;

    public AuctionServiceHttpClient(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<List<Item>> GetItemsForSearchDb()
    {
        var lastUpdate = await DB.Find<Item, string>()
            .Sort(x => x.UpdatedAt, Order.Descending)
            .Project( x => x.UpdatedAt.ToString())
            .ExecuteFirstAsync();

        if (lastUpdate == null)
        {
            // Handle the case when no items are found in the database
            return new List<Item>();
        }
        
        return await _httpClient.GetFromJsonAsync<List<Item>>(_config["AuctionServiceUrl"] + "/api/auctions?date=" +
                                                              lastUpdate);
    }
}