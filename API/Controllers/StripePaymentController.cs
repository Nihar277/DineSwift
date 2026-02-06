using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Repository.Model;
using Repository.Interfaces;
using API.service;
using System.Text;
using System.Text.Json;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StripePaymentController : ControllerBase
    {
        private readonly IOrderInterface _order;
        private readonly IUserHomeServices _cartService;
        private readonly HttpClient _httpClient;

        public StripePaymentController(
            IOrderInterface order,
            IUserHomeServices cartService,
            IHttpClientFactory httpClientFactory)
        {
            _order = order;
            _cartService = cartService;
            _httpClient = httpClientFactory.CreateClient();
        }

        // ======================================================
        // STEP 1: CREATE STRIPE CHECKOUT SESSION
        // ======================================================
        [HttpPost("create-checkout-session")]
        public IActionResult CreateCheckoutSession([FromBody] StripePaymentDto dto)
        {
            if (dto.Amount < 10000)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Minimum payment amount is ₹100"
                });
            }

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                Mode = "payment",

                Metadata = new Dictionary<string, string>
                {
                    { "c_customerid", dto.c_customerid.ToString() },
                    { "c_restaurantid", dto.c_restaurantid.ToString() },
                    { "c_housenumber", dto.c_housenumber },
                    { "c_societyname", dto.c_societyname },
                    { "c_landmark", dto.c_landmark ?? "" },
                    { "c_city", dto.c_city },
                    { "c_state", dto.c_state }
                },

                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = dto.Currency,
                            UnitAmount = dto.Amount,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "DineSwift Food Order"
                            }
                        },
                        Quantity = 1
                    }
                },

                SuccessUrl =
                    "http://localhost:5245/api/StripePayment/payment-success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = "http://localhost:3002/payment-cancel"
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Ok(new
            {
                success = true,
                checkoutUrl = session.Url
            });
        }

        // ======================================================
        // STEP 2: PAYMENT SUCCESS → VERIFY → DB → EMAIL
        // ======================================================
        [HttpGet("payment-success")]
        public async Task<IActionResult> PaymentSuccess([FromQuery] string session_id)
        {
            if (string.IsNullOrWhiteSpace(session_id))
                return BadRequest("Invalid Stripe session");

            var stripeService = new SessionService();
            Session session;

            try
            {
                session = stripeService.Get(session_id);
            }
            catch
            {
                return BadRequest("Stripe verification failed");
            }

            if (session.PaymentStatus != "paid")
                return BadRequest("Payment not completed");

            // ================================
            // 1️⃣ Extract metadata
            // ================================
            int customerId = int.Parse(session.Metadata["c_customerid"]);
            int restaurantId = int.Parse(session.Metadata["c_restaurantid"]);

            // ================================
            // 2️⃣ Fetch cart items
            // ================================
            var cartItems = await _cartService.GetCartItems(customerId);

            if (cartItems == null || !cartItems.Any())
                return BadRequest("Cart is empty");

            // ================================
            // 3️⃣ Build PlaceOrderRequest
            // ================================
            var placeOrderRequest = new PlaceOrderRequest
            {
                OrderId = Guid.NewGuid().ToString(),
                CustomerEmail = session.CustomerDetails?.Email ?? "",
                CustomerName = session.CustomerDetails?.Name ?? "Customer",
                RestaurantName = "Restaurant #" + restaurantId,

                DeliveryAddress =
                    $"{session.Metadata["c_housenumber"]}, " +
                    $"{session.Metadata["c_societyname"]}, " +
                    $"{session.Metadata["c_landmark"]}, " +
                    $"{session.Metadata["c_city"]}, " +
                    $"{session.Metadata["c_state"]}",

                PaymentMethod = "Stripe",
                OrderDate = DateTime.UtcNow,
                EstimatedDeliveryMinutes = 30,
                Items = new List<OrderItem>()
            };

            decimal subTotal = 0;

            foreach (var item in cartItems)
            {
                var orderItem = new OrderItem
                {
                    ItemName = item.c_itemname,
                    Quantity = item.c_quantity,
                    Price = item.c_price
                };

                placeOrderRequest.Items.Add(orderItem);
                subTotal += orderItem.Total;
            }

            placeOrderRequest.SubTotal = subTotal;
            placeOrderRequest.DeliveryFee = 40;
            placeOrderRequest.Tax = subTotal * 0.05m;
            placeOrderRequest.Total =
                placeOrderRequest.SubTotal +
                placeOrderRequest.DeliveryFee +
                placeOrderRequest.Tax;

            // ================================
            // 4️⃣ Save orders in DB
            // ================================
            foreach (var item in cartItems)
            {
                var order = new t_order
                {
                    c_customerid = customerId,
                    c_restaurantid = restaurantId,
                    c_housenumber = session.Metadata["c_housenumber"],
                    c_societyname = session.Metadata["c_societyname"],
                    c_landmark = session.Metadata["c_landmark"],
                    c_city = session.Metadata["c_city"],
                    c_state = session.Metadata["c_state"],
                    c_dishname = item.c_itemname,
                    c_foodimage = item.c_image,
                    c_quantity = item.c_quantity,
                    c_totalprice = item.c_price * item.c_quantity,
                    c_fooditemid = item.c_itemId,
                    c_orderdate = DateTime.Now
                };

                await _order.CreateOrderAsync(order);
            }

            // ================================
            // 5️⃣ CALL OTP ORDER PLACE API (JSON)
            // ================================
            var json = JsonSerializer.Serialize(placeOrderRequest);

            var httpContent = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(
                "http://localhost:5245/api/Otp/order/place",
                httpContent
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Order email failed: " + error);
            }

            // ================================
            // 6️⃣ Redirect user
            // ================================
            return Redirect("http://localhost:5030/OrderHistory/Orders");
        }
    }
}