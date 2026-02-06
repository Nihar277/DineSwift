using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Repository.Model;
using Repository.Interfaces;

namespace API.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IUserHomeServices _userHomeServices;
        private readonly string _groqApiKey;
        private readonly string _groqApiUrl = "https://api.groq.com/openai/v1/chat/completions";

        public ChatController(IHttpClientFactory httpClientFactory, IConfiguration configuration, IUserHomeServices userHomeServices)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _userHomeServices = userHomeServices;
            _groqApiKey = _configuration["Groq:ApiKey"] ?? "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";
        }

        [HttpPost("chat")]
        [Consumes("application/json")]
        [Produces("text/html")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest(Content("Prompt is required", "text/plain", System.Text.Encoding.UTF8));
            }

            try
            {
                // Get all available dishes from database
                var allDishes = await _userHomeServices.getAllDishesforchat();
                
                if (allDishes == null || !allDishes.Any())
                {
                    return Content("No dishes available in database. Please check back later!", "text/html", System.Text.Encoding.UTF8);
                }
                
                // Debug: Show total dishes count
                var responseBuilder = new System.Text.StringBuilder();
                responseBuilder.AppendLine($"📊 Total dishes in database: {allDishes.Count}");
                responseBuilder.AppendLine();
                
                // Get valid restaurant IDs from database
                var validRestaurantIds = new List<int>();
                try
                {
                    validRestaurantIds = allDishes
                        .Where(d => d.c_restaurantid > 0)
                        .Select(d => d.c_restaurantid)
                        .Distinct()
                        .ToList();
                }
                catch (Exception ex)
                {
                    // Continue without restaurant IDs if there's an error
                }
                
                var availableDishes = allDishes.Where(d => d.c_isavailable).ToList();
                var dishesToShow = availableDishes.Any() ? availableDishes : allDishes;
                
                // Check if user is logged in (using static ID 137 for testing)
                var userId = 137; // Static user ID for testing
                bool isLoggedIn = userId > 0;

                // Check if the prompt contains dish keywords
                var promptLower = request.Prompt.ToLower();
                var dishKeywords = new List<string> 
                { 
                    "paneer", "chicken", "biryani", "tikka", "butter", "naan", "dosa", "salad", 
                    "pasta", "pizza", "burger", "noodles", "rice", "curry", "wrap", "sandwich",
                    "veg", "mushroom", "egg", "fish", "mutton", "prawn", "soup", "starter",
                    "dessert", "cake", "chocolate", "juice", "shake", "milk", "coffee", "tea",
                    "chinese", "italian", "mexican", "indian", "thai", "japanese", "continental",
                    "fast", "street", "breakfast", "snacks", "main", "course", "bbq", "grilled"
                };

                bool containsDishKeyword = dishKeywords.Any(keyword => promptLower.Contains(keyword));

                if (containsDishKeyword)
                {
                    // Find matching dishes based on user query
                    var matchingDishes = dishesToShow.Where(d => 
                    {
                        var dishNameLower = d.c_name.ToLower();
                        var dishCategoryLower = (d.c_category ?? "").ToLower();
                        
                        // Check if any keyword matches in dish name OR category
                        return dishKeywords.Any(keyword => 
                            promptLower.Contains(keyword) && 
                            (dishNameLower.Contains(keyword) || dishCategoryLower.Contains(keyword)));
                    }).ToList();

                    // If no matching dishes found, try more specific matching
                    if (!matchingDishes.Any())
                    {
                        // Try matching just the main keywords that are actually in the prompt
                        var keywordsInPrompt = dishKeywords.Where(keyword => promptLower.Contains(keyword)).ToList();
                        
                        matchingDishes = dishesToShow.Where(d => 
                        {
                            var dishNameLower = d.c_name.ToLower();
                            var dishCategoryLower = (d.c_category ?? "").ToLower();
                            
                            return keywordsInPrompt.Any(keyword => 
                                dishNameLower.Contains(keyword) || dishCategoryLower.Contains(keyword));
                        }).ToList();
                    }

                    // If still no matching dishes found, show proper message
                    if (!matchingDishes.Any())
                    {
                        responseBuilder.AppendLine($"Sorry, no dishes found matching \"{request.Prompt}\".");
                        responseBuilder.AppendLine();
                        responseBuilder.AppendLine("**Available dishes:**");
                        responseBuilder.AppendLine("🍽️ Veg Seekh Kebab - ₹209 (Indian)");
                        responseBuilder.AppendLine("🍽️ Greek Salad - ₹170 (Salad)");
                        responseBuilder.AppendLine("🍽️ Chilli Potato - ₹399 (Chinese)");
                        responseBuilder.AppendLine("🍽️ Chocolate Milk Shake - ₹399 (Desserts)");
                        responseBuilder.AppendLine();
                        responseBuilder.AppendLine("Try searching for: veg, salad, chinese, or dessert");
                        return Content(responseBuilder.ToString(), "text/html", System.Text.Encoding.UTF8);
                    }

                    // Get the main category from the first matching dish
                    var mainCategory = matchingDishes.First().c_category?.ToLower() ?? "";
                    
                    // Try to get 4 dishes from the same category
                    var categorySpecificDishes = dishesToShow
                        .Where(d => (d.c_category ?? "").ToLower() == mainCategory)
                        .Take(4)
                        .ToList();
                    
                    // If we don't have 4 dishes from the same category, use matching dishes + similar ones
                    if (categorySpecificDishes.Count < 4)
                    {
                        var additionalDishes = dishesToShow
                            .Where(d => !categorySpecificDishes.Any(cd => cd.c_itemid == d.c_itemid))
                            .Where(d => 
                            {
                                var dishNameLower = d.c_name.ToLower();
                                var dishCategoryLower = (d.c_category ?? "").ToLower();
                                
                                // Add dishes with similar keywords or category
                                return dishKeywords.Any(keyword => 
                                    (dishNameLower.Contains(keyword) || dishCategoryLower.Contains(keyword)) &&
                                    promptLower.Contains(keyword));
                            })
                            .Take(4 - categorySpecificDishes.Count)
                            .ToList();
                        
                        categorySpecificDishes.AddRange(additionalDishes);
                    }
                    
                    // If still less than 4, add popular dishes
                    if (categorySpecificDishes.Count < 4)
                    {
                        var popularDishes = dishesToShow
                            .Where(d => !categorySpecificDishes.Any(cd => cd.c_itemid == d.c_itemid))
                            .OrderByDescending(d => d.c_price)
                            .Take(4 - categorySpecificDishes.Count)
                            .ToList();
                        
                        categorySpecificDishes.AddRange(popularDishes);
                    }

                    if (categorySpecificDishes.Any())
                    {
                        responseBuilder.AppendLine("**Here are the dishes matching your query:**");
                        responseBuilder.AppendLine();
                        
                        foreach (var dish in categorySpecificDishes)
                        {
                            responseBuilder.AppendLine($"🍽️ **{dish.c_name}** - ₹{dish.c_price}");
                            if (isLoggedIn)
                            {
                                // responseBuilder.AppendLine($"   <a href='/api/chat/addToCart?ItemId={dish.c_itemid}' style='background-color: #4CAF50; color: white; padding: 8px 16px; text-decoration: none; border-radius: 4px; display: inline-block; margin: 2px;'>🛒 Add to Cart</a>");
                                
                                // Auto-add item to cart
                                try
                                {
                                    // Use valid restaurant ID or fallback to first available
                                    int restaurantIdToUse = validRestaurantIds.Contains(dish.c_restaurantid) ? 
                                        dish.c_restaurantid : 
                                        (validRestaurantIds.Any() ? validRestaurantIds.First() : 1);
                                    
                                    var autoCartItem = new Repository.Model.t_cart
                                    {
                                        c_customerId = 137,
                                        c_itemId = dish.c_itemid,
                                        c_quantity = 1,
                                        c_restaurantId = restaurantIdToUse,
                                        c_itemname = dish.c_name,
                                        c_price = dish.c_price,
                                        c_image = dish.c_imageurl ?? ""
                                    };
                                    
                                    bool autoResult = await _userHomeServices.AddToCart(autoCartItem);
                                    if (autoResult)
                                    {
                                        responseBuilder.AppendLine($"   ✅ Added to cart successfully");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    responseBuilder.AppendLine($"   ⚠️ Could not add to cart");
                                }
                            }
                            else
                            {
                                responseBuilder.AppendLine($"   🔒 Login required to add to cart");
                            }
                            responseBuilder.AppendLine();
                        }

                        return Content(responseBuilder.ToString(), "text/html", System.Text.Encoding.UTF8);
                    }
                }

                // Original chat logic for non-paneer queries
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_groqApiKey}");

                var groqRequest = new GroqRequest
                {
                    messages = new[]
                    {
                        new GroqMessage
                        {
                            role = "user",
                            content = request.Prompt
                        }
                    },
                    model = "llama-3.1-8b-instant",
                    temperature = 0.5,
                    max_tokens = 1024,
                    top_p = 1,
                    stop = null,
                    stream = false
                };

                var jsonContent = JsonSerializer.Serialize(groqRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(_groqApiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Response.StatusCode = (int)response.StatusCode;
                    return Content($"Error: {errorContent}", "text/plain", System.Text.Encoding.UTF8);
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                
                // Configure JSON options to handle property name casing
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var groqResponse = JsonSerializer.Deserialize<GroqResponse>(responseContent, jsonOptions);

                var chatResponse = groqResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? 
                    "Sorry, I couldn't generate a response. Please try again.";

                // Return plain text response instead of JSON
                return Content(chatResponse, "text/plain", System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Content($"Error: {ex.Message}", "text/plain", System.Text.Encoding.UTF8));
            }
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "ok", message = "Chat API is running" });
        }

        [HttpPost("addToCart")]
        [HttpGet("addToCart")]
        public async Task<IActionResult> AddToCartFromChat([FromQuery] int ItemId = 0, [FromBody] ChatCartRequest? request = null)
        {
            try
            {
                // Get ItemId from either query parameter or request body
                int itemId = ItemId > 0 ? ItemId : (request?.ItemId ?? 0);
                
                if (itemId <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid item data" });
                }

                // Get item details for display and cart creation
                string itemName = "Item";
                int itemPrice = 0;
                int restaurantId = 1;
                try
                {
                    var allDishes = await _userHomeServices.getAllDishes();
                    var item = allDishes.FirstOrDefault(d => d.c_itemid == itemId);
                    if (item != null)
                    {
                        itemName = item.c_name;
                        itemPrice = item.c_price;
                        // Try to get restaurant ID from item if available
                        restaurantId = item.c_restaurantid > 0 ? item.c_restaurantid : 1;
                    }
                }
                catch { /* Ignore if we can't get item details */ }

                // Create cart object for user ID 137 with all required fields
                var cart = new Repository.Model.t_cart
                {
                    c_customerId = 137, // Static user ID for testing
                    c_itemId = itemId,
                    c_quantity = 1, // Default quantity
                    c_restaurantId = restaurantId, // Restaurant ID from item or default
                    c_itemname = itemName, // Use actual item name
                    c_price = itemPrice, // Use actual item price
                    c_image = "" // Will be set by service if needed
                };

                // Try using the service method directly instead of API call
                var cartItem = new Repository.Model.t_cart
                {
                    c_customerId = 137, // Static user ID for testing
                    c_itemId = itemId,
                    c_quantity = 1, // Default quantity
                    c_restaurantId = restaurantId, // Restaurant ID from item or default
                    c_itemname = itemName, // Use actual item name
                    c_price = itemPrice, // Use actual item price
                    c_image = "" // Will be set by service if needed
                };

                // Call the service method directly
                bool result = await _userHomeServices.AddToCart(cartItem);
                
                // Debug: Log the cart object being sent
                System.Diagnostics.Debug.WriteLine($"=== CART SERVICE CALL ===");
                System.Diagnostics.Debug.WriteLine($"Cart Object:");
                System.Diagnostics.Debug.WriteLine($"  CustomerId: {cartItem.c_customerId}");
                System.Diagnostics.Debug.WriteLine($"  ItemId: {cartItem.c_itemId}");
                System.Diagnostics.Debug.WriteLine($"  Quantity: {cartItem.c_quantity}");
                System.Diagnostics.Debug.WriteLine($"  RestaurantId: {cartItem.c_restaurantId}");
                System.Diagnostics.Debug.WriteLine($"  ItemName: {cartItem.c_itemname}");
                System.Diagnostics.Debug.WriteLine($"  Price: {cartItem.c_price}");
                System.Diagnostics.Debug.WriteLine($"Service Result: {result}");
                System.Diagnostics.Debug.WriteLine($"===================");
                
                // Also check if we can retrieve cart items for this customer to verify
                try
                {
                    var cartItems = await _userHomeServices.GetCartItems(cartItem.c_customerId);
                    System.Diagnostics.Debug.WriteLine($"=== CART ITEMS FOR CUSTOMER {cartItem.c_customerId} ===");
                    System.Diagnostics.Debug.WriteLine($"Total items in cart: {cartItems?.Count ?? 0}");
                    if (cartItems != null)
                    {
                        foreach (var item in cartItems)
                        {
                            System.Diagnostics.Debug.WriteLine($"  - {item.c_itemname} (ID: {item.c_itemId}, Price: ₹{item.c_price})");
                        }
                    }
                    System.Diagnostics.Debug.WriteLine($"======================================");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error getting cart items: {ex.Message}");
                }

                // Create a simple success response
                var responseContent = result ? 
                    "{\"success\":true,\"message\":\"Item added to cart\"}" : 
                    "{\"success\":false,\"message\":\"Item already in cart\"}";

                // Check if the service returned success
                bool isSuccess = result;
                
                if (!isSuccess)
                {
                    // Return HTML response for GET requests - redirect back to chat
                    if (ItemId > 0)
                    {
                        return Content("<html><body><script>alert('⚠️ " + itemName + " already in cart!'); window.location.href='/Chat/Index';</script></body></html>", "text/html");
                    }
                    return Ok(new { success = false, message = "Item already in cart" });
                }

                // Return HTML response for GET requests - redirect back to chat
                if (ItemId > 0)
                {
                    return Content("<html><body><script>alert('✅ " + itemName + " added to cart successfully!'); window.location.href='/Chat/Index';</script></body></html>", "text/html");
                }

                return Ok(new { success = true, message = "Item added to cart successfully!" });
            }
            catch (Exception ex)
            {
                // Return HTML response for GET requests - redirect back to chat
                if (ItemId > 0)
                {
                    return Content("<html><body><script>alert('❌ Error adding to cart: " + ex.Message.Replace("'", "\\'") + "'); window.location.href='/Chat/Index';</script></body></html>", "text/html");
                }
                return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }

    public class ChatRequest
    {
        public string Prompt { get; set; } = string.Empty;
    }

    public class ChatCartRequest
    {
        public int ItemId { get; set; }
    }

    public class GroqRequest
    {
        public GroqMessage[] messages { get; set; }
        public string model { get; set; }
        public double temperature { get; set; }
        public int max_tokens { get; set; }
        public double top_p { get; set; }
        public string? stop { get; set; }
        public bool stream { get; set; }
    }

    public class GroqMessage
    {
        public string role { get; set; }
        public string content { get; set; }
    }

    public class GroqResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }
    }

    public class Choice
    {
        [JsonPropertyName("message")]
        public Message? Message { get; set; }
    }

    public class Message
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}

