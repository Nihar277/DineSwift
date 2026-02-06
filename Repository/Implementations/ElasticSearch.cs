using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nest;
using Repository.Implementation;
using Repository.Interfaces;
using Repository.Model;

namespace Repository.service
{
    public class ElasticSearch : IElasticSearch
    {
        private readonly IElasticClient _elasticClient;

        public ElasticSearch(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public async Task CreateFoodItemIndexAsync()
        {
            var indexExistsResponse = await _elasticClient.Indices.ExistsAsync("fooditems");
            if (!indexExistsResponse.Exists)
            {
                var createIndexResponse = await _elasticClient.Indices.CreateAsync("fooditems", c => c
               .Map<t_fooditem>(t => t.AutoMap()));

                Console.WriteLine("Food Item index created.");

                if (!createIndexResponse.IsValid)
                {
                    throw new Exception($"Failed to create index: {createIndexResponse.DebugInformation}");
                }
            }
            else
            {
                Console.WriteLine("Food Item index already exists.");
                // return 0;
            }

        }

        public async Task addFoodItems(t_fooditem t_Fooditem)
        {
            var indexResponse = await _elasticClient.IndexAsync(t_Fooditem, i => i
                .Id(t_Fooditem.c_itemid) // Use the exact c_cityid from PostgreSQL as the Elasticsearch document ID
                .Index("fooditems")
            );
        }

        public async Task UpdateFoodItemsPartial(t_fooditem updateFields)
        {
            var response = await _elasticClient.UpdateAsync<t_fooditem>(updateFields.c_itemid, u => u
                .Index("fooditems")
                .Doc(updateFields)
            );
        }


        public async Task<List<t_fooditem>> GetFooditemByRestaurantAsync(int restaurantId)
        {
            var response = await _elasticClient.SearchAsync<t_fooditem>(s => s
                .Index("fooditems")
                .Size(10000)
                .Query(q => q
                    .Term(t => t
                        .Field(f => f.c_restaurantid)
                        .Value(restaurantId)
                    )
                )
                .Sort(srt => srt
                    .Field(f => f.c_itemid, SortOrder.Ascending)
                )
            );

            return response.Documents.ToList();

        }



        public async Task<List<t_fooditem>> SearchFoodItemNameAsync(string searchTerm, int restaurantId)
        {
            var response = await _elasticClient.SearchAsync<t_fooditem>(s => s
                .Index("fooditems")
                .Query(q => q
                    .Bool(b => b

                        .Must(m => m
                            .Term(t => t
                                .Field(f => f.c_restaurantid)
                                .Value(restaurantId)
                            )
                        )
                        .Should(
                            // 1️⃣ Case-insensitive full-text search, this for single match
                            sh => sh.Match(m => m
                                .Field(f => f.c_name)
                                .Query(searchTerm)
                            ),

                            // 1️⃣ Case-insensitive full-text search, this for multiple match
                            sh => sh.MultiMatch(m => m
                                 .Fields(f => f
                                    .Field(f => f.c_name)
                                    .Field(f => f.c_ingredients)
                                 )
                                .Query(searchTerm)
                            ),


                             // 2️⃣ Autocomplete (match starting letters) for single search
                             // sh => sh.MatchPhrasePrefix(m => m
                             //     .Field(f => f.c_name)
                             //     .Query(searchTerm)
                             // ),

                             sh => sh.MultiMatch(mm => mm // for multiple search
                                .Fields(f => f
                                    .Field(p => p.c_name)
                                    .Field(p => p.c_ingredients)
                                )
                                .Query(searchTerm)
                                .Type(TextQueryType.PhrasePrefix)
                            ),


                               // 3️⃣ Fuzzy match (handle spelling mistakes), for single search
                               // sh => sh.Fuzzy(fz => fz
                               //     .Field(f => f.c_name)
                               //     // .Field(f => f.category)
                               //     .Value(searchTerm)
                               //     .Fuzziness(Fuzziness.Auto)
                               // )

                               sh => sh.MultiMatch(mm => mm  //for multi field search
                                    .Fields(f => f
                                        .Field(p => p.c_name)
                                        .Field(p => p.c_ingredients)
                                    )
                                    .Query(searchTerm)
                                    .Fuzziness(Fuzziness.Auto)
                                )
                        )
                        .MinimumShouldMatch(1)
                    )
                )
            );


            return response.IsValid ? response.Documents.ToList() : new List<t_fooditem>();
        }


        public async Task<List<t_fooditem>> SearchAllFoodItemNameAsync(string searchTerm)
        {
            var response = await _elasticClient.SearchAsync<t_fooditem>(s => s
                .Index("fooditems")
                .Query(q => q
                    .Bool(b => b
                        .Should(
                            // 1️⃣ Case-insensitive full-text search, this for single match
                            sh => sh.Match(m => m
                                .Field(f => f.c_name)
                                .Query(searchTerm)
                            ),

                            // 1️⃣ Case-insensitive full-text search, this for multiple match
                            sh => sh.MultiMatch(m => m
                                 .Fields(f => f
                                    .Field(f => f.c_name)
                                 // .Field(f => f.c_ingredients)
                                 )
                                .Query(searchTerm)
                            ),


                             // 2️⃣ Autocomplete (match starting letters) for single search
                             // sh => sh.MatchPhrasePrefix(m => m
                             //     .Field(f => f.c_name)
                             //     .Query(searchTerm)
                             // ),

                             sh => sh.MultiMatch(mm => mm // for multiple search
                                .Fields(f => f
                                    .Field(p => p.c_name)
                                // .Field(p => p.c_ingredients)
                                )
                                .Query(searchTerm)
                                .Type(TextQueryType.PhrasePrefix)
                            ),


                               // 3️⃣ Fuzzy match (handle spelling mistakes), for single search
                               // sh => sh.Fuzzy(fz => fz
                               //     .Field(f => f.c_name)
                               //     // .Field(f => f.category)
                               //     .Value(searchTerm)
                               //     .Fuzziness(Fuzziness.Auto)
                               // )

                               sh => sh.MultiMatch(mm => mm  //for multi field search
                                    .Fields(f => f
                                        .Field(p => p.c_name)
                                    // .Field(p => p.c_ingredients)
                                    )
                                    .Query(searchTerm)
                                    .Fuzziness(Fuzziness.Auto)
                                )
                        )
                        .MinimumShouldMatch(1)
                    )
                )
            );


            return response.IsValid ? response.Documents.ToList() : new List<t_fooditem>();
        }

        public async Task CreateRestaurantIndexAsync()
        {
            var indexExistsResponse = await _elasticClient.Indices.ExistsAsync("restaurant");
            if (!indexExistsResponse.Exists)
            {
                var createIndexResponse = await _elasticClient.Indices.CreateAsync("restaurant", c => c
               .Map<t_vm_restaurant>(t => t.AutoMap()));

                Console.WriteLine("Restaurant index created.");

                if (!createIndexResponse.IsValid)
                {
                    throw new Exception($"Failed to create index: {createIndexResponse.DebugInformation}");
                }
            }
            else
            {
                Console.WriteLine("Restaurant index already exists.");
                // return 0;
            }
        }

        public async Task addRestaurant(t_vm_restaurant restaurant)
        {
            var indexResponse = await _elasticClient.IndexAsync(restaurant, i => i
               .Id(restaurant.c_r_id) // Use the exact c_cityid from PostgreSQL as the Elasticsearch document ID
               .Index("restaurant")
           );
        }

        public async Task<List<t_vm_restaurant>> SearchRestaurantNameAsync(string searchTerm)
        {
            var response = await _elasticClient.SearchAsync<t_vm_restaurant>(s => s
                .Index("restaurant")
                .Query(q => q
                    .Bool(b => b
                        .Should(
                            // 1️⃣ Case-insensitive full-text search, this for single match
                            sh => sh.Match(m => m
                                .Field(f => f.c_r_name)
                                .Query(searchTerm)
                            ),

                            // 1️⃣ Case-insensitive full-text search, this for multiple match
                            sh => sh.MultiMatch(m => m
                                 .Fields(f => f
                                    .Field(f => f.c_r_name)
                                 // .Field(f => f.c_ingredients)
                                 )
                                .Query(searchTerm)
                            ),


                             // 2️⃣ Autocomplete (match starting letters) for single search
                             // sh => sh.MatchPhrasePrefix(m => m
                             //     .Field(f => f.c_name)
                             //     .Query(searchTerm)
                             // ),

                             sh => sh.MultiMatch(mm => mm // for multiple search
                                .Fields(f => f
                                    .Field(p => p.c_r_name)
                                // .Field(p => p.c_ingredients)
                                )
                                .Query(searchTerm)
                                .Type(TextQueryType.PhrasePrefix)
                            ),


                               // 3️⃣ Fuzzy match (handle spelling mistakes), for single search
                               // sh => sh.Fuzzy(fz => fz
                               //     .Field(f => f.c_name)
                               //     // .Field(f => f.category)
                               //     .Value(searchTerm)
                               //     .Fuzziness(Fuzziness.Auto)
                               // )

                               sh => sh.MultiMatch(mm => mm  //for multi field search
                                    .Fields(f => f
                                        .Field(p => p.c_r_name)
                                    // .Field(p => p.c_ingredients)
                                    )
                                    .Query(searchTerm)
                                    .Fuzziness(Fuzziness.Auto)
                                )
                        )
                        .MinimumShouldMatch(1)
                    )
                )
            );


            return response.IsValid ? response.Documents.ToList() : new List<t_vm_restaurant>();
        }
    }
}