using DataManager.Options;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataManager.Services
{
    public class CosmosDbService
    {
        private readonly CosmosDbOptions _cosmosDbOptions;
        private readonly DocumentClient _documentClient;

        private const int DefaultRu = 400;

        public CosmosDbService(IOptions<CosmosDbOptions> cosmosDbOptions)
        {
            _cosmosDbOptions = cosmosDbOptions.Value;
            _documentClient = new DocumentClient(new Uri(_cosmosDbOptions.Endpoint), _cosmosDbOptions.PrimaryKey);

            InitializeAsync().Wait();
        }

        private async Task InitializeAsync()
        {
            await _documentClient.CreateDatabaseIfNotExistsAsync(new Database { Id = _cosmosDbOptions.Database });
        }

        public async Task<IEnumerable<T>> ReadAllAsync<T>(string collection)
        {
            var databaseLink = UriFactory.CreateDatabaseUri(_cosmosDbOptions.Database);
            await _documentClient.CreateDocumentCollectionIfNotExistsAsync(databaseLink,
                 new DocumentCollection { Id = collection },
                 new RequestOptions { OfferThroughput = DefaultRu });

            var collectionLink = UriFactory.CreateDocumentCollectionUri(_cosmosDbOptions.Database, collection);
            var feedOptions = new FeedOptions { MaxItemCount = -1 };
            return _documentClient.CreateDocumentQuery<T>(collectionLink, feedOptions);
        }
    }
}
